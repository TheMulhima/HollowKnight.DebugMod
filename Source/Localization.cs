using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reflection;
using System.IO;
namespace DebugMod
{
    public static class Localization
    {
        public static Dictionary<string, string> localText;
        public static void LoadLocaltext()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "language.json");
            if(File.Exists(path))
            {
                using FileStream fileStream=File.OpenRead(path);
                using StreamReader sr= new(fileStream);
                string LT = sr.ReadToEnd();
                localText=JsonConvert.DeserializeObject<Dictionary<string, string>>(LT);
            }
            localText ??= new();
        }
        public static string Localize(this string orig)
        {
            if(localText.TryGetValue(orig, out string result))
            {
                return result;
            }
            return orig;
        }
    }
}
