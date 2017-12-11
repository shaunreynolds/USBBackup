using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace USBBackup
{
    public class SettingsFile
    {
        Dictionary<String,Object> settingsHashMap;
        String file;
        public SettingsFile(String file)
        {
            settingsHashMap = new Dictionary<string, object>();
            this.file = file;
            Load(file);
        }

        public T GetKey<T>(String key, T defaultValue)
        {
            if (!settingsHashMap.ContainsKey(key))
            {
                settingsHashMap[key] = defaultValue;
            }
            return (T)Convert.ChangeType(settingsHashMap[key],TypeCode.Object);
        }

        public bool GetKeyBool(string key, bool defaultValue)
        {
            if (!settingsHashMap.ContainsKey(key))
            {
                settingsHashMap[key] = defaultValue;
                return defaultValue;
            }
            return bool.Parse((string)settingsHashMap[key]);
        }

        public int GetKeyInt(string key, int defaultValue)
        {
            if (!settingsHashMap.ContainsKey(key))
            {
                settingsHashMap[key] = defaultValue;
                return defaultValue;
            }
            return int.Parse((string)settingsHashMap[key]);
        }

        public T GetKey<T>(String key)
        {
            if (settingsHashMap.ContainsKey(key))
            {
                return (T)settingsHashMap[key];
            }
            else
            {
                return default(T);
            }
        }

        public T[] GetKeyArray<T>(string key)
        {
            if (settingsHashMap.ContainsKey(key))
            {
                string value = GetKey<string>(key);
                string[] values = value.Split(',');
                T[] tmp = new T[values.Count()];
                for(int i = 0; i < values.Count(); i++)
                {
                    tmp[i] = (T) Convert.ChangeType(values[i],TypeCode.Object);
                }
                return tmp;
            }
            else
            {
                return default(T[]);
            }
        }

        public void SetKey(String key, Object value)
        {
            settingsHashMap[key] = value;
        }

        public void SetKeyArray(string key, object[] value)
        {
            StringBuilder sb = new StringBuilder();
            foreach(object o in value)
            {
                Log.WriteLine("This object is in value array: " + o);
                sb.Append(","+o);
            }

            string result = sb.ToString();
            result = result.TrimStart(',');
            settingsHashMap[key] = result;
        }

        private void Load(String file)
        {
            try
            {
                StreamReader sr = new StreamReader(file);
                while (sr.Peek() > -1)
                {
                    String rawInput = sr.ReadLine();

                    if (rawInput.StartsWith("[") || rawInput.StartsWith("#")) {
                        continue;
                    }

                    String[] kv = rawInput.Split('=');
                    settingsHashMap[kv[0]] = kv[1];
                }
                sr.Dispose();
                sr.Close();
            }catch(Exception e)
            {
                Log.WriteLine("Cannot find the file specified: {0} Exception message: {1}", file, e.Message);
            }
        }

        public void Save()
        {
            Save(file);
        }

        public void Save(String file)
        {
            Log.WriteLine("SettingsFile trying to save file as: " + file);
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(file)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(file));
                }
                StreamWriter sw = new StreamWriter(file);
                foreach (KeyValuePair<String, Object> kvp in settingsHashMap)
                {
                    sw.WriteLine("{0}={1}", kvp.Key, kvp.Value);
                }
                sw.Flush();
                sw.Dispose();
                sw.Close();
            }catch(Exception e)
            {
                Log.WriteLine(e);
            }
        }
    }
}
