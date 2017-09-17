using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace Updater.Core
{
    public class LoadingWindow : Form
    {
        protected Font _font;
        protected SolidBrush _white;
        protected SolidBrush _black;
        protected StringFormat _format;
        protected String _status;

        public LoadingWindow(string name, Image backgroundImage, Icon icon)
        {
            Text = name;
            BackgroundImage = backgroundImage;
            if (BackgroundImage != null)
            {
                Size = new Size(BackgroundImage.Width, BackgroundImage.Height);
            }
            else
            {
                Size = new Size(500, 100);
            }
            Icon = icon;
            FormBorderStyle = FormBorderStyle.None;

            MinimizeBox = false;
            TopMost = true;
            DoubleBuffered = true;
            _font = new Font("SansSerif", 8);
            _white = new SolidBrush(Color.White);
            _black = new SolidBrush(Color.Black);
            _format = new StringFormat();
            _format.Alignment = StringAlignment.Far;
            _format.LineAlignment = StringAlignment.Far;
            CenterToScreen();
        }

        public List<UpdateFile> Run(UpdateChecker uc)
        {
            Exception ex = null;
            List<UpdateFile> files = null;

            var worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += (object sender, ProgressChangedEventArgs e) => {
                _status = e.UserState as string;
                this.Invalidate();
            };

            UpdateChecker.StatusChangedEventHandler statusChanged = (object sender, UpdateCheckerEventArgs e) => {
                worker.ReportProgress(0, e.status);
            };

            uc.StatusChanged += statusChanged;

            worker.DoWork += (object sender, DoWorkEventArgs e) => {
                files = uc.Run();
            };

            worker.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) => {
                this.Close();
                ex = e.Error;
            };

            worker.RunWorkerAsync();
            ShowDialog();

            uc.StatusChanged -= statusChanged;

            if (ex != null)
                throw ex;

            return files;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawString(_status, _font, _black, new RectangleF(-3, -3, Width, Height), _format);
            e.Graphics.DrawString(_status, _font, _white, new RectangleF(-4, -4, Width, Height), _format);
        }
    }
}

