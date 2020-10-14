using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SinaFinance_7X24
{
    public class KeysHelper
    {
        public void AddTextToFile(string filename, string text, bool insert = false)
        {
            string dir = Application.StartupPath + "\\config";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string file = dir + "\\" + filename + ".t";
            if(File.Exists(file) == false)
            {
                FileStream fs = new FileStream(file, FileMode.OpenOrCreate);
                fs.Close();
            }
            string existsContent = File.ReadAllText(file);
            if(existsContent.IndexOf("|" + text) > -1)
            {
                return;
            }
            if (insert)
            {
                existsContent = "|" + text + existsContent;
            }
            else
            {
                existsContent += "|" + text;
            }
            File.WriteAllText(file, existsContent);
        }

        public void RemoveTextFromFile(string filename, string text)
        {
            string file = Application.StartupPath + "/config" + "/" + filename + ".t";
            if (!File.Exists(file))
            {
                return;
            }
            string existsContent = File.ReadAllText(file);
            existsContent = existsContent.Replace("|" + text, "");
            File.WriteAllText(file, existsContent);
        }

        public string[] GetAllText(string filename)
        {
            string file = Application.StartupPath + "/config" + "/" + filename + ".t";
            if (!File.Exists(file))
            {
                return null;
            }
            string existsContent = File.ReadAllText(file);
            
            string[] ks = existsContent.Split('|');
            return ks;
        }
    }
}
