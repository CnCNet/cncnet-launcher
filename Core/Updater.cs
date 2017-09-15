using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Updater.Core
{
    public class UpdaterEventArgs : EventArgs
    {
        public int progress;
        public string status;

        public UpdaterEventArgs(int progress, string status)
        {
            this.progress = progress;
            this.status = status;
        }
    }

    public class Updater
    {
        List<UpdateFile> _files;
        long _totalPosition;
        long _totalSize;
        long _lastProgressPosition;
        long _nextProgressTick;
        long _lastETAPosition;
        long _nextETATick;
        string _ETA;
        string _currentFile;
        string _updateDir;

        public delegate void StatusChangedEventHandler(object sender, UpdaterEventArgs e);

        public event StatusChangedEventHandler StatusChanged;

        protected virtual void OnStatusChanged(UpdaterEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }

        public Updater(List<UpdateFile> files, string updateDir)
        {
            _files = files;
            _updateDir = updateDir;
            _ETA = "calculating";
        }

        private void UpdateDownloadStatus()
        {
            UpdateDownloadStatus(false);
        }

        private void UpdateDownloadStatus(bool forceUpdate)
        {
            if (DateTime.Now.Ticks < _nextProgressTick && !forceUpdate)
                return;

            _nextProgressTick = DateTime.Now.Ticks + TimeSpan.TicksPerSecond;

            int bytesPerSecond = (int)Math.Max(0, (_totalPosition - _lastProgressPosition));
            _lastProgressPosition = _totalPosition;

            if (DateTime.Now.Ticks >= _nextETATick || _totalPosition == _totalSize)
            {
                _ETA = ETA((int)Math.Max(0, (_totalPosition - _lastETAPosition) / 5));
                _lastETAPosition = _totalPosition;
                _nextETATick = DateTime.Now.Ticks + TimeSpan.TicksPerSecond * 5;
            }

            var status = String.Format(
                "Downloading at {0}/s of {1} / {2}, ETA {3}",
                BytesToString(bytesPerSecond),
                BytesToString(_totalPosition),
                BytesToString(_totalSize),
                _ETA
            );

            OnStatusChanged(new UpdaterEventArgs((_totalPosition == _totalSize ? 100 : (int)(((double)_totalPosition / _totalSize) * 100)), status));
        }

        private void UpdateInstallStatus(string currentFile = null)
        {
            UpdateInstallStatus(false, currentFile);
        }

        private void UpdateInstallStatus(bool forceUpdate, string currentFile = null)
        {
            if (DateTime.Now.Ticks < _nextProgressTick && !forceUpdate)
                return;

            if (currentFile != null)
                _currentFile = currentFile;

            _nextProgressTick = DateTime.Now.Ticks + TimeSpan.TicksPerSecond;
            OnStatusChanged(new UpdaterEventArgs(
                (_totalPosition >= _totalSize ? 100 : (int)(((double)_totalPosition / _totalSize) * 100)),
                (_totalPosition >= _totalSize ? "Ready!" : "Extracting " + _currentFile + "...")
            ));
        }

        private string BytesToString(long bytes)
        {
            double val = bytes;
            string ext = "B";

            if (val >= 1024 * 1024 * 1024)
            {
                val /= 1024 * 1024 * 1024;
                ext = "GB";
            }
            else if (val >= 1024 * 1024)
            {
                val /= 1024 * 1024;
                ext = "MB";
            }
            else if (val >= 1024)
            {
                val /= 1024;
                ext = "kB";
            }

            if (val < 100)
                return String.Format("{0:0.#} {1}", val, ext);

            return String.Format("{0:0} {1}", val, ext);
        }

        private string ETA(int bytesPerSecond)
        {
            long bytesLeft = Math.Max(0, _totalSize - _totalPosition);
            long secondsLeft = (long)(bytesLeft / (double)bytesPerSecond);

            if (bytesLeft == 0)
                return "now";

            long seconds = secondsLeft % 60;
            long minutes = secondsLeft / 60;
            long hours = 0;
            long days = 0;

            if (minutes > 60)
            {
                minutes = minutes % 60;
                hours = minutes / 60;
            }

            if (hours > 24)
            {
                hours = hours % 24;
                days = hours / 24;
            }

            StringBuilder timeString = new StringBuilder();

            if (days > 0)
            {
                timeString.Append(days);
                timeString.Append(" ");
                timeString.Append(days == 1 ? "day" : "days");
                timeString.Append(" ");
            }

            if (hours > 0)
            {
                timeString.Append(hours);
                timeString.Append(" ");
                timeString.Append(hours == 1 ? "hour" : "hours");
                timeString.Append(" ");
            }

            if (minutes > 0)
            {
                timeString.Append(minutes);
                timeString.Append(" ");
                timeString.Append(minutes == 1 ? "minute" : "minutes");
                timeString.Append(" ");
            }

            timeString.Append(seconds);
            timeString.Append(" ");
            timeString.Append((seconds == 1 ? "second" : "seconds"));

            return timeString.ToString();
        }

        private void DownloadFile(UpdateFile file)
        {
            long pos;
            int retry = 0;
            var updateFilePath = UpdateFilePath(file);

            if (ToDownload(file) == 0)
                return;

            while (retry < 3)
            {
                pos = 0;

                try
                {
                    Log.Info("Downloading {0}...", file.URL);

                    string dir = Path.GetDirectoryName(updateFilePath);
                    if (dir.Length > 0)
                    {
                        Directory.CreateDirectory(dir);
                    }

                    if (file.SHA1 == null && File.Exists(file.Path))
                    {
                        Log.Info("Skipping {0} because no hash and file exists...", file.Path);
                        break;
                    }

                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(file.URL);
                    req.Timeout = 60000;

                    if (File.Exists(updateFilePath))
                    {
                        req.AddRange(file.Size - ToDownload(file));
                    }

                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                    using (Stream responseStream = resp.GetResponseStream())
                    using (PositionedStream ps = new PositionedStream(responseStream))
                    using (FileStream fs = new FileStream(updateFilePath, FileMode.Append))
                    using (BufferedStream bs = new BufferedStream(fs))
                    {
                        byte[] buf = new byte[4096];
                        int i;
                        while ((i = ps.Read(buf, 0, buf.Length)) > 0)
                        {
                            bs.Write(buf, 0, i);
                            long inc = ps.Position - pos;
                            pos += inc;
                            _totalPosition += inc;

                            UpdateDownloadStatus();
                        }
                    }

                    break;

                }
                catch (Exception e)
                {
                    _totalPosition -= pos;
                    UpdateDownloadStatus(true);

                    if (++retry == 3)
                    {
                        Log.Error(e.ToString(), e.Message);
                        throw e;
                    }
                    else
                    {
                        Log.Warning(e.ToString(), e.Message);
                        Log.Warning("File download failed, retrying...");
                    }
                }
            }
        }

        private void InstallFile(UpdateFile file)
        {
            var updateFilePath = UpdateFilePath(file);

            Log.Info("Installing {0} from {1}...", file.Path, file.SHA1 + ".gz");

            UpdateInstallStatus(true, file.Path);

            if (file.SHA1 == null && File.Exists(file.Path))
            {
                Log.Info("Skipping {0} because no hash and file exists...", file.Path);
                return;
            }

            string dir = Path.GetDirectoryName(file.Path);
            if (dir.Length > 0)
                Directory.CreateDirectory(dir);

            if (File.Exists(file.Path) && Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + file.Path == Application.ExecutablePath)
            {
                Log.Warning("Detected update to self, trying to cope with it.");

                if (File.Exists(Application.ExecutablePath + ".old"))
                    File.Delete(Application.ExecutablePath + ".old");

                File.Move(Application.ExecutablePath, Application.ExecutablePath + ".old");
            }

            if (file.Size == 0)
            {
                using (FileStream ofs = new FileStream(file.Path, FileMode.Create))
                {
                    return;
                }
            }

            try
            {
                using (FileStream ifs = File.OpenRead(updateFilePath))
                using (GZipStream gzs = new GZipStream(ifs, CompressionMode.Decompress))
                using (FileStream ofs = new FileStream(file.Path, FileMode.Create))
                using (BufferedStream bs = new BufferedStream(ofs))
                {
                    byte[] buf = new byte[4096];
                    int i;
                    while ((i = gzs.Read(buf, 0, buf.Length)) > 0)
                    {
                        bs.Write(buf, 0, i);
                    }
                }
            }
            catch (InvalidDataException)
            {
                // corrupt downloads and dest files need to be removed
                File.Delete(updateFilePath);
                File.Delete(file.Path);
                throw;
            }

            _totalPosition += file.Size;
            UpdateInstallStatus(true);
        }

        public string UpdateFilePath(UpdateFile uf)
        {
            return _updateDir + Path.DirectorySeparatorChar + uf.SHA1 + ".gz";
        }

        public long ToDownload(UpdateFile uf)
        {
            var ufp = UpdateFilePath(uf);

            if (!File.Exists(ufp))
                return uf.Size;

            return uf.Size - new FileInfo(ufp).Length;
        }

        public void Run()
        {
            foreach (UpdateFile file in _files)
            {
                _totalSize += ToDownload(file);
            }

            Log.Info("Calculated total download of {0} bytes.", _totalSize);

            foreach (UpdateFile file in _files)
            {
                DownloadFile(file);
            }

            Log.Info("Extracting updates...");

            _totalPosition = 0;
            foreach (UpdateFile file in _files)
            {
                InstallFile(file);
            }

            Log.Info("Removing update files...");

            // remove update files only after everything has been extracted
            foreach (UpdateFile file in _files)
            {
                File.Delete(UpdateFilePath(file));
            }

            _totalPosition = _totalSize;
            UpdateInstallStatus(true);
        }
    }
}

