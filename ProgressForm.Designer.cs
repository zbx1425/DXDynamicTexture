namespace Zbx1425.DXDynamicTexture {

    partial class ProgressForm {

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.captionLabel_JP = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.detailLabel = new System.Windows.Forms.Label();
            this.captionLabel_EN = new System.Windows.Forms.Label();
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // captionLabel_JP
            // 
            this.captionLabel_JP.AutoSize = true;
            this.captionLabel_JP.Font = new System.Drawing.Font("MS UI Gothic", 9F);
            this.captionLabel_JP.Location = new System.Drawing.Point(16, 20);
            this.captionLabel_JP.Name = "captionLabel_JP";
            this.captionLabel_JP.Size = new System.Drawing.Size(143, 12);
            this.captionLabel_JP.TabIndex = 0;
            this.captionLabel_JP.Text = "テクスチャを置き換えています...";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(16, 60);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(288, 24);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 1;
            // 
            // detailLabel
            // 
            this.detailLabel.AutoSize = true;
            this.detailLabel.Font = new System.Drawing.Font("MS UI Gothic", 9F);
            this.detailLabel.Location = new System.Drawing.Point(16, 92);
            this.detailLabel.Name = "detailLabel";
            this.detailLabel.Size = new System.Drawing.Size(56, 12);
            this.detailLabel.TabIndex = 2;
            this.detailLabel.Text = "Initializing";
            // 
            // captionLabel_EN
            // 
            this.captionLabel_EN.AutoSize = true;
            this.captionLabel_EN.Font = new System.Drawing.Font("MS UI Gothic", 9F);
            this.captionLabel_EN.Location = new System.Drawing.Point(16, 36);
            this.captionLabel_EN.Name = "captionLabel_EN";
            this.captionLabel_EN.Size = new System.Drawing.Size(101, 12);
            this.captionLabel_EN.TabIndex = 3;
            this.captionLabel_EN.Text = "Patching textures...";
            // 
            // backgroundWorker
            // 
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_DoWork);
            this.backgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_ProgressChanged);
            this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);
            // 
            // ProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 120);
            this.ControlBox = false;
            this.Controls.Add(this.captionLabel_EN);
            this.Controls.Add(this.detailLabel);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.captionLabel_JP);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressForm";
            this.Text = "DXDynamicTexture";
            this.TopMost = true;
            this.Shown += new System.EventHandler(this.ProgressForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label captionLabel_JP;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label detailLabel;
        private System.Windows.Forms.Label captionLabel_EN;
        private System.ComponentModel.BackgroundWorker backgroundWorker;
    }
}