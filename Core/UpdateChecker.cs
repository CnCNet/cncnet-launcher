﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace Updater.Core
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
        protected Uri manifest;
        public string ETag { get; protected set; }

        public delegate void StatusChangedEventHandler(object sender, UpdateCheckerEventArgs e);

        public event StatusChangedEventHandler StatusChanged;

        protected virtual void OnStatusChanged(UpdateCheckerEventArgs e)
        {
            StatusChanged?.Invoke(this, e);

            Log.Info(e.status);
        }

        public UpdateChecker(Uri manifest, string ETag)
        {
            this.manifest = manifest;
            this.ETag = ETag;
        }

        public List<UpdateFile> Run()
        {
            List<UpdateFile> reinstall = new List<UpdateFile>();
            string baseURL = new Regex("[^/]+$").Replace(manifest.ToString(), "");

            OnStatusChanged(new UpdateCheckerEventArgs("Checking for updates..."));

            Log.Info("Downloading manifest from " + manifest);

            try {
                WebRequest req = WebRequest.Create(manifest);
                if (req == null)
                    throw new Exception("Invalid URL to manifest: " + manifest);

                if (req.GetType() != typeof(HttpWebRequest))
                    throw new Exception("WebRequest has invalid type: " + req.GetType());

                req.Timeout = ETag == null ? 30000 : 5000;
                if (ETag != null)
                    req.Headers["If-None-Match"] = ETag;

                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                using (MemoryStream ms = new MemoryStream())
                using (Stream responseStream = resp.GetResponseStream())
                {
                    ETag = resp.Headers["ETag"];

                    responseStream.CopyTo(ms);
                    responseStream.Close();
                    byte[] data = ms.ToArray();

                    string manifestData = System.Text.Encoding.UTF8.GetString(data);

                    OnStatusChanged(new UpdateCheckerEventArgs("Calculating update size..."));

                    foreach (string line in manifestData.Split(new char[] { '\r', '\n' }))
                    {
                        var m = System.Text.RegularExpressions.Regex.Match(line, "^([A-Za-z0-9]{40})\\s+(\\d+)\\s+([\\-\\?]?)(.+)$");
                        if (!m.Success)
                            continue;

                        var sha1 = m.Groups[1].ToString();
                        var size = Int32.Parse(m.Groups[2].ToString());
                        var path = m.Groups[4].ToString();
                        var deleted = m.Groups[3].ToString() == "-";
                        var anyVersion = m.Groups[3].ToString() == "?";

                        var uf = new UpdateFile(sha1, size, path, deleted, anyVersion, new Uri(baseURL + sha1 + ".gz"));
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

                // allow launching if update check fails if we have an ETag
                if (ETag != null)
                    return reinstall;

                throw;
            }
        }
    }
}

