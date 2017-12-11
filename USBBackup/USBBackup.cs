using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USBBackup
{
    public class USBBackup
    {
        Dictionary<String, SettingsFile> usbProfiles;
        List<USBProfile> actualProfiles;
        String profileDirectory;
        public USBBackup(String profileDirectory)
        {
            this.profileDirectory = profileDirectory;
            usbProfiles = new Dictionary<string, SettingsFile>();
            actualProfiles = new List<USBProfile>();

            Log.WriteLine(profileDirectory);

            foreach (String file in Directory.GetFiles(profileDirectory))
            {
                if (Path.GetExtension(file) != Program.PROFILE_EXTENSION)
                {
                    Log.WriteLine("Found file that is not a profile: " + file);
                    continue;
                }
                try
                {
                    SettingsFile sf = new SettingsFile(file);
                    if (sf.GetKey<string>("ProfileName") != default(string))
                    {
                        usbProfiles.Add(sf.GetKey<string>("ProfileName"), sf);
                        USBProfile p = new USBProfile();
                        p.DeviceID = sf.GetKey<string>("DeviceID");
                        p.DestinationRoot = sf.GetKey<string>("RootDestination");
                        foreach(string dir in sf.GetKeyArray<string>("IgnoredFolders"))
                        {
                            p.IgnoredFolders.Add(dir);
                        }
                        foreach(string ext in sf.GetKeyArray<string>("IgnoredExtensions"))
                        {
                            p.IgnoredExtensions.Add(ext);
                        }
                        actualProfiles.Add(p);
                    }
                }catch(Exception e)
                {
                    Log.WriteLine("Caught exception whilst trying to load usb profiles. " + e.Message);
                }
            }
            this.profileDirectory = profileDirectory;
        }

        public USBProfile CreateProfile(string deviceID,string profileName,string rootDestination,string[] ignoredFolders,string[] ignoredExtensions)
        {
            Log.WriteLine("Attempting to create file called: " + profileDirectory + "\\" + profileName + Program.PROFILE_EXTENSION);
            SettingsFile sf = new SettingsFile(profileDirectory + "\\" + profileName+Program.PROFILE_EXTENSION);
            sf.SetKey("DeviceID", deviceID);
            sf.SetKey("ProfileName", profileName);
            sf.SetKey("RootDestination", rootDestination);
            Log.WriteLine("IgnoredFolders is now: " + ignoredFolders);
            sf.SetKeyArray("IgnoredFolders", ignoredFolders);
            sf.SetKeyArray("IgnoredExtensions", ignoredExtensions);
            sf.Save();
            usbProfiles[profileName] = sf;

            USBProfile p = new USBProfile();
            p.DeviceID = deviceID;
            p.DestinationRoot = rootDestination;
            foreach(string igDir in ignoredFolders)
            {
                p.IgnoredFolders.Add(igDir);
            }
            foreach(string igExt in ignoredExtensions)
            {
                p.IgnoredExtensions.Add(igExt);
            }
            return p;
;        }

        public override string ToString()
        {
            string result = "";
            foreach(SettingsFile sf in usbProfiles.Values)
            {
                result += sf.GetKey<string>("ProfileName")+ " | ";
            }
            return result;
        }

        public Dictionary<string,SettingsFile> GetProfiles()
        {
            return usbProfiles;
        }

        public List<USBProfile> GetActualProfiles()
        {
            return actualProfiles;
        }

    }
}
