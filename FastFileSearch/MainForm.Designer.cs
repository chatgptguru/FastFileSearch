namespace FastFileSearch
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.ListView listViewResults;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButtonDetails;
        private System.Windows.Forms.ToolStripButton toolStripButtonList;
        private System.Windows.Forms.ToolStripButton toolStripButtonLargeIcons;
        private System.Windows.Forms.ToolStripButton toolStripButtonSmallIcons;
        private System.Windows.Forms.Panel panelSearch;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.Button buttonReindex;
        private System.Windows.Forms.Button buttonCancelIndex;
        private System.Windows.Forms.Label labelIndexingStatus;
        private System.Windows.Forms.ProgressBar progressBarIndexing;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _searchCancellation?.Dispose();
                _searchTimer?.Dispose();
                _imageList?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            textBoxSearch = new TextBox();
            listViewResults = new ListView();
            progressBar = new ProgressBar();
            toolStrip = new ToolStrip();
            toolStripButtonDetails = new ToolStripButton();
            toolStripButtonList = new ToolStripButton();
            toolStripButtonLargeIcons = new ToolStripButton();
            toolStripButtonSmallIcons = new ToolStripButton();
            panelSearch = new Panel();
            buttonReindex = new Button();
            buttonCancelIndex = new Button();
            labelIndexingStatus = new Label();
            progressBarIndexing = new ProgressBar();
            panelMain = new Panel();
            statusStrip = new StatusStrip();
            toolStripStatusLabel = new ToolStripStatusLabel();
            toolStrip.SuspendLayout();
            panelSearch.SuspendLayout();
            panelMain.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // textBoxSearch
            // 
            textBoxSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxSearch.Font = new Font("Segoe UI", 11F);
            textBoxSearch.Location = new Point(14, 14);
            textBoxSearch.Name = "textBoxSearch";
            textBoxSearch.PlaceholderText = "Type to search files...";
            textBoxSearch.Size = new Size(550, 27);
            textBoxSearch.TabIndex = 0;
            // 
            // listViewResults
            // 
            listViewResults.Dock = DockStyle.Fill;
            listViewResults.Location = new Point(0, 0);
            listViewResults.Name = "listViewResults";
            listViewResults.Size = new Size(805, 406);
            listViewResults.TabIndex = 0;
            listViewResults.UseCompatibleStateImageBehavior = false;
            listViewResults.View = View.Details;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            progressBar.Location = new Point(574, 14);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(120, 27);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 1;
            progressBar.Visible = false;
            // 
            // toolStrip
            // 
            toolStrip.Items.AddRange(new ToolStripItem[] { toolStripButtonDetails, toolStripButtonList, toolStripButtonLargeIcons, toolStripButtonSmallIcons });
            toolStrip.Location = new Point(0, 80);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new Size(805, 25);
            toolStrip.TabIndex = 1;
            // 
            // toolStripButtonDetails
            // 
            toolStripButtonDetails.Checked = true;
            toolStripButtonDetails.CheckState = CheckState.Checked;
            toolStripButtonDetails.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonDetails.Image = (Image)resources.GetObject("toolStripButtonDetails.Image");
            toolStripButtonDetails.Name = "toolStripButtonDetails";
            toolStripButtonDetails.Size = new Size(23, 22);
            toolStripButtonDetails.Text = "Details";
            toolStripButtonDetails.Click += OnViewModeChanged;
            // 
            // toolStripButtonList
            // 
            toolStripButtonList.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonList.Image = (Image)resources.GetObject("toolStripButtonList.Image");
            toolStripButtonList.Name = "toolStripButtonList";
            toolStripButtonList.Size = new Size(23, 22);
            toolStripButtonList.Text = "List";
            toolStripButtonList.Click += OnViewModeChanged;
            // 
            // toolStripButtonLargeIcons
            // 
            toolStripButtonLargeIcons.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonLargeIcons.Image = (Image)resources.GetObject("toolStripButtonLargeIcons.Image");
            toolStripButtonLargeIcons.Name = "toolStripButtonLargeIcons";
            toolStripButtonLargeIcons.Size = new Size(23, 22);
            toolStripButtonLargeIcons.Text = "Large Icons";
            toolStripButtonLargeIcons.Click += OnViewModeChanged;
            // 
            // toolStripButtonSmallIcons
            // 
            toolStripButtonSmallIcons.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonSmallIcons.Image = (Image)resources.GetObject("toolStripButtonSmallIcons.Image");
            toolStripButtonSmallIcons.Name = "toolStripButtonSmallIcons";
            toolStripButtonSmallIcons.Size = new Size(23, 22);
            toolStripButtonSmallIcons.Text = "Small Icons";
            toolStripButtonSmallIcons.Click += OnViewModeChanged;
            // 
            // panelSearch
            // 
            panelSearch.Controls.Add(textBoxSearch);
            panelSearch.Controls.Add(progressBar);
            panelSearch.Controls.Add(buttonReindex);
            panelSearch.Controls.Add(buttonCancelIndex);
            panelSearch.Controls.Add(labelIndexingStatus);
            panelSearch.Controls.Add(progressBarIndexing);
            panelSearch.Dock = DockStyle.Top;
            panelSearch.Location = new Point(0, 0);
            panelSearch.Name = "panelSearch";
            panelSearch.Size = new Size(805, 80);
            panelSearch.TabIndex = 2;
            // 
            // buttonReindex
            // 
            buttonReindex.Location = new Point(700, 12);
            buttonReindex.Name = "buttonReindex";
            buttonReindex.Size = new Size(80, 30);
            buttonReindex.TabIndex = 2;
            buttonReindex.Text = "Reindex";
            // 
            // buttonCancelIndex
            // 
            buttonCancelIndex.Location = new Point(700, 44);
            buttonCancelIndex.Name = "buttonCancelIndex";
            buttonCancelIndex.Size = new Size(80, 30);
            buttonCancelIndex.TabIndex = 3;
            buttonCancelIndex.Text = "Cancel";
            buttonCancelIndex.Visible = false;
            // 
            // labelIndexingStatus
            // 
            labelIndexingStatus.AutoSize = true;
            labelIndexingStatus.Location = new Point(14, 47);
            labelIndexingStatus.Name = "labelIndexingStatus";
            labelIndexingStatus.Size = new Size(113, 15);
            labelIndexingStatus.TabIndex = 4;
            labelIndexingStatus.Text = "Indexing status here";
            labelIndexingStatus.Visible = false;
            // 
            // progressBarIndexing
            // 
            progressBarIndexing.Location = new Point(574, 47);
            progressBarIndexing.Name = "progressBarIndexing";
            progressBarIndexing.Size = new Size(120, 23);
            progressBarIndexing.Style = ProgressBarStyle.Marquee;
            progressBarIndexing.TabIndex = 5;
            progressBarIndexing.Visible = false;
            // 
            // panelMain
            // 
            panelMain.Controls.Add(listViewResults);
            panelMain.Dock = DockStyle.Fill;
            panelMain.Location = new Point(0, 105);
            panelMain.Name = "panelMain";
            panelMain.Size = new Size(805, 406);
            panelMain.TabIndex = 0;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel });
            statusStrip.Location = new Point(0, 511);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(805, 22);
            statusStrip.TabIndex = 3;
            // 
            // toolStripStatusLabel
            // 
            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Size = new Size(113, 17);
            toolStripStatusLabel.Text = "Enter search terms...";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(805, 533);
            Controls.Add(panelMain);
            Controls.Add(toolStrip);
            Controls.Add(panelSearch);
            Controls.Add(statusStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(600, 400);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Fast File Search";
            FormClosing += OnFormClosing;
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            panelSearch.ResumeLayout(false);
            panelSearch.PerformLayout();
            panelMain.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
