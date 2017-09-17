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
        string _saveEtag = null;

        public string ETag
        {
            get {
                if (_etag == null)
                {
                    if (!File.Exists("manifest.ver"))
                        return null;

                    _etag = File.ReadAllText("manifest.ver");
                    _saveEtag = _etag;
                }

                return _etag;
            }
            set {
                _saveEtag = value;
            }
        }

        public void ResetETag()
        {
            _etag = "";
            _saveEtag = "";
        }

        public void SaveETag()
        {
            if (ETag == _saveEtag)
            {
                Log.Info("ETag has not changed, skipping save.");
                return;
            }

            Log.Info("Saving ETag to manifest.ver");

            using (StreamWriter sw = File.CreateText("manifest.ver"))
            {
                sw.Write(_saveEtag);
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

        public string ExePath
        {
            get { return ConfigurationManager.AppSettings["ExePath"]; }
        }

        public string InstallDir
        {
            get { return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); }
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

