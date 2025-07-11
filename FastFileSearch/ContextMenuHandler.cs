using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace FastFileSearch
{
    public static class ContextMenuHandler
    {
        public static void ShowContextMenu(string filePath, Control parent, int x, int y)
        {
            try
            {
                // Create context menu programmatically with common options
                var contextMenu = new ContextMenuStrip();
                
                // Open
                var openItem = new ToolStripMenuItem("Open");
                openItem.Click += (s, e) => OpenFile(filePath);
                contextMenu.Items.Add(openItem);
                
                // Open with
                var openWithItem = new ToolStripMenuItem("Open with");
                openWithItem.Click += (s, e) => OpenWith(filePath);
                contextMenu.Items.Add(openWithItem);
                
                contextMenu.Items.Add(new ToolStripSeparator());
                
                // Cut
                var cutItem = new ToolStripMenuItem("Cut");
                cutItem.Click += (s, e) => CutFile(filePath);
                contextMenu.Items.Add(cutItem);
                
                // Copy
                var copyItem = new ToolStripMenuItem("Copy");
                copyItem.Click += (s, e) => CopyFile(filePath);
                contextMenu.Items.Add(copyItem);
                
                contextMenu.Items.Add(new ToolStripSeparator());
                
                // Delete
                var deleteItem = new ToolStripMenuItem("Delete");
                deleteItem.Click += (s, e) => DeleteFile(filePath);
                contextMenu.Items.Add(deleteItem);
                
                // Rename
                var renameItem = new ToolStripMenuItem("Rename");
                renameItem.Click += (s, e) => RenameFile(filePath);
                contextMenu.Items.Add(renameItem);
                
                contextMenu.Items.Add(new ToolStripSeparator());
                
                // Properties
                var propertiesItem = new ToolStripMenuItem("Properties");
                propertiesItem.Click += (s, e) => ShowProperties(filePath);
                contextMenu.Items.Add(propertiesItem);
                
                // Show location
                var showLocationItem = new ToolStripMenuItem("Show in Explorer");
                showLocationItem.Click += (s, e) => ShowInExplorer(filePath);
                contextMenu.Items.Add(showLocationItem);
                
                contextMenu.Show(parent, x, y);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing context menu: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void OpenFile(string filePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void OpenWith(string filePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "rundll32.exe",
                    Arguments = $"shell32.dll,OpenAs_RunDLL {filePath}",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening 'Open with' dialog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void CutFile(string filePath)
        {
            try
            {
                var files = new System.Collections.Specialized.StringCollection();
                files.Add(filePath);
                Clipboard.SetFileDropList(files);
                // Note: In a real implementation, you'd need to track cut state
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cutting file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void CopyFile(string filePath)
        {
            try
            {
                var files = new System.Collections.Specialized.StringCollection();
                files.Add(filePath);
                Clipboard.SetFileDropList(files);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void DeleteFile(string filePath)
        {
            try
            {
                var result = MessageBox.Show($"Are you sure you want to delete '{Path.GetFileName(filePath)}'?", 
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    else if (Directory.Exists(filePath))
                    {
                        Directory.Delete(filePath, true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void RenameFile(string filePath)
        {
            try
            {
                string currentName = Path.GetFileName(filePath);
                string newName = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter new name:", "Rename", currentName);
                
                if (!string.IsNullOrEmpty(newName) && newName != currentName)
                {
                    string directory = Path.GetDirectoryName(filePath);
                    string newPath = Path.Combine(directory, newName);
                    
                    if (File.Exists(filePath))
                    {
                        File.Move(filePath, newPath);
                    }
                    else if (Directory.Exists(filePath))
                    {
                        Directory.Move(filePath, newPath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error renaming file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void ShowProperties(string filePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "rundll32.exe",
                    Arguments = $"shell32.dll,ShellExec_RunDLL Properties {filePath}",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing properties: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void ShowInExplorer(string filePath)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{filePath}\"",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Explorer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}