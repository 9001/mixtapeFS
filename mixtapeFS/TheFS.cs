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
        const bool USING_TRAKTOR = false;
        const bool USING_REKORDBOX = true;

        public const string TC_EXT = ".flac"; //".mp3"; 

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
        HashSet<string> flacify, repack;

        public TheFS(Logger logger, string cachedir, int cacheSec, int cacheMB)
        {
            this.logger = logger;
            flacify = new HashSet<string>();
            repack = new HashSet<string>();

            if (USING_TRAKTOR && USING_REKORDBOX)
                throw new Exception("choose one");

            if (USING_TRAKTOR)
            {
                // TODO traktor is/was too broken to be worth even trying,
                // make this a gui option if that improves
                flacify.Add("opus"); // HARDCODE
                repack.Add("mp3"); // HARDCODE
                repack.Add("m4a");
                repack.Add("aac");
                repack.Add("ogg");
            }

            if (USING_REKORDBOX)
            {
                flacify.Add("opus"); // HARDCODE
                flacify.Add("ogg");
            }

            cache = new Cache(logger, cachedir + DateTime.UtcNow.Ticks, flacify, repack, cacheSec, cacheMB);
            mounts = new Dictionary<string, string>();
        }

        void log(int lv, params string[] words)
        {
            logger.log(lv, words);
        }

        public void AddMount(string src, string name)
        {
            mounts.Add(name, src);
        }

        public void Cleanup(string filename, IDokanFileInfo info)
        {
            log(5, "Cleanup", filename);
            try
            {
                if (info.Context != null)
                {
                    (info.Context as FileStream).Dispose();
                    cache.Release(lookup(filename));
                }
            }
            catch { }
            info.Context = null;
        }

        public void CloseFile(string filename, IDokanFileInfo info)
        {
            log(5, "CloseFile", filename);
            try
            {
                if (info.Context != null)
                {
                    (info.Context as FileStream).Dispose();
                    cache.Release(lookup(filename));
                }
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

            if (ret.EndsWith(TC_EXT))
                ret = ret.Substring(0, ret.Length - TC_EXT.Length);

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
            log(5, "CreateFile", filename);

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

            if (!System.IO.File.Exists(src) && src.EndsWith(TC_EXT))
            {
                src = src.Substring(0, src.Length - TC_EXT.Length);
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
            log(9, "DeleteDirectory", filename);

            return DokanResult.Error;
        }

        public NtStatus DeleteFile(string filename, IDokanFileInfo info)
        {
            log(9, "DeleteFile", filename);

            return DokanResult.Error;
        }

        public NtStatus FlushFileBuffers(
            string filename,
            IDokanFileInfo info)
        {
            log(9, "FlushFileBuffers", filename);

            return DokanResult.Error;
        }

        public NtStatus FindFiles(
            string filename,
            out IList<FileInformation> files,
            IDokanFileInfo info)
        {
            log(7, "FindFiles", filename);

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

            try
            {
                foreach (var dpath in System.IO.Directory.EnumerateDirectories(top_path))
                {
                    var dname = dpath.Substring(top_path.Length + 1);
                    files.Add(new FileInformation { FileName = dname, Attributes = FileAttributes.Directory });
                }

                foreach (var fpath in System.IO.Directory.EnumerateFiles(top_path))
                {
                    var fname = fpath.Substring(top_path.Length + 1);
                    foreach (var ext in flacify)
                        if (fname.EndsWith("." + ext))
                            fname += TC_EXT;

                    files.Add(new FileInformation { FileName = fname, Attributes = FileAttributes.ReadOnly });
                }
            }
            catch (UnauthorizedAccessException ex) {}

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
            log(7, "GetFileInformation", filename);
            fileinfo = new FileInformation { FileName = filename };

            var src = lookup(filename);
            if (src == null)
                return DokanResult.FileNotFound;

            var isDir = System.IO.Directory.Exists(src) || src == "";
            var tcPath = isDir ? null : cache.Get(src, false); //, false); //(traktor needs real size)

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
            log(9, "LockFile", filename);

            return DokanResult.Success;
        }

        public NtStatus MoveFile(
            string filename,
            string newname,
            bool replace,
            IDokanFileInfo info)
        {
            log(9, "MoveFile", filename);

            return DokanResult.Error;
        }

        public NtStatus ReadFile(
            string filename,
            byte[] buffer,
            out int readBytes,
            long offset,
            IDokanFileInfo info)
        {
            log(9, "ReadFile", filename);

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
            log(9, "SetEndOfFile", filename);

            return DokanResult.Error;
        }

        public NtStatus SetAllocationSize(string filename, long length, IDokanFileInfo info)
        {
            log(9, "SetAllocationSize", filename);

            return DokanResult.Error;
        }

        public NtStatus SetFileAttributes(
            string filename,
            FileAttributes attr,
            IDokanFileInfo info)
        {
            log(9, "SetFileAttributes", filename);

            return DokanResult.Error;
        }

        public NtStatus SetFileTime(
            string filename,
            DateTime? ctime,
            DateTime? atime,
            DateTime? mtime,
            IDokanFileInfo info)
        {
            log(9, "SetFileTime", filename);

            return DokanResult.Error;
        }

        public NtStatus UnlockFile(string filename, long offset, long length, IDokanFileInfo info)
        {
            log(9, "UnlockFile", filename);

            return DokanResult.Success;
        }

        public NtStatus Mounted(IDokanFileInfo info)
        {
            log(9, "Mounted");

            return DokanResult.Success;
        }

        public NtStatus Unmounted(IDokanFileInfo info)
        {
            log(9, "Unmounted");

            return DokanResult.Success;
        }

        public NtStatus GetDiskFreeSpace(
            out long freeBytesAvailable,
            out long totalBytes,
            out long totalFreeBytes,
            IDokanFileInfo info)
        {
            log(9, "GetDiskFreeSpace");

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
            log(9, "WriteFile", filename);

            writtenBytes = 0;
            return DokanResult.Error;
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features,
            out string fileSystemName, out uint maximumComponentLength, IDokanFileInfo info)
        {
            log(9, "GetVolumeInformation");

            volumeLabel = "mixtapeFS";
            features = FileSystemFeatures.None;
            fileSystemName = string.Empty;
            maximumComponentLength = 256;
            return DokanResult.Success;
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections,
            IDokanFileInfo info)
        {
            log(9, "GetFileSecurity", fileName);

            security = null;
            return DokanResult.Error;
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections,
            IDokanFileInfo info)
        {
            log(9, "SetFileSecurity", fileName);

            return DokanResult.Error;
        }

        public NtStatus EnumerateNamedStreams(string fileName, IntPtr enumContext, out string streamName,
            out long streamSize, IDokanFileInfo info)
        {
            log(9, "EnumerateNamedStreams", fileName);

            streamName = string.Empty;
            streamSize = 0;
            return DokanResult.NotImplemented;
        }

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            log(9, "FindStreams", fileName);

            streams = new FileInformation[0];
            return DokanResult.NotImplemented;
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files,
            IDokanFileInfo info)
        {
            log(9, "FindFilesWithPattern", fileName, searchPattern);
            
            // traktor support (does not load tags otherwise)
            string hit = lookup(fileName + "\\" + searchPattern);
            if (hit != null && hit != "")
            {
                bool isdir = System.IO.Directory.Exists(hit); 
                files = new FileInformation[1];
                files[0] = new FileInformation()
                {
                    Attributes = isdir ? FileAttributes.Directory : FileAttributes.ReadOnly,
                    LastAccessTime = DateTime.Now,
                    LastWriteTime = null,
                    CreationTime = null,
                    FileName = fileName + "\\" + searchPattern,
                    Length = isdir ? 0 : new FileInfo(hit).Length
                };
                return DokanResult.Success;
            }

            files = new FileInformation[0];
            return DokanResult.NotImplemented;
        }
    }
}
