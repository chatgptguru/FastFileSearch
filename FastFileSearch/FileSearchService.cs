using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FastFileSearch
{
    public class FileSearchService
    {
        private readonly ConcurrentDictionary<string, List<FileSearchResult>> _fileIndex = new();
        private readonly SemaphoreSlim _indexingSemaphore = new(1, 1);
        private bool _isIndexing = false;
        private CancellationTokenSource _indexingCancellation;
        private readonly object _lockObject = new object();

        public event EventHandler<SearchProgressEventArgs> SearchProgress;
        public event EventHandler<SearchCompletedEventArgs> SearchCompleted;
        public event EventHandler<IndexingStatusEventArgs> IndexingStatusChanged;

        public bool IsIndexing => _isIndexing;
        public int TotalIndexedFiles => _fileIndex.Values.Sum(list => list.Count);

        public async Task<List<FileSearchResult>> SearchAsync(string query, string rootPath = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<FileSearchResult>();

            var results = new List<FileSearchResult>();

            // If indexing is complete, search in index for much faster results
            if (!_isIndexing && _fileIndex.Count > 0)
            {
                results = await SearchInIndexAsync(query, cancellationToken);
            }
            else
            {
                // Fallback to real-time search if index is not ready
                results = await SearchInRealTimeAsync(query, rootPath, cancellationToken);
            }

            // Sort by relevance (exact matches first, then contains)
            results = results
                .OrderBy(r => !Path.GetFileNameWithoutExtension(r.FileName).Equals(query, StringComparison.OrdinalIgnoreCase))
                .ThenBy(r => !r.FileName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ThenBy(r => r.FileName)
                .Take(1000) // Limit results
                .ToList();

            SearchCompleted?.Invoke(this, new SearchCompletedEventArgs(results, query));
            return results;
        }

        private async Task<List<FileSearchResult>> SearchInIndexAsync(string query, CancellationToken cancellationToken)
        {
            var results = new List<FileSearchResult>();

            await Task.Run(() =>
            {
                foreach (var driveFiles in _fileIndex.Values)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var matchingFiles = driveFiles
                        .Where(f => f.FileName.Contains(query, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    results.AddRange(matchingFiles);
                }
            }, cancellationToken);

            return results;
        }

        private async Task<List<FileSearchResult>> SearchInRealTimeAsync(string query, string rootPath, CancellationToken cancellationToken)
        {
            var results = new List<FileSearchResult>();
            var searchPaths = new List<string>();

            if (!string.IsNullOrEmpty(rootPath) && Directory.Exists(rootPath))
            {
                searchPaths.Add(rootPath);
            }
            else
            {
                // Search in all available drives
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                    .Select(d => d.RootDirectory.FullName)
                    .ToList();

                searchPaths.AddRange(drives);
            }

            var tasks = searchPaths.Select(path => SearchInPathAsync(query, path, cancellationToken)).ToArray();
            var pathResults = await Task.WhenAll(tasks);

            foreach (var pathResult in pathResults)
            {
                results.AddRange(pathResult);
            }

            return results;
        }

        private async Task<List<FileSearchResult>> SearchInPathAsync(string query, string searchPath, CancellationToken cancellationToken)
        {
            var results = new List<FileSearchResult>();
            
            try
            {
                if (!Directory.Exists(searchPath))
                    return results;

                await Task.Run(() =>
                {
                    var enumOptions = new EnumerationOptions
                    {
                        RecurseSubdirectories = true,
                        IgnoreInaccessible = true,
                        MaxRecursionDepth = 10
                    };

                    var files = Directory.EnumerateFiles(searchPath, "*", enumOptions)
                        .Where(f => Path.GetFileName(f).Contains(query, StringComparison.OrdinalIgnoreCase))
                        .Take(250); // Limit per path

                    foreach (var file in files)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        try
                        {
                            var fileInfo = new FileInfo(file);
                            results.Add(new FileSearchResult
                            {
                                FullPath = file,
                                FileName = fileInfo.Name,
                                Directory = fileInfo.DirectoryName,
                                Size = fileInfo.Length,
                                Modified = fileInfo.LastWriteTime,
                                Extension = fileInfo.Extension
                            });
                        }
                        catch
                        {
                            // Skip files that can't be accessed
                        }
                    }

                    // Also search directories
                    var directories = Directory.EnumerateDirectories(searchPath, "*", enumOptions)
                        .Where(d => Path.GetFileName(d).Contains(query, StringComparison.OrdinalIgnoreCase))
                        .Take(50); // Limit directories

                    foreach (var dir in directories)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        try
                        {
                            var dirInfo = new DirectoryInfo(dir);
                            results.Add(new FileSearchResult
                            {
                                FullPath = dir,
                                FileName = dirInfo.Name,
                                Directory = dirInfo.Parent?.FullName,
                                Size = 0,
                                Modified = dirInfo.LastWriteTime,
                                Extension = "[Folder]",
                                IsDirectory = true
                            });
                        }
                        catch
                        {
                            // Skip directories that can't be accessed
                        }
                    }

                }, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log error but continue with other paths
                System.Diagnostics.Debug.WriteLine($"Search error in {searchPath}: {ex.Message}");
            }

            return results;
        }

        public async Task BuildFullIndexAsync(CancellationToken cancellationToken = default)
        {
            await _indexingSemaphore.WaitAsync(cancellationToken);
            try
            {
                lock (_lockObject)
                {
                    _indexingCancellation?.Cancel();
                    _indexingCancellation = new CancellationTokenSource();
                    _isIndexing = true;
                }

                IndexingStatusChanged?.Invoke(this, new IndexingStatusEventArgs(true, "Starting indexing..."));

                _fileIndex.Clear();

                // Get all fixed drives
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                    .ToList();

                IndexingStatusChanged?.Invoke(this, new IndexingStatusEventArgs(true, $"Found {drives.Count} drives to index"));

                int driveIndex = 0;
                foreach (var drive in drives)
                {
                    if (cancellationToken.IsCancellationRequested || _indexingCancellation.Token.IsCancellationRequested)
                        break;

                    driveIndex++;
                    IndexingStatusChanged?.Invoke(this, new IndexingStatusEventArgs(true, $"Indexing drive {drive.Name} ({driveIndex}/{drives.Count})"));

                    await IndexDriveAsync(drive.RootDirectory.FullName, 
                        CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _indexingCancellation.Token).Token);
                }

                if (!cancellationToken.IsCancellationRequested && !_indexingCancellation.Token.IsCancellationRequested)
                {
                    IndexingStatusChanged?.Invoke(this, new IndexingStatusEventArgs(false, $"Indexing complete! {TotalIndexedFiles:N0} files indexed"));
                }
                else
                {
                    IndexingStatusChanged?.Invoke(this, new IndexingStatusEventArgs(false, "Indexing cancelled"));
                }
            }
            finally
            {
                lock (_lockObject)
                {
                    _isIndexing = false;
                    _indexingCancellation?.Dispose();
                    _indexingCancellation = null;
                }
                _indexingSemaphore.Release();
            }
        }

        private async Task IndexDriveAsync(string drivePath, CancellationToken cancellationToken)
        {
            try
            {
                var driveFiles = new List<FileSearchResult>();
                int fileCount = 0;

                await Task.Run(() =>
                {
                    var enumOptions = new EnumerationOptions
                    {
                        RecurseSubdirectories = true,
                        IgnoreInaccessible = true,
                        MaxRecursionDepth = 20
                    };

                    // Index files
                    foreach (var file in Directory.EnumerateFiles(drivePath, "*", enumOptions))
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        try
                        {
                            var fileInfo = new FileInfo(file);
                            driveFiles.Add(new FileSearchResult
                            {
                                FullPath = file,
                                FileName = fileInfo.Name,
                                Directory = fileInfo.DirectoryName,
                                Size = fileInfo.Length,
                                Modified = fileInfo.LastWriteTime,
                                Extension = fileInfo.Extension,
                                IsDirectory = false
                            });

                            fileCount++;
                            
                            // Report progress every 1000 files
                            if (fileCount % 1000 == 0)
                            {
                                SearchProgress?.Invoke(this, new SearchProgressEventArgs
                                {
                                    FilesProcessed = fileCount,
                                    CurrentPath = fileInfo.DirectoryName
                                });
                            }
                        }
                        catch
                        {
                            // Skip files that can't be accessed
                        }
                    }

                    // Index directories
                    foreach (var directory in Directory.EnumerateDirectories(drivePath, "*", enumOptions))
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        try
                        {
                            var dirInfo = new DirectoryInfo(directory);
                            driveFiles.Add(new FileSearchResult
                            {
                                FullPath = directory,
                                FileName = dirInfo.Name,
                                Directory = dirInfo.Parent?.FullName,
                                Size = 0,
                                Modified = dirInfo.LastWriteTime,
                                Extension = "[Folder]",
                                IsDirectory = true
                            });
                        }
                        catch
                        {
                            // Skip directories that can't be accessed
                        }
                    }

                }, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    _fileIndex[drivePath] = driveFiles;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Index error in drive {drivePath}: {ex.Message}");
            }
        }

        public void CancelIndexing()
        {
            lock (_lockObject)
            {
                _indexingCancellation?.Cancel();
            }
        }

        public void Dispose()
        {
            CancelIndexing();
            _indexingSemaphore?.Dispose();
            _indexingCancellation?.Dispose();
        }
    }

    public class FileSearchResult
    {
        public string FullPath { get; set; }
        public string FileName { get; set; }
        public string Directory { get; set; }
        public long Size { get; set; }
        public DateTime Modified { get; set; }
        public string Extension { get; set; }
        public bool IsDirectory { get; set; }
    }

    public class SearchProgressEventArgs : EventArgs
    {
        public int FilesProcessed { get; set; }
        public string CurrentPath { get; set; }
    }

    public class SearchCompletedEventArgs : EventArgs
    {
        public List<FileSearchResult> Results { get; set; }
        public string Query { get; set; }

        public SearchCompletedEventArgs(List<FileSearchResult> results, string query)
        {
            Results = results;
            Query = query;
        }
    }

    public class IndexingStatusEventArgs : EventArgs
    {
        public bool IsIndexing { get; set; }
        public string StatusMessage { get; set; }

        public IndexingStatusEventArgs(bool isIndexing, string statusMessage)
        {
            IsIndexing = isIndexing;
            StatusMessage = statusMessage;
        }
    }
}