using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Updater.Core
{
    public static class Bootstrap
    {
        public static void Run(IConfiguration c)
        {
            Application.EnableVisualStyles();
            Directory.SetCurrentDirectory(c.InstallDir);

            Log.Info("Saved ETag: {0}", c.ETag);

            if (File.Exists(Application.ExecutablePath + ".old"))
            {
                Log.Info("Cleanup: Removing old {0}.old", Application.ExecutablePath);
                File.Delete(Application.ExecutablePath + ".old");
            }

            var uc = new UpdateChecker(c.Manifest, c.ETag);
            var lw = new LoadingWindow(c.Name, c.Background, c.Icon);
            List<UpdateFile> files = lw.Run(uc);
            c.ETag = uc.ETag;

            if (files == null)
            {
                Log.Error("Failed to download manifest.");
                return;
            }

            if (files.Count == 0)
            {
                Log.Info("No files to download, launching.");
                c.SaveETag();
                StartManagedProcess(c.ExePath);
                return;
            }

            Log.Info("{0} files to download.", files.Count);

            var u = new Updater(files, c.UpdateDir);
            var uw = new UpdateWindow(c.Name, c.Background, c.Icon, c.News, c.ETag != null);
            if (!uw.Run(u))
                return;

            Log.Info("Download successful, launching...");
            c.SaveETag();
            StartManagedProcess(c.ExePath);
        }

        // this is our best guess
        public static bool IsLinux
        {
            get { return (int)Environment.OSVersion.Platform == 4; }
        }

        public static bool IsMacOSX
        {
            get { return (int)Environment.OSVersion.Platform == 6; }
        }

        public static void StartManagedProcess(string path)
        {
            Log.Info("Path to executable: {0}", path);

            if (path == null)
                return;

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;

            if (IsLinux || IsMacOSX)
            {
                Log.Warning("Non-Windows platform, running with mono in PATH.");
                // self-update works by luck this way
                psi.FileName = "mono";
                psi.Arguments = path;
            }
            else
            {
                psi.FileName = path;
            }

            Process.Start(psi);
        }
    }
}
