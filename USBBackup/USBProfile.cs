using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USBBackup
{
    public class USBProfile
    {
        public string DeviceID;
        public string DestinationRoot;
        public HashSet<string> IgnoredFolders;
        public HashSet<string> IgnoredExtensions;

        public USBProfile()
        {
            this.IgnoredFolders = new HashSet<string>();
            this.IgnoredExtensions = new HashSet<string>();
        }
    }
}
