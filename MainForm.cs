using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SinaFinance_7X24
{
    public partial class MainForm : Form
    {
        private List<Keys> ks = new List<Keys>();
        private List<ShowNewsInfo> ShowNewsList = new List<ShowNewsInfo>();
        private List<int> ids = new List<int>();
        private tipForm tform = null;
        public MainForm()
        {
            InitializeComponent();
            Evt.NewsKeyAdd += Evt_NewsKeyAdd;
            Evt.NewsKeyDel += Evt_NewsKeyDel;
            AddTabs();
            tform = new tipForm();
            tform.Hide();
            timer1_Tick(null, null);
            timer1.Enabled = true;
        }

        

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                string jqueryFunc = "jQuery111202289871336355307_1597916916441";
                long times = DateTime.Now.Ticks;
                string url = "http://zhibo.sina.com.cn/api/zhibo/feed?callback=" + jqueryFunc + "&page=1&page_size=20&zhibo_id=152&tag_id=0&dire=f&dpc=1&pagesize=20&id=1801036&type=0&_=" + times;

                string content = new HttpHelper().GetRemoteData(url);
                if (string.IsNullOrEmpty(content)) return;

                if (content.IndexOf("\"feed\":") > -1 && content.IndexOf("\"page_info\":") > -1)
                {
                    content = content.Substring(content.IndexOf("\"feed\":") + "\"feed\":".Length);
                    content = content.Substring(0, content.IndexOf("\"page_info\":"));
                    if (content.EndsWith(",\"html\":\"\","))
                    {
                        content = content.Substring(0, content.Length - ",\"html\":\"\",".Length);
                    }
                    content = content + "}";
                }

                NewsList nl = JsonConvert.DeserializeObject<NewsList>(content);
                if (nl.list.Count > 0)
                {
                    nl.list = nl.list.OrderByDescending((n) => n.create_time).ToList();
                    bool foundNewNews = false;
                    for (var i = nl.list.Count - 1; i >= 0; i--)
                    {
                        var n = nl.list[i];
                        if (ids.IndexOf(n.id) < 0)
                        {
                            AddKeysToFile(n.create_time, n.rich_text);
                            string[] es = new string[] { Convert.ToDateTime(n.create_time).ToString("HH:mm:ss"), n.rich_text };
                            listView1.Items.Insert(0, new ListViewItem(es));
                            ids.Add(n.id);
                            if (AddShowNews(n))
                            {
                                foundNewNews = true;
                            }
                        }
                    }
                    if (foundNewNews)
                    {
                        ShowNewsList = ShowNewsList.OrderBy((n) => n.create_time).ToList();
                        tform.Evt_ClearEvent();
                        foreach (ShowNewsInfo ns in ShowNewsList)
                        {
                            string[] es = new string[] { ns.create_time, ns.rich_text };
                            tform.E_NewsHandler(null, es);
                            string t1 = es[0];
                            string c1 = es[1];
                            tform.richTextBox1.Text = t1 + "\t" + c1 + Environment.NewLine + Environment.NewLine + tform.richTextBox1.Text;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                tform.richTextBox1.Text = DateTime.Now.ToString("hh:mm:ss") + "\t" + ex.Message + Environment.NewLine + Environment.NewLine + tform.richTextBox1.Text;
            }
        }
        private bool AddShowNews(NewsInfo n)
        {
            ShowNewsList = ShowNewsList.OrderByDescending((n1) => n1.create_time).ToList();
            if (ShowNewsList.Count >= 5)
            {
                ShowNewsList.RemoveAt(ShowNewsList.Count - 1);
            }
            ShowNewsList.Insert(0, new ShowNewsInfo()
            {
                id = n.id,
                rich_text = n.rich_text,
                create_time = Convert.ToDateTime(n.create_time).ToString("HH:mm:ss")
            });
            return true;
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            if(this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            KeysForm kf = new KeysForm();
            kf.ShowDialog();
        }

        private void AddTabs()
        {
            KeysHelper kh = new KeysHelper();
            string[] keys = kh.GetAllText("keys");
            if(keys != null && keys.Length > 0)
            {
                foreach(string k in keys)
                {
                    if (!string.IsNullOrEmpty(k))
                    {
                        Evt.AddNewsKey(k);
                    }
                }
            }
        }

        private void Evt_NewsKeyDel(object sender, string e)
        {
            for(int i = 0;i < this.tabControl1.TabPages.Count; i++)
            {
                if(this.tabControl1.TabPages[i].Text == e)
                {
                    this.tabControl1.TabPages.RemoveAt(i);
                }
            }
        }

        private void Evt_NewsKeyAdd(object sender, string e)
        {
            TabPage tp = new TabPage();
            ListView lv = new ListView();
            lv.Columns.AddRange(new ColumnHeader[]
                {
                    new ColumnHeader() { Text = "Time", Width = 100, Name = "time" },
                    new ColumnHeader() { Text = "Content", Width = 1200, Name="content" }
                }
            );
            lv.Dock = DockStyle.Fill;
            lv.View = View.Details;
            lv.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lv.FullRowSelect = true;
            lv.BorderStyle = BorderStyle.None;
            lv.GridLines = true;
            lv.Font = new Font(FontFamily.GenericSansSerif, 10);
            lv.ShowItemToolTips = true;
            tp.Text = e;
            tp.Controls.Add(lv);
            this.tabControl1.TabPages.Add(tp);
            string[] keyTexts = new KeysHelper().GetAllText(e);
            if(keyTexts == null || keyTexts.Length == 0)
            {
                return;
            }
            foreach(string text in keyTexts)
            {
                if(!string.IsNullOrEmpty(text) && text.IndexOf(">") > -1)
                {
                    string time = text.Split('>')[0];
                    if(Convert.ToDateTime(time) < Convert.ToDateTime(DateTime.Now.ToShortDateString()))
                    {
                        continue;
                    }
                    string content = text.Split('>')[1];
                    string[] es = new string[] { Convert.ToDateTime(time).ToString("HH:mm:ss"), content };
                    lv.Items.Insert(0, new ListViewItem(es));
                }
            }
        }
        private void AddKeysToFile(string create_time, string rich_text)
        {
            KeysHelper kh = new KeysHelper();
            string[] keys = kh.GetAllText("keys");
            if(keys != null && keys.Length > 0)
            {
                foreach(string k in keys)
                {
                    if(!string.IsNullOrEmpty(k) && rich_text.IndexOf(k) > -1)
                    {
                        string text = create_time + ">" + rich_text;
                        kh.AddTextToFile(k, text, true);
                        for(int i = 0;i < tabControl1.TabPages.Count; i++)
                        {
                            if(tabControl1.TabPages[i].Text == k)
                            {
                                string[] es = new string[] { Convert.ToDateTime(create_time).ToString("HH:mm:ss"), rich_text };
                                for (int j=0;j< tabControl1.TabPages[i].Controls.Count; j++)
                                {
                                    if(tabControl1.TabPages[i].Controls[j] is ListView)
                                    {
                                        (tabControl1.TabPages[i].Controls[j] as ListView).Items.Insert(0, new ListViewItem(es));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void 关闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }
    }

    public class NewsList
    {
        public List<NewsInfo> list { get; set; }
    }
    public class NewsInfo
    {
        public int id { get; set; }
        public int zhibo_id { get; set; }
        public string rich_text { get; set; }
        public string create_time { get; set; }
    }


    public class ShowNewsInfo
    {
        public int id { get; set; }
        public string rich_text { get; set; }
        public string create_time { get; set; }
    }
}
