using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;

namespace CnCNetLauncher
{
    public class LoadingWindow : Form
    {
        protected Font _font;
        protected SolidBrush _white;
        protected SolidBrush _black;
        protected StringFormat _format;
        protected String _status;

        public LoadingWindow()
        {
            Text = Configuration.Name;
            BackgroundImage = Configuration.Background;
            Size = new Size(BackgroundImage.Width, BackgroundImage.Height);
            Icon = Configuration.Icon;
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

        public List<UpdateFile> Run()
        {
            Exception ex = null;
            List<UpdateFile> files = null;

            var worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += (object sender, ProgressChangedEventArgs e) => {
                _status = e.UserState as string;
                this.Invalidate();
            };

            var uc = new UpdateChecker();
            uc.StatusChanged += (object sender, UpdateCheckerEventArgs e) => {
                worker.ReportProgress(0, e.status);
            };

            worker.DoWork += (object sender, DoWorkEventArgs e) => {
                files = uc.Run();
            };

            worker.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) => {
                this.Close();
                ex = e.Error;
            };

            worker.RunWorkerAsync();
            ShowDialog();

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

