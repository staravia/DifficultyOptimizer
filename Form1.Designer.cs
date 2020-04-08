namespace DifficultyOptimizer
{
    partial class DifficultyOptimizerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.DataGrid = new System.Windows.Forms.DataGridView();
            this.Compute = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.FilePath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TargetDifficulty = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Weight = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Output = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.ButtonOptimize = new System.Windows.Forms.Button();
            this.ButtonExportData = new System.Windows.Forms.Button();
            this.ButtonImportData = new System.Windows.Forms.Button();
            this.ButtonImportMap = new System.Windows.Forms.Button();
            this.ButtonSetDirectory = new System.Windows.Forms.Button();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.TextBoxOutput = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.DataGrid);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(927, 657);
            this.splitContainer1.SplitterDistance = 455;
            this.splitContainer1.TabIndex = 0;
            // 
            // DataGrid
            // 
            this.DataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.DataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Compute,
            this.FilePath,
            this.TargetDifficulty,
            this.Weight,
            this.Output});
            this.DataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DataGrid.Location = new System.Drawing.Point(0, 0);
            this.DataGrid.Name = "DataGrid";
            this.DataGrid.Size = new System.Drawing.Size(927, 455);
            this.DataGrid.TabIndex = 1;
            this.DataGrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // Compute
            // 
            this.Compute.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Compute.FillWeight = 50F;
            this.Compute.HeaderText = "Compute";
            this.Compute.Name = "Compute";
            this.Compute.Width = 55;
            // 
            // FilePath
            // 
            this.FilePath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.FilePath.HeaderText = "File Path";
            this.FilePath.Name = "FilePath";
            // 
            // TargetDifficulty
            // 
            this.TargetDifficulty.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.TargetDifficulty.HeaderText = "Target Difficulty";
            this.TargetDifficulty.Name = "TargetDifficulty";
            this.TargetDifficulty.Width = 97;
            // 
            // Weight
            // 
            this.Weight.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Weight.HeaderText = "Weight";
            this.Weight.Name = "Weight";
            this.Weight.Width = 66;
            // 
            // Output
            // 
            this.Output.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.Output.HeaderText = "Output";
            this.Output.Name = "Output";
            this.Output.ReadOnly = true;
            this.Output.Width = 64;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.ButtonOptimize);
            this.splitContainer2.Panel1.Controls.Add(this.ButtonExportData);
            this.splitContainer2.Panel1.Controls.Add(this.ButtonImportData);
            this.splitContainer2.Panel1.Controls.Add(this.ButtonImportMap);
            this.splitContainer2.Panel1.Controls.Add(this.ButtonSetDirectory);
            this.splitContainer2.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer2_Panel1_Paint);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.ProgressBar);
            this.splitContainer2.Panel2.Controls.Add(this.TextBoxOutput);
            this.splitContainer2.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer2_Panel2_Paint);
            this.splitContainer2.Size = new System.Drawing.Size(927, 198);
            this.splitContainer2.SplitterDistance = 153;
            this.splitContainer2.TabIndex = 0;
            // 
            // ButtonOptimize
            // 
            this.ButtonOptimize.AutoSize = true;
            this.ButtonOptimize.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonOptimize.Dock = System.Windows.Forms.DockStyle.Top;
            this.ButtonOptimize.Location = new System.Drawing.Point(0, 92);
            this.ButtonOptimize.Name = "ButtonOptimize";
            this.ButtonOptimize.Size = new System.Drawing.Size(153, 23);
            this.ButtonOptimize.TabIndex = 3;
            this.ButtonOptimize.Text = "Optimize";
            this.ButtonOptimize.UseVisualStyleBackColor = true;
            this.ButtonOptimize.Click += new System.EventHandler(this.ButtonOptimize_Click);
            // 
            // ButtonExportData
            // 
            this.ButtonExportData.AutoSize = true;
            this.ButtonExportData.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonExportData.Dock = System.Windows.Forms.DockStyle.Top;
            this.ButtonExportData.Location = new System.Drawing.Point(0, 69);
            this.ButtonExportData.Name = "ButtonExportData";
            this.ButtonExportData.Size = new System.Drawing.Size(153, 23);
            this.ButtonExportData.TabIndex = 1;
            this.ButtonExportData.Text = "Export Dataset Json";
            this.ButtonExportData.UseVisualStyleBackColor = true;
            this.ButtonExportData.Click += new System.EventHandler(this.ButtonExportData_Click);
            // 
            // ButtonImportData
            // 
            this.ButtonImportData.AutoSize = true;
            this.ButtonImportData.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonImportData.Dock = System.Windows.Forms.DockStyle.Top;
            this.ButtonImportData.Location = new System.Drawing.Point(0, 46);
            this.ButtonImportData.Name = "ButtonImportData";
            this.ButtonImportData.Size = new System.Drawing.Size(153, 23);
            this.ButtonImportData.TabIndex = 2;
            this.ButtonImportData.Text = "Import Dataset Json";
            this.ButtonImportData.UseVisualStyleBackColor = true;
            this.ButtonImportData.Click += new System.EventHandler(this.ButtonImportData_Click);
            // 
            // ButtonImportMap
            // 
            this.ButtonImportMap.AutoSize = true;
            this.ButtonImportMap.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonImportMap.Dock = System.Windows.Forms.DockStyle.Top;
            this.ButtonImportMap.Location = new System.Drawing.Point(0, 23);
            this.ButtonImportMap.Name = "ButtonImportMap";
            this.ButtonImportMap.Size = new System.Drawing.Size(153, 23);
            this.ButtonImportMap.TabIndex = 4;
            this.ButtonImportMap.Text = "Import Maps";
            this.ButtonImportMap.UseVisualStyleBackColor = true;
            this.ButtonImportMap.Click += new System.EventHandler(this.ButtonImportMap_Click);
            // 
            // ButtonSetDirectory
            // 
            this.ButtonSetDirectory.AutoSize = true;
            this.ButtonSetDirectory.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ButtonSetDirectory.Dock = System.Windows.Forms.DockStyle.Top;
            this.ButtonSetDirectory.Location = new System.Drawing.Point(0, 0);
            this.ButtonSetDirectory.Name = "ButtonSetDirectory";
            this.ButtonSetDirectory.Size = new System.Drawing.Size(153, 23);
            this.ButtonSetDirectory.TabIndex = 5;
            this.ButtonSetDirectory.Text = "Set Maps Directory";
            this.ButtonSetDirectory.UseVisualStyleBackColor = true;
            this.ButtonSetDirectory.Click += new System.EventHandler(this.ButtonSetDirectory_Click);
            // 
            // ProgressBar
            // 
            this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ProgressBar.Location = new System.Drawing.Point(3, 185);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(764, 10);
            this.ProgressBar.Step = 1;
            this.ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.ProgressBar.TabIndex = 1;
            // 
            // TextBoxOutput
            // 
            this.TextBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBoxOutput.Location = new System.Drawing.Point(3, 3);
            this.TextBoxOutput.Name = "TextBoxOutput";
            this.TextBoxOutput.ReadOnly = true;
            this.TextBoxOutput.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.TextBoxOutput.Size = new System.Drawing.Size(764, 183);
            this.TextBoxOutput.TabIndex = 0;
            this.TextBoxOutput.Text = "";
            // 
            // DifficultyOptimizerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(927, 657);
            this.Controls.Add(this.splitContainer1);
            this.MinimumSize = new System.Drawing.Size(675, 450);
            this.Name = "DifficultyOptimizerForm";
            this.Text = "Quaver Difficulty Optimizer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DataGrid)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.RichTextBox TextBoxOutput;
        private System.Windows.Forms.ProgressBar ProgressBar;
        private System.Windows.Forms.DataGridView DataGrid;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Compute;
        private System.Windows.Forms.DataGridViewTextBoxColumn FilePath;
        private System.Windows.Forms.DataGridViewTextBoxColumn TargetDifficulty;
        private System.Windows.Forms.DataGridViewTextBoxColumn Weight;
        private System.Windows.Forms.DataGridViewTextBoxColumn Output;
        private System.Windows.Forms.Button ButtonOptimize;
        private System.Windows.Forms.Button ButtonExportData;
        private System.Windows.Forms.Button ButtonImportData;
        private System.Windows.Forms.Button ButtonImportMap;
        private System.Windows.Forms.Button ButtonSetDirectory;
    }
}

