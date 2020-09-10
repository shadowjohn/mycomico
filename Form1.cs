using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using utility;
using System.Reflection;

namespace mycomico
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public ConcurrentDictionary<string, ConcurrentDictionary<string, object>> SOURCES = new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>();
        /*
         * SOURCES[PAGE_0][URL、NAME...]
        */
        myinclude my = new myinclude();
        string PWD = "";
        public string wBDomain = "";
        public string wBCookie = "";
        private void Form1_Load(object sender, EventArgs e)
        {
            PWD = my.pwd();
            downloadURL.Text = "https://www.comico.com.tw/2871/";
        }





        private void downloadURL_Enter(object sender, EventArgs e)
        {

        }

        private void downloadURL_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(downloadURL.Text))
            {
                downloadURL.Text = "https://www.comico.com.tw/2871/";
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //wBDomain = wB.Document.Domain;
            //wBCookie = wB.Document.Cookie;
        }

        private void wB_DocumentCompleted(object ss, WebBrowserDocumentCompletedEventArgs e)
        {
            //$(".btnPopup a").click();
            //Thread.Sleep(5000);
            WebBrowser w = ss as WebBrowser;
            //
            if (!my.is_string_like(w.DocumentText, "3wa3wa"))
            {
                w.DocumentText += @"                            
                            <script src='https://3wa.tw/inc/javascript/jquery/jquery-1.8.3.min.js'></script>
                            <script>
                                //3wa3wa
                                $('.btnPopup').click();
                            </script>
                    ";
            }
            //w.Document.InvokeScript("eval", new string[] {
            //    "(function(){window.onload=function(){setInterval(function(){$('.btnPopup a').remove();},3000);};");
            //runSC("setInterval(function(){$('.btnPopup').click();},3000);");
            Console.WriteLine("Done....XD");
        }

        private void wB_LocationChanged(object sender, EventArgs e)
        {

        }

        private void wB_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            string URL = wB.Url.ToString();
            Console.WriteLine(URL);
            URL = URL.Replace("https://", "");
            var m = my.explode("/", URL);
            if (m.Count() != 3)
            {
                return;
            }
            if (!my.IsNumeric(m[1]))
            {
                return;
            }
            downloadURL.Text = "https://" + URL;
        }

        private void downloadURL_MouseClick(object sender, MouseEventArgs e)
        {
            if (downloadURL.Text == "https://www.comico.com.tw/2871/")
            {
                downloadURL.Text = "";
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button1.Text = "載入中...";
            string URL = downloadURL.Text;
            if (!Regex.IsMatch(URL, @"^https://www.comico.com.tw/\d*/"))
            {
                MessageBox.Show("網址不對...");
                button1.Enabled = true;
                button1.Text = "載入";
                return;
            }
            string orin_URL = URL;

            //從已知的 tabs 找 tooltips，如果一樣，就跳過，直接跳到那頁
            for (int i = 0, max_i = tabControl1.TabPages.Count; i < max_i; i++)
            {
                string tipname = tabControl1.TabPages[i].ToolTipText;
                if (tipname == orin_URL)
                {
                    tabControl1.SelectedIndex = i;
                    button1.Enabled = true;
                    button1.Text = "載入";
                    return;
                }
            }


            string orin_data = my.b2s(my.file_get_contents(orin_URL));
            //my.file_put_contents(PWD + "\\log.txt", orin_data);

            string orin_NAME = my.get_between(orin_data, "<meta property=\"og:title\" content=\"", "|");
            orin_NAME = orin_NAME.Replace("。", "");
            URL = URL.Replace("https://", "");
            var m = my.explode("/", URL);
            string id = m[1];
            string jsonURL = "https://www.comico.com.tw/api/getArticleList.nhn";
            string data = my.b2s(my.file_get_contents_post(jsonURL, "titleNo=" + id));
            //my.file_put_contents(PWD + "\\log.txt", data);
            var jd = my.json_decode(data);
            List<ConcurrentDictionary<string, string>> ldt = new List<ConcurrentDictionary<string, string>>();
            for (int i = 0; i < jd[0]["result"]["list"].Count(); i++)
            {
                var d = new ConcurrentDictionary<string, string>();
                d["URL"] = jd[0]["result"]["list"][i]["articleDetailUrl"].ToString();
                d["NAME"] = jd[0]["result"]["list"][i]["subtitle"].ToString();
                d["URL_IMG"] = jd[0]["result"]["list"][i]["imgUrl"].ToString();
                ldt.Add(d);
            }
            Console.WriteLine(my.json_encode(ldt));


            // Gets the controls collection for tabControl1.
            // Adds the tabPage1 to this collection.
            var tab = new TabPage("item_" + orin_NAME);
            tab.ToolTipText = orin_URL;

            DataGridView dgv = new DataGridView();
            //dgv.AutoGenerateColumns = false; //這啥
            dgv.AllowUserToAddRows = false; //不能允許使用者自行調整
            dgv.RowHeadersVisible = false; //左邊空欄移除
            dgv.Dock = DockStyle.None; //自動展開到最大
            dgv.AllowDrop = false;
            dgv.ReadOnly = true;
            dgv.Name = "PAGE_" + tabControl1.TabPages.Count.ToString();
            SOURCES[dgv.Name] = new ConcurrentDictionary<string, object>();
            SOURCES[dgv.Name]["NAME"] = orin_NAME;
            SOURCES[dgv.Name]["URL"] = orin_URL;
            dgv.Width = 650;
            dgv.Height = 500;
            //dgv.Columns.Add("ID", "序號");
            //dgv.Columns.Add("NAME", "話");
            //dgv.Columns.Add("URL", "URL");            
            dgv.Location = new Point(0, 50);
            DataTable dtt = new DataTable();
            dtt.Columns.Add("ID");
            dtt.Columns.Add("NAME");
            dtt.Columns.Add("URL");
            dtt.Columns.Add("STATUS");
            for (int i = 0, max_i = ldt.Count; i < max_i; i++)
            {
                dtt.Rows.Add();
                int LAST_ID = dtt.Rows.Count - 1;
                dtt.Rows[LAST_ID]["ID"] = (i + 1).ToString();
                dtt.Rows[LAST_ID]["NAME"] = ldt[i]["NAME"];
                dtt.Rows[LAST_ID]["URL"] = ldt[i]["URL"];
                dtt.Rows[LAST_ID]["STATUS"] = "待命";
            }
            dgv.DataSource = dtt;
            SOURCES[dgv.Name]["回"] = dtt;

            //加一個標題
            var labelTitle = new System.Windows.Forms.Label();
            labelTitle.Text = orin_NAME + " (共 " + ldt.Count.ToString() + " 回)";
            labelTitle.Width = 650;
            labelTitle.Font = new Font("微軟正黑體", 16);
            labelTitle.Location = new Point(0, 0);

            //加一個下載的鈕
            var btnRun = new System.Windows.Forms.Button();
            btnRun.Name = "goBtn - " + dgv.Name;

            btnRun.Text = "開始下載";
            btnRun.Location = new Point(this.Width - btnRun.Width - 50, 0);
            btnRun.Click += (object s, EventArgs ee) =>
            {
                //按鈕的程式
                //MessageBox.Show(dgv.Name);          
                Button btn = s as Button;
                string key = my.explode(" - ", btn.Name)[1];
                switch (btn.Text)
                {
                    case "開始下載":
                        {
                            btn.Text = "下載中...(點了取消)";
                            SOURCES[key]["RUN_THREAD"] = new Thread(() => run(key, btn));
                            ((Thread)SOURCES[key]["RUN_THREAD"]).Start();
                        }
                        break;
                    default:
                        {
                            btn.Text = "開始下載";
                            if (SOURCES[key].Keys.Contains("RUN_THREAD") && SOURCES[key]["RUN_THREAD"] != null)
                            {
                                ((Thread)(SOURCES[key]["RUN_THREAD"])).Abort();
                            }
                        }
                        break;
                }
            };
            tab.Controls.Add(labelTitle);
            tab.Controls.Add(btnRun);
            tab.Controls.Add(dgv);
            tab.Text = dgv.Name;
            tabControl1.TabPages.Add(tab);

            for (int i = 0; i < dgv.Columns.Count - 1; i++)
            {
                dgv.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            dgv.Columns[dgv.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                int colw = dgv.Columns[i].Width;
                dgv.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                dgv.Columns[i].Width = colw;
            }
            button1.Enabled = true;
            button1.Text = "載入";
            tabControl1.SelectedIndex = tabControl1.TabPages.Count - 1;
        }
        public void run(string key, Button btn)
        {
            //Thread.Sleep(5000);
            //MessageBox.Show(key);
            DataGridView c = this.Controls.Find(key, true)[0] as DataGridView;
            //MessageBox.Show(c.Rows.Count.ToString());
            DataTable dt = ((DataTable)(SOURCES[key]["回"]));
            for (int i = 0, max_i = c.Rows.Count; i < max_i; i++)
            {
                //下載單頁
                Console.WriteLine(my.json_encode(dt.Rows[i]));
                Dictionary<string, object> d = new Dictionary<string, object>();
                //From : https://stackoverflow.com/questions/15049877/getting-webbrowser-cookies-to-log-in
                //https://stackoverflow.com/questions/9048922/c-sharp-invalidcastexception-when-trying-to-access-webbrowser-control-from-tim
                //d["webBrowser"] = getBrowser();
                //wB.Url = new System.Uri(dt.Rows[i]["URL"].ToString(), System.UriKind.Absolute);
                //Thread.Sleep(500);
                //d["cookie_domain"] = wBDomain;
                //d["cookie_data"] = wBCookie;
                string data = my.b2s(my.file_get_contents_post(dt.Rows[i]["URL"].ToString(), "", d));
                //取得漫畫照片範圍                
                data = my.get_between(data, "var cmnData = ", "</script>");
                data = data.Trim();
                //移除最後一個;
                if (data.Substring(data.Length - 1, 1) == ";")
                {
                    data = data.Substring(0, data.Length - 1);
                }
                data = data.Trim();
                data += "3WA超強";
                data = my.get_between(data, "imageData:", "}3WA超強");
                var pics = my.json_decode(data);

                WebBrowser newWB = getBrowser();
                TabPage tp = tabControl1.TabPages[key];
                //newWB.Location = new Point(500, 0);
                //newWB.Width = 1024;
                //newWB.Height = 768;
                //tp.Controls.Add(newWB);
                //copyControl(wB, newWB);
                newWB.Url = new System.Uri(dt.Rows[i]["URL"].ToString(), System.UriKind.Absolute);
                newWB.WebBrowserShortcutsEnabled = true;
                //newWB.ScriptErrorsSuppressed = false;


                newWB.ScriptErrorsSuppressed = true;


                Func<string, string> Eval = (script) =>
                {
                    return newWB.Document.InvokeScript("eval", new string[] {
                            "(function() { return " + script + "; })()" }).ToString();
                };
                Func<string, string> runSC = (script) =>
                {
                    try
                    {
                        return newWB.Document.InvokeScript("eval", new string[] {
                            "(function() { " + script + " })()" }).ToString();
                    }
                    catch
                    {
                        return "";
                    }
                };

                newWB.DocumentCompleted -= wB_DocumentCompleted;
                newWB.DocumentCompleted += wB_DocumentCompleted;

                //感覺不適合下載圖
                break;
                for (int j = 0, max_j = pics[0].Count(); j < max_j; j++)
                {
                    Console.WriteLine("PIC[0][" + j.ToString() + "]: " + pics[0][j].ToString());
                    string expires = my.get_between(pics[0][j].ToString(), "Expires=", "&");
                    pics[0][j] = pics[0][j].ToString().Replace(expires, (Convert.ToInt64(my.time()) + 2).ToString());
                    d["cookie_domain"] = ".comico.com.tw";
                    wBCookie = wBCookie.Replace("appier_pv_counterqvJaMzdQzyXvi7N=8", "appier_pv_counterqvJaMzdQzyXvi7N=1");
                    wBCookie = wBCookie.Replace("appier_pv_counterTw501dpRLjrYXae=7", "appier_pv_counterTw501dpRLjrYXae=1");
                    d["cookie_data"] = wBCookie;
                    d["referer"] = dt.Rows[i]["URL"].ToString();
                    d["headers"] = new List<string>();
                    d["accept"] = "image/webp,*/*";
                    ((List<string>)(d["headers"])).Add("TE: Trailers");
                    //((List<string>)(d["headers"])).Add("Accept: image/webp,*/*");
                    ((List<string>)(d["headers"])).Add("Accept-Language: zh-TW,zh;q=0.8,en-US;q=0.5,en;q=0.3");
                    ((List<string>)(d["headers"])).Add("Accept-Encoding: gzip, deflate, br");
                    byte[] p = my.file_get_contents_post(pics[0][j].ToString(), "", d);
                    my.file_put_contents(PWD + "\\" + j + ".png", p);
                    Array.Clear(p, 0, p.Length);
                    break;
                }
                //my.file_put_contents(PWD + "\\ok.txt", data);
                break;
            }

            //finish
            UpdateUIText("開始下載", btn);
        }

        private void copyControl(Control sourceControl, Control targetControl)
        {
            // make sure these are the same
            if (sourceControl.GetType() != targetControl.GetType())
            {
                throw new Exception("Incorrect control types");
            }

            foreach (PropertyInfo sourceProperty in sourceControl.GetType().GetProperties())
            {
                object newValue = sourceProperty.GetValue(sourceControl, null);

                MethodInfo mi = sourceProperty.GetSetMethod(true);
                if (mi != null)
                {
                    sourceProperty.SetValue(targetControl, newValue, null);
                }
            }
        }
        public delegate WebBrowser getBrowserHandler();
        public WebBrowser getBrowser()
        {
            if (InvokeRequired)
            {
                return Invoke(new getBrowserHandler(getBrowser)) as WebBrowser;
            }
            else
            {
                return wB;
            }
        }
        private delegate void UpdateUITextCallBack(string value, object ctl);
        public void UpdateUIText(string value, object ctl)
        {
            if (this.InvokeRequired)
            {
                UpdateUITextCallBack uu = new UpdateUITextCallBack(UpdateUIText);
                this.Invoke(uu, value, ctl);
            }
            else
            {
                string obj_kind = ctl.GetType().ToString();
                switch (obj_kind.ToLower())
                {
                    case "system.windows.forms.textbox":
                        {
                            ((TextBox)ctl).Text = value;
                        }
                        break;
                    case "system.windows.forms.label":
                        {
                            ((Label)ctl).Text = value;
                        }
                        break;
                    case "system.windows.forms.button":
                        {
                            ((Button)ctl).Text = value;
                        }
                        break;
                }
            }
        }
    }
}