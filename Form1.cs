using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using utility;
namespace mycomico
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        myinclude my = new myinclude();
        string PWD = "";
        private void Form1_Load(object sender, EventArgs e)
        {
            PWD = my.pwd();
            downloadURL.Text = "網址，如：https://www.comico.com.tw/2871/";
        }





        private void downloadURL_Enter(object sender, EventArgs e)
        {

        }

        private void downloadURL_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(downloadURL.Text))
            {
                downloadURL.Text = "網址，如：https://www.comico.com.tw/2871/";
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

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
            if (downloadURL.Text == "網址，如：https://www.comico.com.tw/2871/")
            {
                downloadURL.Text = "";
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string URL = downloadURL.Text;
            if (!Regex.IsMatch(URL, @"^https://www.comico.com.tw/\d*/"))
            {
                MessageBox.Show("網址不對...");
                return;
            }
            string orin_URL = URL;

            string orin_data = my.b2s(my.file_get_contents(orin_URL));
            my.file_put_contents(PWD + "\\log.txt", orin_data);

            string orin_NAME = my.get_between(orin_data, "<meta property=\"og:title\" content=\"", "|");
            orin_NAME = orin_NAME.Replace("。", "");
            URL = URL.Replace("https://", "");
            var m = my.explode("/", URL);
            string id = m[1];
            string jsonURL = "https://www.comico.com.tw/api/getArticleList.nhn";
            string data = my.b2s(my.file_get_contents_post(jsonURL, "titleNo=" + id));
            //my.file_put_contents(PWD+"\\log.txt",data);
            var jd = my.json_decode(data);
            List<Dictionary<string, string>> dt = new List<Dictionary<string, string>>();
            for (int i = 0; i < jd[0]["result"]["list"].Count(); i++)
            {
                var d = new Dictionary<string, string>();
                d["URL"] = jd[0]["result"]["list"][i]["articleDetailUrl"].ToString();
                d["NAME"] = jd[0]["result"]["list"][i]["subtitle"].ToString();
                d["URL_IMG"] = jd[0]["result"]["list"][i]["imgUrl"].ToString();
                dt.Add(d);
            }
            Console.WriteLine(my.json_encode(dt));


            // Gets the controls collection for tabControl1.
            // Adds the tabPage1 to this collection.
            var tab = new TabPage("item_"+orin_NAME);

            DataGridView dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            //dgv.Columns.Add("ID", "序號");
            //dgv.Columns.Add("NAME", "話");
            //dgv.Columns.Add("URL", "URL");            
            dgv.Location = new Point(20, 10);
            DataTable dtt = new DataTable();
            dtt.Columns.Add("ID");
            dtt.Columns.Add("NAME");
            dtt.Columns.Add("URL");
            dtt.Columns.Add("STATUS");
            for (int i=0,max_i=dt.Count;i<max_i;i++)
            {
                dtt.Rows.Add();
                int LAST_ID = dtt.Rows.Count - 1;
                dtt.Rows[LAST_ID]["ID"] = (i + 1).ToString();
                dtt.Rows[LAST_ID]["NAME"] = dt[i]["NAME"];
                dtt.Rows[LAST_ID]["URL"] = dt[i]["URL"];
                dtt.Rows[LAST_ID]["STATUS"] = "待命";
            }
            dgv.DataSource = dtt;
            tab.Controls.Add(dgv);            
            tab.Text = orin_NAME;            
            tabControl1.TabPages.Add(tab);
        }
    }
}
