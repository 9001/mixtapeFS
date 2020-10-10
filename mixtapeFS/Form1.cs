using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DokanNet;

namespace mixtapeFS
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        TheFS cmfs;
        Logger logger;
        List<string> msgs;

        private void Form1_Load(object sender, EventArgs e)
        {
            logger = new Logger();
            msgs = new List<string>();
            
            Timer t = new Timer();
            t.Interval = 10;
            t.Start();
            t.Tick += t_Tick;

            gLog.Font = new Font("Consolas", gLog.Font.Size);

            if (System.IO.File.Exists("mixtapeFS.ini"))
            {
                var txt = System.IO.File.ReadAllText("mixtapeFS.ini", Encoding.UTF8);
                var lines = txt.Replace("\r", "").Split('\n');

                string v_name = null;
                for (var a = 0; a < lines.Length; a++)
                {
                    var kv = lines[a].Split(new char[] { ' ' }, 2);
                    if (kv.Length != 2)
                        continue;

                    var k = kv[0];
                    var v = kv[1];

                    if (k == "mountpoint")
                        gDst.Text = v;
                    else if (k == "cache_min")
                        gCacheMin.Text = v;
                    else if (k == "cache_gbyte")
                        gCacheGB.Text = v;
                    else if (k == "cache_dir")
                        gCacheDir.Text = v;
                    else if (k == "m_name")
                        v_name = v;
                    else if (k == "m_src")
                        gList.Items.Add(new KeyValuePair<string, string>(v_name, v));
                    else
                        MessageBox.Show("unparseable line in mixtapeFS.ini:\n[" + lines[a] + "]");
                }
            }

            var asdf = this.Text.Split(new char[] { ' ' }, 2);
            string ver = Application.ProductVersion;
            this.Text = asdf[0] + "  v" + ver.Substring(0, ver.Length - 2) + " " + asdf[1];

            //this.DoubleBuffered = true;
            //typeof(Label).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, gLog, new object[] { true });
        }

        void t_Tick(object sender, EventArgs e)
        {
            string[] new_msgs = logger.pop();
            
            if (new_msgs.Length == 0)
                return;

            msgs.AddRange(new_msgs);
            if (msgs.Count > 200)
                msgs.RemoveRange(0, msgs.Count - 200);

            string txt = "";
            for (int a = msgs.Count - 1; a >= 0; a--)
                txt += msgs[a] + "\r\n";

            gLog.Text = txt;
        }

        private void gStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(gDst.Text))
            {
                MessageBox.Show("you need to provide a mountpoint to serve the files on (Mount:)");
                return;
            }
            if (gList.Items.Count < 1)
            {
                MessageBox.Show("you need to add at least one folder to mirror/rehost files from (Source:)");
                return;
            }
            var cfg = new StringBuilder();
            cfg.AppendLine("mountpoint " + gDst.Text);
            cfg.AppendLine("cache_min " + gCacheMin.Text);
            cfg.AppendLine("cache_gbyte " + gCacheGB.Text);
            cfg.AppendLine("cache_dir " + gCacheDir.Text);

            int cache_sec = (int)(Convert.ToDouble(gCacheMin.Text) * 60);
            int cache_mbyte = (int)(Convert.ToDouble(gCacheGB.Text) * 1024);
            var cache_dir = gCacheDir.Text.TrimEnd(' ', '\\', '/') + "\\";
            cmfs = new TheFS(logger, cache_dir, cache_sec, cache_mbyte);
            foreach (var item in gList.Items) {
                var kvp = (KeyValuePair<string, string>)item;
                cfg.AppendLine("m_name " + kvp.Key);
                cfg.AppendLine("m_src " + kvp.Value);

                if (System.IO.Directory.Exists(kvp.Value))
                    cmfs.AddMount(kvp.Value, kvp.Key);
                else
                    MessageBox.Show("could not open directory; skipping " + kvp.Value);
            }

            var mp = gDst.Text.Substring(0, 1) + ":\\";
            logger.log(1, new string[] { "Now active @ " + mp });

            var thr = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                cmfs.Mount(mp, 0); // DokanNet.DokanOptions.DebugMode);
            }));
            thr.IsBackground = true;
            thr.Start();

            this.Controls.Remove(panel1);
            System.IO.File.WriteAllText("mixtapeFS.ini", cfg.ToString(), Encoding.UTF8);
            this.Text = "mixtapeFS @ " + mp;
        }

        private void gBrowseMount_Click(object sender, EventArgs e)
        {
            var b = new SaveFileDialog();
            b.CreatePrompt = false;
            b.OverwritePrompt = false;
            b.Title = "Select mountpoint where the output files should be served";
            if (DialogResult.OK == b.ShowDialog())
                gDst.Text = b.FileName;
        }

        private void gBrowseSource_Click(object sender, EventArgs e)
        {
            var b = new FolderBrowserDialog();
            b.Description = "Select folder to serve files from";
            if (DialogResult.OK == b.ShowDialog())
                gSrc.Text = b.SelectedPath;
        }

        private void gBrowseCache_Click(object sender, EventArgs e)
        {
            var b = new FolderBrowserDialog();
            b.Description = "Select folder to use as cache";
            if (DialogResult.OK == b.ShowDialog())
                gCacheDir.Text = b.SelectedPath;
        }

        private void gAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(gSrc.Text))
            {
                MessageBox.Show("you need to select a Source: folder to share files from");
                return;
            }
            if (string.IsNullOrWhiteSpace(gName.Text))
            {
                MessageBox.Show("you need to provide a name (an alias for this source folder which will appear in the mountpoint)");
                return;
            }
            gList.Items.Add(new KeyValuePair<string, string>(gName.Text, gSrc.Text));
        }

        private void gDel_Click(object sender, EventArgs e)
        {
            if (gList.SelectedIndex >= 0)
                gList.Items.RemoveAt(gList.SelectedIndex);
        }
    }
}
