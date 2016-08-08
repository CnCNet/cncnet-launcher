using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace CnCNetLauncher
{
    public class UpdateCheckerEventArgs : EventArgs
    {
        public string status;

        public UpdateCheckerEventArgs(string status)
        {
            this.status = status;
        }
    }

    public class UpdateChecker
    {
        public delegate void StatusChangedEventHandler(object sender, UpdateCheckerEventArgs e);

        public event StatusChangedEventHandler StatusChanged;

        protected virtual void OnStatusChanged(UpdateCheckerEventArgs e)
        {
            if (StatusChanged != null)
                StatusChanged(this, e);

            Log.Info(e.status);
        }

        public UpdateChecker ()
        {
        }

        public List<UpdateFile> Run()
        {
            List<UpdateFile> reinstall = new List<UpdateFile>();

            OnStatusChanged(new UpdateCheckerEventArgs("Checking for updates..."));

            Log.Info("Downloading manifest from " + Configuration.UrlTo("manifest.txt"));

            try {
                WebRequest req = WebRequest.Create(Configuration.UrlTo("manifest.txt"));
                if (req == null)
                    throw new Exception("Invalid URL to manifest: " + Configuration.UrlTo("manifest.txt"));

                if (req.GetType() != typeof(HttpWebRequest))
                    throw new Exception("WebRequest has invalid type: " + req.GetType());

                req.Timeout = Configuration.FirstLaunch ? 60000 : 2000;
                if (Configuration.ETag != null)
                    req.Headers["If-None-Match"] = Configuration.ETag;

                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                using (MemoryStream ms = new MemoryStream())
                using (Stream responseStream = resp.GetResponseStream())
                {
                    Configuration.ETag = resp.Headers["ETag"];

                    Utils.CopyStream(responseStream, ms);
                    responseStream.Close();
                    byte[] data = ms.ToArray();

                    string manifest = System.Text.Encoding.UTF8.GetString(data);

                    OnStatusChanged(new UpdateCheckerEventArgs("Calculating update size..."));

                    foreach (string line in manifest.Split(new char[] { '\r', '\n' }))
                    {
                        var m = System.Text.RegularExpressions.Regex.Match(line, "^([A-Za-z0-9]{40})\\s+(\\d+)\\s+([\\-\\?]?)(.+)$");
                        if (!m.Success)
                            continue;

                        var sha1 = m.Groups[1].ToString();
                        var size = Int32.Parse(m.Groups[2].ToString());
                        var path = m.Groups[4].ToString();
                        var deleted = m.Groups[3].ToString() == "-";
                        var anyVersion = m.Groups[3].ToString() == "?";

                        var uf = new UpdateFile(sha1, size, path, deleted, anyVersion);
                        if (!uf.Validate())
                        {
                            Log.Info("File failed validation: " + uf.Path);
                            reinstall.Add(uf);
                        }
                    }
                }

                return reinstall;

            } catch (WebException e) {
                if (e.Status == WebExceptionStatus.ProtocolError) {
                    HttpStatusCode status = ((HttpWebResponse)e.Response).StatusCode;
                    if (status == HttpStatusCode.NotModified) {
                        OnStatusChanged(new UpdateCheckerEventArgs("Up-to-date."));
                        return reinstall;
                    }
                }

                Log.Warning("WebException: {0}", e.Message);

                OnStatusChanged(new UpdateCheckerEventArgs(e.Message));
                Thread.Sleep(3000);

                if (!Configuration.FirstLaunch)
                    return reinstall;

                throw e;
            }
        }
    }
}

