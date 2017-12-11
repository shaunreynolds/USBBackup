using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace USBBackup
{
    public partial class MainWindow : Form
    {
        List<DriveInfo> availableDrives;
        USBBackup usb;
        USBListener usbListener;
        public SettingsFile meta = new SettingsFile(Program.CURRENT_DIR+"\\meta.txt");
        ProgressForm pf;
        public MainWindow(USBBackup usb,USBListener usbListener)
        {
            InitializeComponent();
            availableDrives = new List<DriveInfo>();
            this.usb = usb;
            this.usbListener = usbListener;
            this.Shown += MainWindow_Shown;
            this.notifyIcon1.MouseMove += NotifyIcon1_MouseMove;
            this.FormClosing += MainWindow_FormClosing;
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {


            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Removable)
                {
                    Log.WriteLine("Device Root Is: "+drive.RootDirectory);
                    availableDrives.Add(drive);
                    driveLetterComboBox.Items.Add(drive);
                }
            }

            PopulateListBox();

            Log.WriteLine(usb);

            usbListener.Start();

            checkBox1.Checked = meta.GetKeyBool("runAtStartup", false);
            startMinimizedCheckBox.Checked = meta.GetKeyBool("startMinimized", false);
            showProgressCheckbox.Checked = meta.GetKeyBool("showProgress", false);
            Program.MINS_TO_WAIT = meta.GetKeyInt("minsToWait", 30);
            numericUpDown1.Value = Program.MINS_TO_WAIT;

            usbListener.SetMainWindow(this);
            
        }

        public void PopulateListBox()
        {
            profilesListBox.Items.Clear();
            foreach (SettingsFile sf in usb.GetProfiles().Values)
            {
                profilesListBox.Items.Add(sf.GetKey<string>("ProfileName"));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(driveLetterComboBox.SelectedIndex > -1)
            {
                DriveInfo di =(DriveInfo) driveLetterComboBox.SelectedItem;
                string toHash = Program.GetUniqueDeviceID(di);
                string hashCode = Program.GetHashCode(toHash);

                CreateProfile cp = new CreateProfile(hashCode,usb,this,usbListener);
                cp.Show();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (profilesListBox.SelectedIndex > -1)
            {
                

                string fullPath = Program.CURRENT_DIR + "\\" + profilesListBox.SelectedItem + Program.PROFILE_EXTENSION;
                Log.WriteLine("Attempting to delete profile: " + fullPath);
                profilesListBox.Items.Remove(profilesListBox.SelectedItem);
                File.Delete(fullPath);
            }
        }

        private void editProfile_Click(object sender, EventArgs e)
        {
            if (profilesListBox.SelectedIndex > -1)
            {
                string profileName = (string) profilesListBox.SelectedItem;
                // Get sf from name
                SettingsFile sf = usb.GetProfiles()[profileName];
                CreateProfile cp = new CreateProfile(sf.GetKey<string>("DeviceID"), usb,this,usbListener);
                cp.SetFromSettingsFile(sf);
                cp.Show();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            usbListener.ResetTimers();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Thread t = new Thread(delegate() {
                if (checkBox1.Checked)
                {

                    RegistryKey add = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    add.SetValue("USBBackup", "\"" + Application.ExecutablePath.ToString() + "\"");
                }
                else
                {
                    RegistryKey add = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    if (add != null)
                    {
                        add.DeleteValue("USBBackup");
                    }
                }
                meta.SetKey("runAtStartup", checkBox1.Checked);
                meta.Save();

            });
            t.Start();
            //if (checkBox1.Checked)
            //{
                
            //    RegistryKey add = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            //    add.SetValue("USBBackup", "\"" + Application.ExecutablePath.ToString() + "\"");
            //}
            //else
            //{
            //    RegistryKey add = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            //    if (add != null)
            //    {
            //        add.DeleteValue("USBBackup");
            //    }
            //}
            //meta.SetKey("runAtStartup", checkBox1.Checked);
            //meta.Save();
        }

        private void startMinimizedCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            meta.SetKey("startMinimized", startMinimizedCheckBox.Checked);
            meta.Save();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        public void BackupStarted()
        {
            notifyIcon1.BalloonTipText = "Backup of USB Drive started";
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.BalloonTipTitle = "USBBackup";
            notifyIcon1.ShowBalloonTip(500);
            Invoke((MethodInvoker)delegate {

                if (this.showProgressCheckbox.Checked)
                {
                    pf = new ProgressForm(usbListener);
                    usbListener.SetProgressForm(pf);
                    pf.Show();
                }
            });
        }

        public void BackupFinished()
        {
            Invoke((MethodInvoker)delegate {

                if (pf!=null)
                {
                    pf.Hide();
                    pf.Dispose();
                    pf = null;
                }
            });
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (usbListener.BackupInProgress)
            {
                usbListener.AbortCopy = true;
                while (usbListener.BackupInProgress)
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
            //Log.Dump();
            notifyIcon1.Visible = false;
            Application.Exit();
            Application.ExitThread();
            System.Environment.Exit(0);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Program.MINS_TO_WAIT = (int) this.numericUpDown1.Value;
            meta.SetKey("minsToWait", Program.MINS_TO_WAIT);
            meta.Save();
        }

        private void showProgressCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            meta.SetKey("showProgress", showProgressCheckbox.Checked);
            meta.Save();
        }
    }
}
