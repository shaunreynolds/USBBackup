using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USBBackup
{
    public class Log
    {
        private static List<string> log = new List<string>();

        public static void WriteLine(string s)
        {
            Console.WriteLine(s);
            log.Add(s);
        }

        public static void WriteLine(string s, params object[] objects)
        {
            WriteLine(string.Format(s, objects));
        }

        public static void WriteLine(object o)
        {
            WriteLine(o.ToString());
        }

        public static void Dump(string file)
        {
            try
            {
                StreamWriter sw = new StreamWriter(file);
                foreach (string s in log)
                {
                    sw.WriteLine(s);
                }
                sw.Flush();
                sw.Close();
                sw.Dispose();
            }catch(Exception e)
            {
                Log.WriteLine(e);
            }
        }

        public static void Dump()
        {
            Dump("LastLog"+DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss")+".txt");
            //Dump("LastLog.txt");
        }
    }
}
