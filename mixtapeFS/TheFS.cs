using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DokanNet;
using FileAccess = DokanNet.FileAccess;
using System.IO;
using System.Security.AccessControl;
using System.Text.RegularExpressions;

namespace mixtapeFS
{
    class Z
    {
        // http://csharptest.net/529/how-to-correctly-escape-command-line-arguments-in-c/index.html
        public static string EscapeArguments(params string[] args)
        {
            StringBuilder arguments = new StringBuilder();
            Regex invalidChar = new Regex("[\x00\x0a\x0d]");//  these can not be escaped
            Regex needsQuotes = new Regex(@"\s|""");//          contains whitespace or two quote characters
            Regex escapeQuote = new Regex(@"(\\*)(""|$)");//    one or more '\' followed with a quote or end of string
            for (int carg = 0; args != null && carg < args.Length; carg++)
            {
                if (args[carg] == null) { throw new ArgumentNullException("args[" + carg + "]"); }
                if (invalidChar.IsMatch(args[carg])) { throw new ArgumentOutOfRangeException("args[" + carg + "]"); }
                if (args[carg] == String.Empty) { arguments.Append("\"\""); }
                else if (!needsQuotes.IsMatch(args[carg])) { arguments.Append(args[carg]); }
                else
                {
                    arguments.Append('"');
                    arguments.Append(escapeQuote.Replace(args[carg], m =>
                    m.Groups[1].Value + m.Groups[1].Value +
                    (m.Groups[2].Value == "\"" ? "\\\"" : "")
                    ));
                    arguments.Append('"');
                }
                if (carg + 1 < args.Length)
                    arguments.Append(' ');
            }
            return arguments.ToString();
        }
    }

    class TheFS : IDokanOperations
    {
        private const FileAccess DataAccess = FileAccess.ReadData | FileAccess.WriteData | FileAccess.AppendData |
                                              FileAccess.Execute |
                                              FileAccess.GenericExecute | FileAccess.GenericWrite |
                                              FileAccess.GenericRead;

        private const FileAccess DataWriteAccess = FileAccess.WriteData | FileAccess.AppendData |
                                                   FileAccess.Delete |
                                                   FileAccess.GenericWrite;

        public List<String> msgs;
        Dictionary<string, string> mounts;
        HashSet<string> busyFiles;

        public TheFS()
        {
            mounts = new Dictionary<string, string>();
            busyFiles = new HashSet<string>();
            msgs = new List<string>();
        }

        void log(params string[] words)
        {
            //return;
            lock (msgs)
            {
                msgs.Add(System.DateTime.UtcNow.ToString("HH:mm:ss.ffff [") + string.Join("] [", words) + "]");
            }
        }

        public void AddMount(string src, string name)
        {
            mounts.Add(name, src);
        }

        public void Cleanup(string filename, IDokanFileInfo info)
        {
            log("Cleanup", filename);
            try
            {
                if (info.Context != null)
                    (info.Context as FileStream).Dispose();
            }
            catch { }
            info.Context = null;
        }

        public void CloseFile(string filename, IDokanFileInfo info)
        {
            log("CloseFile", filename);
            try
            {
                if (info.Context != null)
                    (info.Context as FileStream).Dispose();
            }
            catch { }
            info.Context = null;
        }

        string lookup(string virt)
        {
            if (virt == "\\")
                return "";

            var ofs = virt.IndexOf("\\", 1);
            if (ofs < 0) ofs = virt.Length;

            var top_path = virt.Substring(ofs);
            var mp_name = virt.Substring(1, ofs - 1).Trim('\\');

            try
            {
                if (mounts.ContainsKey(mp_name))
                    return mounts[mp_name] + top_path;
            }
            catch { }
            return "\\fhwuaeifhweuajifwefgva";
        }

