using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SinaFinance_7X24
{
    public partial class tipForm : Form
    {
        bool refreshHk = false;
        bool refreshUs = false;
        public tipForm()
        {
            InitializeComponent();
            this.Hide();
            this.LoadShares();
            GetSharesValue();
        }

        private void LoadShares()
        {
            LoadShares(listView1, "hk_shares");
            LoadShares(listView2, "us_shares");
        }

        private void LoadShares(ListView listview, string filename)
        {
            string[] vs = new KeysHelper().GetAllText(filename);
            if (vs != null && vs.Length > 0)
            {
                foreach (string v in vs)
                {
                    if (string.IsNullOrEmpty(v)) continue;
                    string code = v.Split('>')[0];
                    string name = v.Split('>')[1];
                    string[] item = new string[] { "(" + code + ")" + name, "" };
                    if(filename == "us_shares")
                    {
                        item = new string[] { "(" + code + ")" + name, "", "" };
                    }
                    listview.Items.Add(new ListViewItem(item));
                }
            }
        }

        public void Evt_ClearEvent()
        {
            richTextBox1.Text = "";
        }

        public void E_NewsHandler(object sender, string[] e)
        {
            this.Show();
            this.Icon = null;
            this.Left = Screen.AllScreens[0].WorkingArea.Width - this.Width;
            this.Top = Screen.AllScreens[0].WorkingArea.Height - this.Height;
        }

        private void richTextBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Hide();
        }

        private void btn_hk_new_Click(object sender, EventArgs e)
        {
            AddNewShares(txt_hk_shares_code, 5, listView1);
        }

        private void AddNewShares(TextBox txt, int txtLen,ListView listview, string type = "hk")
        {
            string code = txt.Text.Trim();
            if (txtLen != 0 && code.Length != txtLen)
            {
                MessageBox.Show("请输入标准code值");
                return;
            }
            foreach (ListViewItem item in listview.Items)
            {
                if (item.SubItems[0].Text.IndexOf(code) > -1)
                {
                    MessageBox.Show("该code已经存在");
                    return;
                }
            }
            string name = FindShares(code);
            if (name != null)
            {
                string cn = "(" + code + ")" + name;
                if (type == "hk")
                {
                    listview.Items.Add(new ListViewItem(new string[] { cn, "" }));
                }
                else
                {
                    listview.Items.Add(new ListViewItem(new string[] { cn, "", "" }));
                }
                new KeysHelper().AddTextToFile(type + "_shares", code + ">" + name);
                txt.Text = "";
                txt.Focus();
            }
        }

        private string FindShares(string code)
        {
            string suggest_key = "suggestdata_" + ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000) + new Random().Next(100, 999);
            string url = "https://suggest3.sinajs.cn/suggest/type=&key=" + code + "&name=" + suggest_key;
            string result = new HttpHelper().GetRemoteData(url, "GET", "application/javascript; charset=GB2312");
            if(result != null)
            {
                result = result.TrimStart(("var " + suggest_key + "=").ToCharArray());
                result = result.TrimStart('"').TrimEnd('"');
                string[] vs = result.Split(';');
                if(vs.Length > 0)
                {
                    foreach(string v in vs)
                    {
                        if(v.Split(',')[0].ToLower() == code.ToLower())
                        {
                            return v.Split(',')[4];
                        }
                    }
                }
            }
            return null;
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DelUSShares(listView1, "hk");
        }

        private void btn_us_add_Click(object sender, EventArgs e)
        {
            AddNewShares(txt_us_shares_code, 0, listView2, "us");
        }

        private void listView2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DelUSShares(listView2, "us");
        }
        private void DelUSShares(ListView listview, string type)
        {
            if (listview.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("确定要删除该项吗？", "询问", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    string code_t = listview.SelectedItems[0].SubItems[0].Text;
                    string c = code_t.Split(')')[0].Replace("(", "");
                    string n = code_t.Split(')')[1];
                    string cn = c + ">" + n;
                    listview.Items.Remove(listview.SelectedItems[0]);
                    new KeysHelper().RemoveTextFromFile(type + "_shares", cn);
                }
            }
        }

        private void GetSharesValue()
        {
            Action act = GetHKSharesValue;
            act.BeginInvoke(null, null);
            Action act1 = GetUSSharesValue;
            act1.BeginInvoke(null, null);
        }

        private void GetHKSharesValue()
        {
            while (true)
            {
                if (refreshHk)
                {
                    int index = 0;
                    string[] vs = new KeysHelper().GetAllText("hk_shares");
                    if (vs != null && vs.Length > 0)
                    {
                        foreach (string v in vs)
                        {
                            if (string.IsNullOrEmpty(v)) continue;
                            string c = v.Split('>')[0];
                            string url = "https://hq.sinajs.cn/?_=0.5245967248715309&list=rt_hk" + c;
                            string k = "var hq_str_rt_hk" + c + "=";
                            GetSharesData(listView1, url, k, index, 6, 8);
                            index++;
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }
        private void GetSharesData(ListView listview, string url, string key, int index, int vi, int pi)
        {
            string result = new HttpHelper().GetRemoteData(url);
            if (result != null)
            {
                result = result.TrimStart(key.ToCharArray());
                result = result.Trim('"');
                if (result.Split(',').Length > pi && result.Split(',').Length > vi)
                {
                    string value = result.Split(',')[vi];
                    if(value.IndexOf(".") > -1)
                    {
                        value = value.TrimEnd('0');
                    }
                    string p = result.Split(',')[pi];
                    if (p.IndexOf(".") > -1)
                    {
                        p = p.TrimEnd('0');
                    }
                    string pq = "";
                    if(listview.Name == "listView2")
                    {
                        string pq_v = result.Split(',')[21];
                        if (pq_v.IndexOf(".") > -1)
                        {
                            pq_v = pq_v.TrimEnd('0');
                        }
                        string pq_p = result.Split(',')[22];
                        if (pq_p.IndexOf(".") > -1)
                        {
                            pq_p = pq_p.TrimEnd('0');
                        }
                        pq = pq_v + "(" + pq_p + "%)";
                    }                    
                    SetListViewValue(listview, index, value + "(" + p + "%)", pq, Convert.ToDecimal(p));
                }

            }
        }

        private void GetUSSharesValue()
        {
            while (true)
            {
                if (refreshUs)
                {
                    int index = 0;
                    string[] vs = new KeysHelper().GetAllText("us_shares");
                    if (vs != null && vs.Length > 0)
                    {
                        foreach (string v in vs)
                        {
                            if (string.IsNullOrEmpty(v)) continue;
                            string c = v.Split('>')[0].ToLower();
                            string url = "https://hq.sinajs.cn/etag.php?_=" + ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000) + new Random().Next(100, 999) + "&list=gb_" + c;
                            string k = "var hq_str_gb_" + c + "=";
                            GetSharesData(listView2, url, k, index, 1, 2);
                            index++;
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }

        private void SetListViewValue(ListView listview, int index, string value, string pq = "", decimal v_c = 0)
        {
            if(listview.InvokeRequired)
            {
                Action<ListView, int, string, string, decimal> act = SetListViewValue;
                listview.Invoke(act, listview, index, value, pq, v_c);
            }
            else
            {
                listview.Items[index].ForeColor = v_c > 0 ? Color.Red : v_c < 0 ? Color.Green : Color.Black;
                listview.Items[index].SubItems[1].Text = value;
                if(pq != "")
                {
                    listview.Items[index].SubItems[2].Text = pq;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "S")
            {
                refreshUs = false;
                button1.Text = "R";
            }
            else
            {
                refreshUs = true;
                button1.Text = "S";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "S")
            {
                refreshHk = false;
                button2.Text = "R";
            }
            else
            {
                refreshHk = true;
                button2.Text = "S";
            }
        }
    }
}
