using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace USBBackup
{
    class Program
    {
        public static string CURRENT_DIR = System.Reflection.Assembly.GetEntryAssembly().Location;
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;

        public static int MINS_TO_WAIT = 30;
        public static readonly string  PROFILE_EXTENSION=".usbdat";

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            AttachConsole(ATTACH_PARENT_PROCESS);
            //USBBackup usb = new USBBackup(CURRENT_DIR);
            //foreach(SettingsFile p in usb.GetProfiles().Values)
            //{

            //}
            //USBListener usbl = new USBListener();
            //HashSet<string> temp = new HashSet<string>() { "#DB","Microsoft VS Code","MySQL","PrintScreen", "BscCompSci\\Year2\\DBDesignImplementation\\XAMPP" };
            //HashSet<string> temp2 = new HashSet<string>() {".png",".txt" };
            //usbl.Copy("C:\\Users\\Shaun\\Desktop\\test\\", "E:\\", temp,temp2);
            CURRENT_DIR = Path.GetDirectoryName(CURRENT_DIR);
            Log.WriteLine(CURRENT_DIR);

            USBBackup usbBackup = new USBBackup(CURRENT_DIR);
            USBListener usbListener = new USBListener();

            foreach(USBProfile p in usbBackup.GetActualProfiles())
            {
                usbListener.AddUSBProfile(p);
            }

            
            Application.EnableVisualStyles();
            MainWindow mw = new MainWindow(usbBackup, usbListener);
            Application.Run(mw);

            if (mw.meta.GetKeyBool("startMinimized", false))
            {
                mw.Hide();
            }

            //Log.Dump();
            Console.ReadLine();
        }

        public static string GetUniqueDeviceID(DriveInfo di)
        {
            return di.DriveFormat + di.DriveType + di.Name + di.VolumeLabel + di.TotalSize;
        }

        public static string GetHashCode(string input)
        {
            return String.Format("{0:X}", input.GetHashCode());
        }

        public static string RemoveDriveLetter(string path)
        {
            string fullDir = path;
            string dir = fullDir.Substring(Path.GetPathRoot(fullDir).Length);
            return dir;
        }

        static void OnProcessExit(object s, EventArgs e)
        {
            Log.Dump();
        }
    }
}
