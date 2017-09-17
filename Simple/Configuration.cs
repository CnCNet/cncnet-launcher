using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Configuration;
using Updater.Core;

namespace Updater.Simple
{
    public class Configuration : IConfiguration
    {
        string _etag = null;

        public string ETag
        {
            get {
                if (_etag == null)
                {
                    if (!File.Exists("manifest.ver"))
                        return null;

                    _etag = File.ReadAllText("manifest.ver");
                }

                return _etag;
            }
            set {
                if (_etag != value)
                {
                    _etag = value;

                    Log.Info("Saving ETag to manifest.ver");

                    using (StreamWriter sw = File.CreateText("manifest.ver"))
                    {
                        sw.Write(_etag);
                    }
                }
            }
        }

        public string Name
        {
            get { return ConfigurationManager.AppSettings["Name"] ?? "Unnamed"; }
        }

        public Uri Manifest
        {
            get
            {
                var manifestUrl = ConfigurationManager.AppSettings["ManifestURL"];
                if (manifestUrl == null)
                    throw new InvalidDataException("ManifestURL must be set in app.config");

                return new Uri(manifestUrl);
            }
        }

        public string Executable
        {
            get { return ConfigurationManager.AppSettings["Executable"]; }
        }

        public string InstallDir
        {
            get { return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); }
        }

        public string UpdateDir
        {
            get { return "patch"; }
        }

        public Image Background
        {
            get
            {
                var backgroundPath = ConfigurationManager.AppSettings["Background"];
                return backgroundPath == null ? null : Image.FromFile(backgroundPath);
            }
        }

        public Icon Icon
        {
            get
            {
                var iconPath = ConfigurationManager.AppSettings["Icon"];
                return iconPath == null ? null : Icon.ExtractAssociatedIcon(iconPath);
            }
        }

        public Uri News
        {
            get
            {
                var newsUrl = ConfigurationManager.AppSettings["NewsURL"];
                return newsUrl == null ? null : new Uri(newsUrl);
            }
        }
    }
}

