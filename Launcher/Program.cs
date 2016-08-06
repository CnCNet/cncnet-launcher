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
            Application.EnableVisualStyles();

            if (File.Exists(Application.ExecutablePath + ".old"))
            {
                File.Delete(Application.ExecutablePath + ".old");
            }

            var lw = new LoadingWindow();
            List<UpdateFile> files = lw.Run();

            if (files.Count == 0)
            {
                Configuration.SaveETag();
                Configuration.StartManagedProcess(Configuration.ExePath);
                return;
            }

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

            Configuration.SaveETag();
            Configuration.StartManagedProcess(Configuration.ExePath);
        }
    }
}
