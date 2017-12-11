using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace USBBackup
{
    public partial class CreateProfile : Form
    {
        string deviceID;
        USBBackup  usb;
        USBListener usbl;
        MainWindow mw;

        public CreateProfile(String deviceID,USBBackup usb,MainWindow mw,USBListener usbl)
        {
            InitializeComponent();
            this.deviceID = deviceID;
            this.usb = usb;
            this.mw = mw;
            this.usbl = usbl;
        }

        public void SetFromSettingsFile(SettingsFile sf)
        {
            this.deviceIDTextBox.Text = sf.GetKey<string>("DeviceID");
            this.profileNameTextBox.Text = sf.GetKey<string>("ProfileName");
            this.destinationRootTextBox.Text = sf.GetKey<string>("RootDestination");
            this.ignoreListBox.Items.AddRange(sf.GetKeyArray<string>("IgnoredFolders"));
        }

        private void CreateProfile_Load(object sender, EventArgs e)
        {
            deviceIDTextBox.Text = deviceID;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult dr = fbd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                destinationRootTextBox.Text = fbd.SelectedPath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult dr = fbd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string fullDir = fbd.SelectedPath;
                string dir = fullDir.Substring(Path.GetPathRoot(fullDir).Length);
                ignoreListBox.Items.Add(dir);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (ignoreListBox.SelectedIndex > -1)
            {
                ignoreListBox.Items.Remove(ignoreListBox.SelectedItem);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //USBBackup usb = new USBBackup(Program.CURRENT_DIR);
            string[] array = new string[ignoreListBox.Items.Count];
            for(int i = 0; i < ignoreListBox.Items.Count; i++)
            {
                array[i] =(string) ignoreListBox.Items[i];
            }
            Console.WriteLine("IgnoredFolders is now: " + array);
            USBProfile p = usb.CreateProfile(deviceIDTextBox.Text, profileNameTextBox.Text, destinationRootTextBox.Text+"\\", array,new string[] { });
            
            this.Hide();
            this.Dispose();
            mw.PopulateListBox();
            usbl.AddUSBProfile(p);
        }
    }
}
