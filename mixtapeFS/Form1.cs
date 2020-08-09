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
            cmfs = new TheFS(logger);
            cmfs.AddMount(@"C:\Users\ed\Music", "cmusic"); // HARDCODE
            cmfs.AddMount(@"Q:\music", "nas");

            var thr = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                cmfs.Mount(@"m:\", 0); // DokanNet.DokanOptions.DebugMode); // HARDCODE
            }));
            thr.IsBackground = true;
            thr.Start();

            Timer t = new Timer();
            t.Interval = 10;
            t.Start();
            t.Tick += t_Tick;

            gLog.Font = new Font("Consolas", gLog.Font.Size);

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
    }
}
