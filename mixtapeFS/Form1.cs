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
        List<string> msgs;

        private void Form1_Load(object sender, EventArgs e)
        {
            msgs = new List<string>();
            cmfs = new TheFS();
            cmfs.AddMount(@"C:\Users\ed\Music", "music");

            var thr = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                cmfs.Mount(@"m:\", 0); // DokanNet.DokanOptions.DebugMode);
            }));
            thr.IsBackground = true;
            thr.Start();

            Timer t = new Timer();
            t.Interval = 10;
            t.Start();
            t.Tick += t_Tick;

            //this.DoubleBuffered = true;
            //typeof(Label).InvokeMember("DoubleBuffered", System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, gLog, new object[] { true });
        }

        void t_Tick(object sender, EventArgs e)
        {
            string[] new_msgs;
            lock (cmfs.msgs)
            {
                new_msgs = cmfs.msgs.ToArray();
                cmfs.msgs.Clear();
            }
            
            if (new_msgs.Length == 0)
                return;

            msgs.AddRange(new_msgs);
            if (msgs.Count > 200)
            {
                msgs.RemoveRange(0, msgs.Count - 200);
            }

            string txt = "";
            for (int a = msgs.Count - 1; a >= 0; a--)
                txt += msgs[a] + "\r\n";

            gLog.Text = txt;
        }
    }
}
