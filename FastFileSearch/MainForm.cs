using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FastFileSearch
{
    public partial class MainForm : Form
    {
        private FileSearchService _searchService;
        private CancellationTokenSource _searchCancellation;
        private System.Windows.Forms.Timer _searchTimer;
        private ImageList _imageList;
        private Dictionary<string, int> _iconCache;
        private bool _isInitialIndexingComplete = false;

        public MainForm()
        {
            InitializeComponent();
            InitializeServices();
            SetupUI();
            StartInitialIndexing();
        }

        private void InitializeServices()
        {
            _searchService = new FileSearchService();
            _searchService.SearchCompleted += OnSearchCompleted;
            _searchService.SearchProgress += OnSearchProgress;
            _searchService.IndexingStatusChanged += OnIndexingStatusChanged;
            _iconCache = new Dictionary<string, int>();
            
            _searchTimer = new System.Windows.Forms.Timer();
            _searchTimer.Interval = 300; // 300ms delay
            _searchTimer.Tick += OnSearchTimerTick;
        }

        private void SetupUI()
        {
            // Setup ImageList for icons
            _imageList = new ImageList();
            _imageList.ImageSize = new Size(32, 32);
            _imageList.ColorDepth = ColorDepth.Depth32Bit;
            
            listViewResults.SmallImageList = _imageList;
            listViewResults.LargeImageList = _imageList;
            
            // Setup ListView
            listViewResults.View = View.Details;
            listViewResults.FullRowSelect = true;
            listViewResults.GridLines = true;
            listViewResults.MultiSelect = true;
            
            // Add columns
            listViewResults.Columns.Add("Name", 250);
            listViewResults.Columns.Add("Path", 350);
            listViewResults.Columns.Add("Size", 80);
            listViewResults.Columns.Add("Modified", 120);
            listViewResults.Columns.Add("Type", 100);
            
            // Events
            textBoxSearch.TextChanged += OnSearchTextChanged;
            listViewResults.DoubleClick += OnResultDoubleClick;
            listViewResults.MouseClick += OnResultMouseClick;
            buttonReindex.Click += OnReindexClick;
            buttonCancelIndex.Click += OnCancelIndexClick;
            
            // Initially hide cancel button
            buttonCancelIndex.Visible = false;
            
            // Focus on search box
            textBoxSearch.Focus();
        }

        private async void StartInitialIndexing()
        {
            try
            {
                await _searchService.BuildFullIndexAsync();
                _isInitialIndexingComplete = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during initial indexing: {ex.Message}", "Indexing Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void OnReindexClick(object sender, EventArgs e)
        {
            try
            {
                await _searchService.BuildFullIndexAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during reindexing: {ex.Message}", "Reindex Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnCancelIndexClick(object sender, EventArgs e)
        {
            _searchService.CancelIndexing();
        }

        private void OnIndexingStatusChanged(object sender, IndexingStatusEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnIndexingStatusChanged(sender, e)));
                return;
            }

            if (e.IsIndexing)
            {
                labelIndexingStatus.Text = e.StatusMessage;
                labelIndexingStatus.Visible = true;
                progressBarIndexing.Visible = true;
                progressBarIndexing.Style = ProgressBarStyle.Marquee;
                buttonReindex.Enabled = false;
                buttonCancelIndex.Visible = true;
                buttonCancelIndex.Enabled = true;
            }
            else
            {
                labelIndexingStatus.Text = e.StatusMessage;
                progressBarIndexing.Visible = false;
                buttonReindex.Enabled = true;
                buttonCancelIndex.Visible = false;
                
                // Show indexing status for a few seconds
                var timer = new System.Windows.Forms.Timer();
                timer.Interval = 3000;
                timer.Tick += (s, args) =>
                {
                    labelIndexingStatus.Visible = false;
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
        }

        private void OnSearchProgress(object sender, SearchProgressEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnSearchProgress(sender, e)));
                return;
            }

            labelIndexingStatus.Text = $"Indexing... {e.FilesProcessed:N0} files processed";
        }

        private void OnSearchTextChanged(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private async void OnSearchTimerTick(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            await PerformSearch();
        }

        private async Task PerformSearch()
        {
            string query = textBoxSearch.Text.Trim();
            
            if (string.IsNullOrEmpty(query))
            {
                listViewResults.Items.Clear();
                toolStripStatusLabel.Text = "Enter search terms...";
                return;
            }

            if (query.Length < 2)
            {
                toolStripStatusLabel.Text = "Search query too short...";
                return;
            }

            // Cancel previous search
            _searchCancellation?.Cancel();
            _searchCancellation = new CancellationTokenSource();

            try
            {
                toolStripStatusLabel.Text = _searchService.IsIndexing ? "Searching (indexing in progress)..." : "Searching...";
                progressBar.Style = ProgressBarStyle.Marquee;
                progressBar.Visible = true;

                var results = await _searchService.SearchAsync(query, null, _searchCancellation.Token);
                
                if (!_searchCancellation.Token.IsCancellationRequested)
                {
                    DisplayResults(results);
                }
            }
            catch (OperationCanceledException)
            {
                // Search was cancelled
            }
            catch (Exception ex)
            {
                toolStripStatusLabel.Text = $"Search error: {ex.Message}";
            }
            finally
            {
                progressBar.Visible = false;
            }
        }

        private void DisplayResults(List<FileSearchResult> results)
        {
            listViewResults.BeginUpdate();
            listViewResults.Items.Clear();

            try
            {
                foreach (var result in results)
                {
                    var item = new ListViewItem(result.FileName);
                    item.SubItems.Add(result.Directory);
                    item.SubItems.Add(result.IsDirectory ? "[Folder]" : FormatFileSize(result.Size));
                    item.SubItems.Add(result.Modified.ToString("yyyy-MM-dd HH:mm"));
                    item.SubItems.Add(result.Extension);
                    item.Tag = result;

                    // Set icon
                    int iconIndex = GetIconIndex(result.FullPath, result.IsDirectory);
                    item.ImageIndex = iconIndex;

                    listViewResults.Items.Add(item);
                }

                string indexStatus = _searchService.IsIndexing ? " (indexing in progress)" : 
                    _isInitialIndexingComplete ? $" (from {_searchService.TotalIndexedFiles:N0} indexed files)" : "";
                toolStripStatusLabel.Text = $"Found {results.Count} results{indexStatus}";
            }
            finally
            {
                listViewResults.EndUpdate();
            }
        }

        private int GetIconIndex(string filePath, bool isDirectory)
        {
            string extension = isDirectory ? "[FOLDER]" : Path.GetExtension(filePath).ToLower();
            
            if (_iconCache.TryGetValue(extension, out int cachedIndex))
            {
                return cachedIndex;
            }

            try
            {
                Icon icon = IconExtractor.GetFileIcon(filePath, false);
                _imageList.Images.Add(icon);
                int index = _imageList.Images.Count - 1;
                _iconCache[extension] = index;
                return index;
            }
            catch
            {
                return 0; // Default icon
            }
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 B";
            
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double size = bytes;
            
            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }
            
            return $"{size:F1} {suffixes[suffixIndex]}";
        }

        private void OnSearchCompleted(object sender, SearchCompletedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnSearchCompleted(sender, e)));
                return;
            }

            DisplayResults(e.Results);
        }

        private void OnResultDoubleClick(object sender, EventArgs e)
        {
            if (listViewResults.SelectedItems.Count > 0)
            {
                var result = (FileSearchResult)listViewResults.SelectedItems[0].Tag;
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = result.FullPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening file: {ex.Message}", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnResultMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = listViewResults.HitTest(e.Location);
                if (hitTest.Item != null)
                {
                    var result = (FileSearchResult)hitTest.Item.Tag;
                    ContextMenuHandler.ShowContextMenu(result.FullPath, this, e.X, e.Y);
                }
            }
        }

        private void OnViewModeChanged(object sender, EventArgs e)
        {
            // Update button states
            toolStripButtonDetails.Checked = sender == toolStripButtonDetails;
            toolStripButtonList.Checked = sender == toolStripButtonList;
            toolStripButtonLargeIcons.Checked = sender == toolStripButtonLargeIcons;
            toolStripButtonSmallIcons.Checked = sender == toolStripButtonSmallIcons;

            if (sender == toolStripButtonDetails)
            {
                listViewResults.View = View.Details;
            }
            else if (sender == toolStripButtonList)
            {
                listViewResults.View = View.List;
            }
            else if (sender == toolStripButtonLargeIcons)
            {
                listViewResults.View = View.LargeIcon;
            }
            else if (sender == toolStripButtonSmallIcons)
            {
                listViewResults.View = View.SmallIcon;
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            _searchService?.CancelIndexing();
            _searchCancellation?.Cancel();
            _searchTimer?.Stop();
            _imageList?.Dispose();
            _searchService?.Dispose();
        }
    }
}