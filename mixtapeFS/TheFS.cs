using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DokanNet;
using FileAccess = DokanNet.FileAccess;
using System.IO;
using System.Security.AccessControl;

namespace mixtapeFS
{
    class TheFS : IDokanOperations
    {
        private const FileAccess DataAccess = FileAccess.ReadData | FileAccess.WriteData | FileAccess.AppendData |
                                              FileAccess.Execute |
                                              FileAccess.GenericExecute | FileAccess.GenericWrite |
                                              FileAccess.GenericRead;

        private const FileAccess DataWriteAccess = FileAccess.WriteData | FileAccess.AppendData |
                                                   FileAccess.Delete |
                                                   FileAccess.GenericWrite;

        Logger logger;
        Cache cache;
        Dictionary<string, string> mounts;

        public TheFS(Logger logger)
        {
            this.logger = logger;
            cache = new Cache(logger, @"c:\cmfsc\" + DateTime.UtcNow.Ticks);
            mounts = new Dictionary<string, string>();
        }

        void log(params string[] words)
        {
            logger.log(words);
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

            string volroot;
            if (!mounts.TryGetValue(mp_name, out volroot))
                return null;

            string ret = volroot + top_path;
            if (System.IO.Directory.Exists(ret))
                return ret;

            if (System.IO.File.Exists(ret))
                return ret;

            if (ret.EndsWith(".flac"))
                ret = ret.Substring(0, ret.Length - 5);

            if (System.IO.File.Exists(ret))
                return ret;

            return null;
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

            var src = lookup(filename);
            if (src == null)
                return DokanResult.FileNotFound;

            if (System.IO.Directory.Exists(src))
            {
                info.IsDirectory = true;
                return DokanResult.Success;
            }

            if (!System.IO.File.Exists(src) && src.EndsWith(".flac"))
            {
                src = src.Substring(0, src.Length - 5);
                if (!System.IO.File.Exists(src))
                    return DokanResult.FileNotFound;
            }

            var tcPath = cache.Get(src);

            if (tcPath != null && System.IO.File.Exists(tcPath))
                try
                {
                    info.Context = new FileStream(tcPath, FileMode.Open, System.IO.FileAccess.Read);
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
            if (top_path == null)
                return DokanResult.FileNotFound;

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
            if (src == null)
                return DokanResult.FileNotFound;

            var isDir = System.IO.Directory.Exists(src) || src == "";
            var tcPath = isDir? null : cache.Get(src);

            fileinfo.Attributes = isDir ? FileAttributes.Directory : FileAttributes.ReadOnly;
            fileinfo.LastAccessTime = DateTime.Now;
            fileinfo.LastWriteTime = null;
            fileinfo.CreationTime = null;
            if (tcPath != null && System.IO.File.Exists(tcPath))
                fileinfo.Length = new System.IO.FileInfo(tcPath).Length;

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
