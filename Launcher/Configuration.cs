using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Configuration;

namespace CnCNetLauncher
{
    public class Configuration
    {
        public static bool FirstLaunch
        {
            get { return ETag == null; }
        }

        static string _etag = null;
        static string _saveEtag = null;

        public static string ETag
        {
            get {
                if (_etag == null)
                {
                    if (!File.Exists(FilePath("manifest.ver")))
                        return null;

                    _etag = File.ReadAllText(FilePath("manifest.ver"));
                    _saveEtag = _etag;
                }

                return _etag;
            }
            set {
                _saveEtag = value;
            }
        }

        public static void ResetETag()
        {
            _etag = "";
            _saveEtag = "";
        }

        public static void SaveETag()
        {
            if (ETag == _saveEtag)
            {
                Log.Info("ETag has not changed, skipping save.");
                return;
            }

            Log.Info("Saving ETag to manifest.ver");

            using (StreamWriter sw = File.CreateText(FilePath("manifest.ver")))
            {
                sw.Write(_saveEtag);
            }
        }

        public static string Name
        {
            get { return ConfigurationManager.AppSettings["Name"]; }
        }

        public static string BaseURL
        {
            get { return ConfigurationManager.AppSettings["BaseURL"]; }
        }

        public static string ExePath
        {
            get { return ConfigurationManager.AppSettings["ExePath"]; }
        }

        public static string InstallPath
        {
            get { return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); }
        }

        public static Image Background
        {
            get { return Image.FromStream(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("CnCNetLauncher.Resources.header.png"), false, true); }
        }

        public static Icon Icon
        {
            get { return new Icon(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("CnCNetLauncher.Resources.icon.ico")); }
        }

        public static string NewsURL
        {
            get { return ConfigurationManager.AppSettings["NewsURL"]; }
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

        public static string UrlTo(string file)
        {
            return BaseURL + "/" + file;
        }

        public static string FilePath(string path)
        {
            return InstallPath + Path.DirectorySeparatorChar + String.Join(Char.ToString(Path.DirectorySeparatorChar), path.Split(new char[]{'\\', '/'}));
        }

        public static void StartManagedProcess(string path)
        {
            var fullPath = FilePath(path);
            if (!File.Exists(fullPath))
                path = fullPath;

            Log.Info("Path to executable: {0}", path);

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

