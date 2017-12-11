using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace USBBackup
{
    public partial class ProgressForm : Form
    {
        private USBListener usbListener;
        public ProgressForm()
        {
            InitializeComponent();
        }

        public ProgressForm(USBListener usb)
        {
            InitializeComponent();
            this.usbListener = usb;
        }

        private void ProgressForm_Load(object sender, EventArgs e)
        {
            this.totalFilesTextbox.Text = usbListener.filesInDrive.ToString();
            this.currentFileTextbox.Text = usbListener.filesIterated.ToString();

            this.progressBar1.Maximum = usbListener.filesInDrive;
            this.progressBar1.Step = 1;
            this.progressBar1.Value = 0;
        }

        public void BackupStarted()
        {

        }

        public void IncrementProgress()
        {
            Invoke((MethodInvoker)delegate {
                this.progressBar1.PerformStep();
                this.currentFileTextbox.Text = usbListener.filesIterated.ToString();
            });
        }
    }
}
