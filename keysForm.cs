using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SinaFinance_7X24
{
    public partial class KeysForm : Form
    {
        public KeysForm()
        {
            InitializeComponent();
            LoadKeys();
            textBox1.Focus();
            if(listBox1.Items.Count > 0)
            {
                listBox1.SelectedIndex = 0;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string k = textBox1.Text.Trim();
            if (k != "" && listBox1.Items.IndexOf(k) < 0)
            {
                this.listBox1.Items.Add(k);
                Evt.AddNewsKey(k);
                AddKeyToFile(k);
                textBox1.Text = "";
                textBox1.Focus();
                listBox1.SelectedIndex = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(listBox1.SelectedItems.Count > 0)
            {
                foreach(int i in listBox1.SelectedIndices)
                {
                    Evt.DelNewsKey(listBox1.Items[i].ToString());
                    new KeysHelper().RemoveTextFromFile("keys", listBox1.Items[i].ToString());
                    listBox1.Items.RemoveAt(i);
                }
            }
        }

        private void AddKeyToFile(string key)
        {
            KeysHelper kh = new KeysHelper();
            kh.AddTextToFile("keys", key);
        }
        private void RemoveKeysFromFile(string key)
        {
            KeysHelper kh = new KeysHelper();
            kh.RemoveTextFromFile("keys", key);
        }

        private void LoadKeys()
        {
            KeysHelper kh = new KeysHelper();
            string[] keys = kh.GetAllText("keys");
            if(keys != null && keys.Length > 0)
            {
                foreach(string k in keys)
                {
                    if(!String.IsNullOrEmpty(k))
                    {
                        listBox1.Items.Add(k);
                    }
                }
            }
        }
    }
}
