using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace USBBackup
{
    public class USBListener
    {
        private Dictionary<USBProfile, int> AvailableProfiles = new Dictionary<USBProfile, int>();
        public bool BackupInProgress = false;
        public string LastCopiedFile = "";
        public bool AbortCopy = false;
        private static object syncLock = new object();
        private static readonly int MINS_TO_WAIT=30;
        private MainWindow mw;
        private ProgressForm pf;
        Timer timer;

        public int filesInDrive = 0;
        public int filesIterated = 0;

        public USBListener()
        {
            timer = new Timer();
            timer.Interval = 1000 * 60;
            timer.Elapsed += Timer_Elapsed;
            
        }

        public void SetProgressForm(ProgressForm pf)
        {
            this.pf = pf;
        }

        public void SetMainWindow(MainWindow mw)
        {
            this.mw = mw;
        }

        public void Start()
        {

            timer.Start();
            //Timer_Elapsed(null, null);
        }

        public void AddUSBProfile(USBProfile p)
        {
            lock (syncLock)
            {
                AvailableProfiles[p] = 0;
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Log.WriteLine("Scanning for USB Drives...");
            ScanForDevices();
        }

        public void PerformBackup(USBProfile p, DriveInfo di)
        {
            //if (mw != null)
            //{
            //    mw.BackupStarted();
            //}
            BackupInProgress = true;
            //Copy(p.DestinationRoot, di.RootDirectory.ToString(),p.IgnoredFolders,p.IgnoredExtensions);
            filesInDrive = SafeWalk.EnumerateFiles(di.RootDirectory.ToString(),"*.*",SearchOption.AllDirectories).Count();
            if (mw != null)
            {
                mw.BackupStarted();
            }
            DirectoryCopy(di.RootDirectory.ToString(), p.DestinationRoot,true,p.IgnoredFolders,p.IgnoredExtensions);
            BackupInProgress = false;
            if (mw != null)
            {
                mw.BackupFinished();
            }
        }

        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs,HashSet<string> ignoredFolders, HashSet<string> ignoredExtensions)
        {
            DirectoryInfo[] dirs = new DirectoryInfo[0];
            if (ignoredFolders.Contains(Program.RemoveDriveLetter(sourceDirName)))
            {
                Log.WriteLine("Folder {0} is in the ignore list, returning.", sourceDirName);
                return;
            }
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }
            
            try
            {
                dirs = dir.GetDirectories();
            }catch(Exception e)
            {
                Log.WriteLine(e);
            }
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files=new FileInfo[0];
            try
            {
                files = dir.GetFiles();
            }
            catch(Exception e)
            {
                Log.WriteLine(e);
            }
            foreach (FileInfo file in files)
            {
                filesIterated++;
                if (this.pf != null)
                {
                    pf.IncrementProgress();
                }
                string temppath = Path.Combine(destDirName, file.Name);

                if (ignoredExtensions.Contains(file.Extension))
                {
                    Log.WriteLine("File {0} has extension {1} that is in the ignore list. Continuing.", file, file.Extension);
                    continue;
                }
                
                FileInfo destinationFile = new FileInfo(temppath);
                if (destinationFile.LastWriteTime < file.LastWriteTime)
                {
                    Log.WriteLine("Copying File: {0} to location: {1}", file, temppath);
                    file.CopyTo(temppath, true);

                }
                else
                {
                    Log.WriteLine("File: {0} already exists at destination with matching timestamp, ignoring.", destinationFile);
                }
            }
            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs,ignoredFolders,ignoredExtensions);
                }
            }
        }

        public void Copy(String destinationRoot, String source,HashSet<string>ignoredFolders,HashSet<string>ignoredExtensions)
        {
            if (AbortCopy)
            {
                return;
            }
            //Log.WriteLine("Source: " + source);
            string currentDir = Path.GetDirectoryName(source);
            string currentDirNoLetter = Program.RemoveDriveLetter(source);
            string endDir = destinationRoot + currentDirNoLetter;
            string endFile = destinationRoot + source;

            //Log.WriteLine("currentDir : {0} endDir : {1} endFile : {2}", currentDir, endDir, endFile);

            // Check to see if current dir is to be ignored
            if (ignoredFolders.Contains(currentDirNoLetter)&&currentDirNoLetter.Length>=1)
            {
                Log.WriteLine("Current Folder is in the ignore list: " + currentDirNoLetter);
              return;
            }

            Directory.CreateDirectory(endDir);
            try
            {
                foreach (string subDirs in Directory.GetDirectories(source))
                {
                    Log.WriteLine("Found dir: " + subDirs);
                    Copy(destinationRoot, subDirs, ignoredFolders,ignoredExtensions);
                }
            }catch (UnauthorizedAccessException e)
            {
                //Log.WriteLine("Cannot access file, skipping. " + e.Message);
            }

            try
            {
                foreach (string file in Directory.GetFiles(currentDir))
                {
                    Log.WriteLine(" -> Found File: " + file);
                    // If string extension is incorrect, remove it
                    if (ignoredExtensions.Contains(Path.GetExtension(file))&&Path.GetExtension(file).Length>0)
                    {
                        Log.WriteLine("File has file extension to ignore: " + Path.GetExtension(file)+" File is : "+file);
                        continue;
                    }

                    string file1 = Program.RemoveDriveLetter(file);
                    
                    //File.Create(destinationRoot + "\\" + file1);

                    FileInfo destinationFileInfo = new FileInfo(destinationRoot + file1);
                    FileInfo sourceFileInfo = new FileInfo(file);
                    if (destinationFileInfo.Exists)
                    {
                        if (sourceFileInfo.LastWriteTime > destinationFileInfo.LastWriteTime) {
                            Log.WriteLine("Copied File: " + file1 + " To: " + destinationRoot + "\\" + file1);
                            File.Copy(file, destinationRoot + file1, true);
                            LastCopiedFile = file;
                        }
                        else
                        {
                            //Log.WriteLine("Destination File and Source File match. Skipping.");
                        }
                    }
                    else
                    {
                        Log.WriteLine("Copied File: " + file1 + " To: " + destinationRoot + "\\" + file1);
                        File.Copy(file, destinationRoot + file1, true);
                        LastCopiedFile = file;
                    }
                    
                }
            }catch(Exception e)
            {
                Log.WriteLine(e.ToString());
            }
        }

        public void ResetTimers()
        {
            foreach(KeyValuePair<USBProfile,int>kvp in AvailableProfiles)
            {
                AvailableProfiles[kvp.Key] = 0;
            }
        }

        public void ScanForDevices()
        {
            Log.WriteLine("There are this many USBProfiles available: " + AvailableProfiles.Count + " It should be 1!");
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Removable)
                {
                    string deviceID = Program.GetUniqueDeviceID(drive);
                    deviceID = Program.GetHashCode(deviceID);
                    Log.WriteLine("DeviceID is: " + deviceID);

                    lock (syncLock)
                    {

                        foreach (USBProfile p in AvailableProfiles.Keys.ToArray())
                        {
                            Log.WriteLine("Profile Device ID: " + p.DeviceID);
                            if (p.DeviceID == deviceID && AvailableProfiles[p] > 0)
                            {
                                Log.WriteLine("Found a match, but have backed up lately. Decrementing value");
                                AvailableProfiles[p]--;
                            }
                            if (p.DeviceID == deviceID && AvailableProfiles[p] <= 0)
                            {
                                Log.WriteLine("Found a match, and it requires a backup. Backing up");
                                PerformBackup(p, drive);
                                AvailableProfiles[p] = Program.MINS_TO_WAIT;
                            }
                        }

                    }
                }
            }
        }
    }
}
