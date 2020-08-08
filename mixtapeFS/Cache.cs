using System;
using System.Collections.Generic;
using System.IO;
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
        public long active;
        public string origPath;
        public string tcPath;
        public long sz;

        public Centry(string origPath)
        {
            this.origPath = origPath;
            busy = true;
            refs = 0;
            active = DateTime.UtcNow.Ticks / Z.SEC;
            tcPath = null;
            sz = 0;
        }
    }

    class Cache
    {
        const long CACHE_MAX_SEC = 600; // HARDCODE
        const long CACHE_MAX_MBYTE = 8192;

        string root;
        Logger logger;
        long sz;
        Dictionary<string, Centry> known;
        HashSet<string> flacify, repack;
        
        public Cache(Logger logger, string root)
        {
            this.root = root;
            this.logger = logger;
            System.IO.Directory.CreateDirectory(root);
            sz = 0;

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

        void log(int lv, params string[] words)
        {
            logger.log(lv, words);
        }

        public void Release(string origPath)
        {
            if (origPath == null || origPath == "")
                return;

            //if (!System.IO.Directory.Exists(origPath)) // TODO awful
            //    log(4, "cache releasing", origPath);
            
            Centry ce;
            lock (known)
                if (known.TryGetValue(origPath, out ce))
                {
                    ce.refs--;
                    ce.active = DateTime.UtcNow.Ticks / Z.SEC;
                    log(4, "cache released", ce.refs.ToString(), origPath);
                }
        }

        public string Get(string origPath, bool refcount = true, bool need_conv = true)
        {
            var ext = origPath.Substring(origPath.LastIndexOf('.') + 1).ToLower();
            var tcext = ext;

            var copy = repack.Contains(ext);
            if (!copy)
            {
                if (!flacify.Contains(ext))
                    return origPath;

                tcext += TheFS.TC_EXT;
            }

            Centry ce;
            var yaranaika = false;
            lock (known)
            {
                if (!known.TryGetValue(origPath, out ce) && need_conv)
                {
                    ce = new Centry(origPath);
                    known.Add(origPath, ce);
                    yaranaika = true;
                }
                if (need_conv && refcount)
                    ce.refs++;
            }
            //log("cache get", origPath);

            if (ce == null)
            {
                log(3, "cache nullret", origPath);
                return null;
            }
            
            while (true)
            {
                lock (known)
                    if (!ce.busy || yaranaika)
                        break;

                log(3, "cache waiting", origPath);
                System.Threading.Thread.Sleep(100);
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
            ce.tcPath = string.Format(@"{0}\{1:X}-{2}.{3}", root, DateTime.UtcNow.Ticks / Z.MSEC, hash, tcext);

            log(4, "cache gen ", origPath);
            
            var cmd = Z.EscapeArguments(
                "-hide_banner",
                "-nostdin",
                "-i", origPath
            );

            if (ext == "flac" || ext == "ogg" || ext == "opus")
                cmd += " " + Z.EscapeArguments(
                    "-map_metadata", "0:s:0"  // sigh
                );
            
            cmd += " " + Z.EscapeArguments(
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
                    //"-c:a", "libmp3lame",
                    //"-b:a", "320k"
                );

            cmd += " " + Z.EscapeArguments(ce.tcPath);

            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "ffmpeg";
            p.StartInfo.Arguments = cmd;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            p.Start();
            p.WaitForExit();
            
            try
            {
                ce.sz = new FileInfo(ce.tcPath).Length;
            }
            catch
            {
                lock (known)
                {
                    ce.tcPath = null;
                    ce.busy = false;
                    ce.active = 0;
                    known.Remove(origPath);
                    return null;
                }
            }

            lock (known)
            {
                ce.busy = false;
                ce.active = DateTime.UtcNow.Ticks / Z.SEC;
                this.sz += ce.sz;
            }
            log(4, "cache DONE", this.sz.ToString(), origPath);
            return ce.tcPath;
        }

        void janny()
        {
            while (true)
            {
                Thread.Sleep(1);
                var drop = new List<string>();
                var now = DateTime.UtcNow.Ticks / Z.SEC;
                lock (known)
                {
                    foreach (var e in known)
                        if (e.Value.refs == 0 && now - e.Value.active >= CACHE_MAX_SEC)
                            drop.Add(e.Key);

                    foreach (var k in drop)
                    {
                        var ce = known[k];
                        log(3, "cache drop", (now - ce.active).ToString(), k);
                        System.IO.File.Delete(ce.tcPath);
                        this.sz -= ce.sz;
                        known.Remove(k);
                    }
                }

                drop.Clear();
                if (sz / (1024 * 1024) >= CACHE_MAX_MBYTE)
                {
                    lock (known)
                    {
                        var lst = known.OrderBy(o => o.Value.active).ToList();
                        foreach (var e in lst)
                        {
                            if (e.Value.refs == 0)
                            {
                                var ce = known[e.Key];
                                log(3, "cache drop SIZE", (sz / (1024 * 1024)).ToString(), e.Key);
                                System.IO.File.Delete(ce.tcPath);
                                this.sz -= ce.sz;
                                known.Remove(e.Key);
                                if (sz / (1024 * 1024) < CACHE_MAX_MBYTE)
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}
