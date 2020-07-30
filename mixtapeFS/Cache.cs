using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace mixtapeFS
{
    class Centry
    {
        public bool busy;
        public int refs;
        public string origPath;
        public string tcPath;

        public Centry(string origPath)
        {
            this.origPath = origPath;
            this.busy = true;
            this.refs = 0;
            this.tcPath = null;
        }
    }

    class Cache
    {
        string root;
        Logger logger;
        Dictionary<string, Centry> known;
        HashSet<string> flacify, repack;
        
        public Cache(Logger logger, string root)
        {
            this.root = root;
            this.logger = logger;
            System.IO.Directory.CreateDirectory(root);

            known = new Dictionary<string, Centry>();
            
            flacify = new HashSet<string>();
            flacify.Add("opus"); // HARDCODE
            
            repack = new HashSet<string>();
            repack.Add("mp3"); // HARDCODE
            repack.Add("m4a");
            repack.Add("aac");
            repack.Add("ogg");

            var thr = new Thread(new ThreadStart(janny));
            thr.IsBackground = true;
            thr.Start();
        }

        void log(params string[] words)
        {
            logger.log(words);
        }

        public void Close(string origPath)
        {
            Centry ce;
            lock (known)
                if (known.TryGetValue(origPath, out ce))
                    ce.refs--;
        }

        public string Get(string origPath)
        {
            var ext = origPath.Substring(origPath.LastIndexOf('.') + 1).ToLower();
            var tcext = ext;

            var copy = repack.Contains(ext);
            if (!copy)
            {
                if (!flacify.Contains(ext))
                    return origPath;

                tcext += ".flac";
            }

            Centry ce;
            var yaranaika = false;
            lock (known)
            {
                if (!known.TryGetValue(origPath, out ce))
                {
                    ce = new Centry(origPath);
                    known.Add(origPath, ce);
                    yaranaika = true;
                }
                ce.refs++;
            }
            //log("cache get", origPath);
            
            while (true)
            {
                lock (known)
                    if (!ce.busy || yaranaika)
                        break;

                System.Threading.Thread.Sleep(100);
                log("cache waiting", origPath);
            }

            //log("cache FOUND", origPath);
            if (ce.tcPath != null)
                return ce.tcPath;

            string hash = "";
            using (MD5 md5 = MD5.Create())
            {
                var digest = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(origPath));
                hash = Convert.ToBase64String(digest, Base64FormattingOptions.None);
                hash = hash.Replace('+', '-').Replace('/', '_').TrimEnd('=');
            }

            // safe, only this thread can manip this ce in known rn
            ce.tcPath = string.Format(@"{0}\{1:X}-{2}.{3}", root, DateTime.UtcNow.Ticks / 10000, hash, tcext);

            log("cache create", ce.tcPath);
            
            var cmd = Z.EscapeArguments(
                "-hide_banner",
                "-nostdin",
                "-i", origPath,
                "-metadata", "iTunNORM=",
                "-metadata", "iTunSMPB="
            );

            if (copy)
                cmd += " " + Z.EscapeArguments(
                    "-c", "copy"
                );
            else
                cmd += " " + Z.EscapeArguments(
                    // TODO maybe want some -af volume clamping here
                    "-sample_fmt", "s16",
                    "-ar", "44100",
                    "-f", "flac"
                );

            cmd += " " + Z.EscapeArguments(ce.tcPath);

            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "ffmpeg";
            p.StartInfo.Arguments = cmd;
            p.Start();
            p.WaitForExit();

            lock (known)
            {
                ce.busy = false;
            }
            //log("cache DONE", origPath);
            return ce.tcPath;
        }

        void janny()
        {
            while (true)
            {
                Thread.Sleep(9001);
            }
        }
    }
}
