using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Zbx1425.DXDynamicTexture {
    public partial class ProgressForm : Form {

        private Action Work;

        private int InspectedObjCount = 0;
        private int PatchedTextureCount = 0;

        public ProgressForm() {
            InitializeComponent();
        }

        public void SetWork(Action work) => Work = work;

        private void ProgressForm_Shown(object sender, EventArgs e) {
            backgroundWorker.RunWorkerAsync();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            Work();
        }

        public void ReportProgress(int? progressPercentage, string status) {
            backgroundWorker.ReportProgress(progressPercentage ?? progressBar.Value, status);
        }
        public void ReportProgress(int? progressPercentage, int? inspectedObjCount, int? patchedTextureCount) {
            InspectedObjCount = inspectedObjCount ?? InspectedObjCount;
            PatchedTextureCount = patchedTextureCount ?? PatchedTextureCount;

            ReportProgress(progressPercentage, $"{InspectedObjCount} object(s) inspected, {PatchedTextureCount} texture(s) patched");
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            progressBar.Value = e.ProgressPercentage;
            detailLabel.Text = (string)e.UserState;
            Update();
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