        public NtStatus CreateFile(
            string filename,
            FileAccess access,
            FileShare share,
            FileMode mode,
            FileOptions options,
            FileAttributes attributes,
            IDokanFileInfo info)
        {
            log("CreateFile", filename);
            if (filename.EndsWith(".ogg"))
                Console.WriteLine("fds");

            if (info.IsDirectory)
                return DokanResult.Success;

            var readOrWrite = (access & DataAccess) != 0;
            var readOnly = (access & DataWriteAccess) == 0;
            
            if (!readOrWrite)
                return DokanResult.Success;

            if (!readOnly)
                return DokanResult.Success;

            if (mode != FileMode.Open)
                return DokanResult.AccessDenied;

            if (filename == "\\")
                return DokanResult.Success;

            if (filename.EndsWith("*"))
                return DokanResult.Success; // tabcomplete

            if (access != FileAccess.ReadData && access != FileAccess.GenericRead)
                return DokanResult.Success;

            while (true)
            {
                lock (busyFiles)
                {
                    if (!busyFiles.Contains(filename))
                    {
                        busyFiles.Add(filename);
                        break;
                    }
                }
                System.Threading.Thread.Sleep(10);
            }

            bool isdir = false;
            var dst = "c:\\cmfsc" + filename; // HARDCODE
            if (!System.IO.File.Exists(dst))
            {
                var src = lookup(filename);
                isdir = System.IO.Directory.Exists(src);
                
                bool copy = true;
                if (!System.IO.File.Exists(src))
                {
                    copy = false;
                    src = src.Substring(0, src.Length - 5); // ".opus.flac" HARDCODE UNSAFE
                }
                if (!System.IO.File.Exists(src))
                {
                    lock (busyFiles)
                        busyFiles.Remove(filename);  // HACK (remove along with ^)
                    
                    return DokanResult.FileNotFound;
                }

                // TODO move all this to a media manager thing
                var cmd = Z.EscapeArguments(
                    "-hide_banner",
                    "-nostdin",
                    "-i", src,
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

                cmd += " " + Z.EscapeArguments(dst);

                var dst_dir = dst.Substring(0, dst.LastIndexOf('\\'));
                if (!System.IO.Directory.Exists(dst_dir))
                    System.IO.Directory.CreateDirectory(dst_dir);  // == mkdir -p

                var p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "ffmpeg";
                p.StartInfo.Arguments = cmd;
                p.Start();
                p.WaitForExit();
            }

            lock (busyFiles)
                busyFiles.Remove(filename);  // actually fine

            if (isdir)
            {
                info.IsDirectory = true;
                return DokanResult.Success;
            }

            if (System.IO.File.Exists(dst))
                try
                {
                    info.Context = new FileStream(dst, FileMode.Open, System.IO.FileAccess.Read);
                    return DokanResult.Success;
                }
                catch { }
            
            return DokanResult.FileNotFound;            
        }

        public NtStatus DeleteDirectory(string filename, IDokanFileInfo info)
        {
            log("DeleteDirectory", filename);

            return DokanResult.Error;
        }

        public NtStatus DeleteFile(string filename, IDokanFileInfo info)
        {
            log("DeleteFile", filename);

            return DokanResult.Error;
        }

        public NtStatus FlushFileBuffers(
            string filename,
            IDokanFileInfo info)
        {
            log("FlushFileBuffers", filename);

            return DokanResult.Error;
        }

        public NtStatus FindFiles(
            string filename,
            out IList<FileInformation> files,
            IDokanFileInfo info)
        {
            log("FindFiles", filename);

            files = new List<FileInformation>();
            if (filename == "\\")
            {
                foreach (var mp in mounts)
                {
                    var fileinfo = new FileInformation { FileName = mp.Key };

                    fileinfo.Attributes = FileAttributes.Directory;
                    fileinfo.LastAccessTime = DateTime.Now;
                    fileinfo.LastWriteTime = null;
                    fileinfo.CreationTime = null;

                    files.Add(fileinfo);
                }
                return DokanResult.Success;
            }

            var top_path = lookup(filename);
            if (System.IO.File.Exists(top_path))
                return DokanResult.Success;

            foreach (var dpath in System.IO.Directory.EnumerateDirectories(top_path))
            {
                var dname = dpath.Substring(top_path.Length + 1);
                files.Add(new FileInformation { FileName = dname, Attributes = FileAttributes.Directory });
            }

            foreach (var fpath in System.IO.Directory.EnumerateFiles(top_path))
            {
                var fname = fpath.Substring(top_path.Length + 1);
                if (fname.EndsWith(".opus")) // HARDCODE
                    fname += ".flac";

                files.Add(new FileInformation { FileName = fname, Attributes = FileAttributes.ReadOnly });
            }

            for (var a = 0; a < files.Count; a++)
            {
                var fi = files[a];
                fi.LastAccessTime = DateTime.Now;
                fi.LastWriteTime = null;
                fi.CreationTime = null;
            }

            return DokanResult.Success;
        }
        
        public NtStatus GetFileInformation(
            string filename,
            out FileInformation fileinfo,
            IDokanFileInfo info)
        {
            log("GetFileInformation", filename);

            fileinfo = new FileInformation { FileName = filename };
            var src = lookup(filename);
            
            while (true)
            {
                lock (busyFiles)
                    if (!busyFiles.Contains(filename))
                        break;

                System.Threading.Thread.Sleep(10);
            }

            fileinfo.Attributes = (System.IO.Directory.Exists(src) || src == "") ? FileAttributes.Directory : FileAttributes.ReadOnly;
            fileinfo.LastAccessTime = DateTime.Now;
            fileinfo.LastWriteTime = null;
            fileinfo.CreationTime = null;
            try
            {
                var dst = "c:\\cmfsc" + filename;
                if (System.IO.File.Exists(dst))
                    fileinfo.Length = new System.IO.FileInfo(dst).Length;
            }
            catch { }

            return DokanResult.Success;
        }

        public NtStatus LockFile(
            string filename,
            long offset,
            long length,
            IDokanFileInfo info)
        {
            log("LockFile", filename);

            return DokanResult.Success;
        }

        public NtStatus MoveFile(
            string filename,
            string newname,
            bool replace,
            IDokanFileInfo info)
        {
            log("MoveFile", filename);

            return DokanResult.Error;
        }

        public NtStatus ReadFile(
            string filename,
            byte[] buffer,
            out int readBytes,
            long offset,
            IDokanFileInfo info)
        {
            log("ReadFile", filename);

            var close = false;
            if (info.Context == null)
            {
                close = true;
                CreateFile(filename, FileAccess.ReadData, FileShare.Read, FileMode.Open, FileOptions.None, FileAttributes.Normal, info);
            }
            
            var f = info.Context as FileStream;
            if (f == null)
            {
                readBytes = 0;
                return DokanResult.FileNotFound;
            }

            lock (f)
            {
                f.Position = offset;
                readBytes = f.Read(buffer, 0, buffer.Length);
            }

            if (close)
                CloseFile(filename, info);

            return DokanResult.Success;
        }

        public NtStatus SetEndOfFile(string filename, long length, IDokanFileInfo info)
        {
            log("SetEndOfFile", filename);

            return DokanResult.Error;
        }

        public NtStatus SetAllocationSize(string filename, long length, IDokanFileInfo info)
        {
            log("SetAllocationSize", filename);

            return DokanResult.Error;
        }

        public NtStatus SetFileAttributes(
            string filename,
            FileAttributes attr,
            IDokanFileInfo info)
        {
            log("SetFileAttributes", filename);

            return DokanResult.Error;
        }

        public NtStatus SetFileTime(
            string filename,
            DateTime? ctime,
            DateTime? atime,
            DateTime? mtime,
            IDokanFileInfo info)
        {
            log("SetFileTime", filename);

            return DokanResult.Error;
        }

        public NtStatus UnlockFile(string filename, long offset, long length, IDokanFileInfo info)
        {
            log("UnlockFile", filename);

            return DokanResult.Success;
        }

        public NtStatus Mounted(IDokanFileInfo info)
        {
            log("Mounted");

            return DokanResult.Success;
        }

        public NtStatus Unmounted(IDokanFileInfo info)
        {
            log("Unmounted");

            return DokanResult.Success;
        }

        public NtStatus GetDiskFreeSpace(
            out long freeBytesAvailable,
            out long totalBytes,
            out long totalFreeBytes,
            IDokanFileInfo info)
        {
            log("GetDiskFreeSpace");

            freeBytesAvailable = 512 * 1024 * 1024;
            totalBytes = 1024 * 1024 * 1024;
            totalFreeBytes = 512 * 1024 * 1024;
            return DokanResult.Success;
        }

        public NtStatus WriteFile(
            string filename,
            byte[] buffer,
            out int writtenBytes,
            long offset,
            IDokanFileInfo info)
        {
            log("WriteFile", filename);

            writtenBytes = 0;
            return DokanResult.Error;
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
            out string fileSystemName, out uint maximumComponentLength, IDokanFileInfo info)
        {
            log("GetVolumeInformation");

            volumeLabel = "mixtapeFS";
            features = FileSystemFeatures.None;
            fileSystemName = string.Empty;
            maximumComponentLength = 256;
            return DokanResult.Error;
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections,
            IDokanFileInfo info)
        {
            log("GetFileSecurity", fileName);

            security = null;
            return DokanResult.Error;
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
            IDokanFileInfo info)
        {
            log("SetFileSecurity", fileName);

            return DokanResult.Error;
        }

        public NtStatus EnumerateNamedStreams(string fileName, IntPtr enumContext, out string streamName,
            out long streamSize, IDokanFileInfo info)
        {
            log("EnumerateNamedStreams", fileName);

            streamName = string.Empty;
            streamSize = 0;
            return DokanResult.NotImplemented;
        }

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            log("FindStreams", fileName);

            streams = new FileInformation[0];
            return DokanResult.NotImplemented;
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files,
            IDokanFileInfo info)
        {
            log("FindFilesWithPattern", fileName);

            files = new FileInformation[0];
            return DokanResult.NotImplemented;
        }
    }
}
