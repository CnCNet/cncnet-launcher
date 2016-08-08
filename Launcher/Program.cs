using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace CnCNetLauncher 
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg == "-log")
                    Log.Open();
                else if (arg == "-force")
                    Configuration.ResetETag();
            }

            Log.Info("START " + DateTime.Now);

            try
            {
                Run();
            }
            catch (Exception e)
            {
                Log.Error("{0}: {1}", e.GetType(), e.Message);
                throw e;
            }

            Log.Info("END {0}", DateTime.Now);
            Log.Close();
        }

        public static void Run()
        {
            Application.EnableVisualStyles();

            Log.Info("Saved ETag: {0}", Configuration.ETag);

            if (File.Exists(Application.ExecutablePath + ".old"))
            {
                Log.Info("Cleanup: Removing old {0}.old", Application.ExecutablePath);
                File.Delete(Application.ExecutablePath + ".old");
            }

            var lw = new LoadingWindow();
            List<UpdateFile> files = lw.Run();

            if (files == null)
            {
                Log.Error("Failed to download manifest.");
                return;
            }

            if (files.Count == 0)
            {
                Log.Info("No files to download, launching.");
                Configuration.SaveETag();
                Configuration.StartManagedProcess(Configuration.ExePath);
                return;
            }

            Log.Info("{0} files to download.", files.Count);

            // self-update
            foreach (UpdateFile uf in files)
            {
                if (Configuration.FilePath(uf.Path) == Application.ExecutablePath)
                {
                    var selfUpdateList = new List<UpdateFile>();
                    selfUpdateList.Add(uf);

                    var selfUw = new UpdateWindow();
                    selfUw.AutoLaunch = true;
                    if (!selfUw.Run(selfUpdateList))
                        return;

                    Configuration.StartManagedProcess(Application.ExecutablePath);
                    return;
                }
            }

            var uw = new UpdateWindow();
            if (!uw.Run(files))
                return;

            Log.Info("Download successful, launching...");
            Configuration.SaveETag();
            Configuration.StartManagedProcess(Configuration.ExePath);
        }
    }
}
