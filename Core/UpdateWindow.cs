using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel;

namespace Updater.Core
{
    public class UpdateWindow : Form
    {
        Label _status;
        ProgressBar _progressBar;
        CheckBox _autoLaunch;
        Button _launchButton;

        public bool AutoLaunch
        {
            get
            {
                return _autoLaunch.Checked;
            }
            set
            {
                _autoLaunch.Checked = value;
            }
        }

        public UpdateWindow(string name, Image backgroundImage, Icon icon, Uri news, bool autoLaunch)
        {
            Text = "Updating " + name;
            Icon = icon;

            MaximizeBox = false;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            Padding = Padding.Empty;
            Margin = Padding.Empty;

            var outerPanel = new FlowLayoutPanel();
            var picture = new PictureBox();

            picture.Image = backgroundImage;
            picture.ClientSize = new Size(picture.Image.Width, picture.Image.Height);
            picture.Margin = Padding.Empty;
            picture.Margin = new Padding();

            outerPanel.Padding = Padding.Empty;
            outerPanel.Margin = Padding.Empty;
            outerPanel.FlowDirection = FlowDirection.TopDown;
            outerPanel.AutoSize = true;

            outerPanel.Controls.Add(picture);

            var controlPanel = new TableLayoutPanel();
            controlPanel.AutoSize = true;
            controlPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            controlPanel.Margin = new Padding(8);

            _status = new Label();
            _status.Text = "Initializing...";
            _status.AutoSize = true;
            _status.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            controlPanel.Controls.Add(_status);

            _progressBar = new ProgressBar();
            _progressBar.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            _progressBar.Value = 0;
            controlPanel.Controls.Add(_progressBar);

            _autoLaunch = new CheckBox();
            _autoLaunch.AutoSize = true;
            _autoLaunch.Text = "Launch " + name + " as soon as it's ready";
            _autoLaunch.Checked = autoLaunch;
            _autoLaunch.TextAlign = ContentAlignment.BottomLeft;
            controlPanel.Controls.Add(_autoLaunch);

            var buttonPanel = new FlowLayoutPanel();
            buttonPanel.AutoSize = true;
            buttonPanel.Margin = Padding.Empty;

            EventHandler newsButtonClick = (sender, e) => {
                System.Diagnostics.Process.Start(news.ToString());
            };

            var newsButton = new Button();
            newsButton.AutoSize = true;
            newsButton.Text = "Update news...";
            newsButton.Click += newsButtonClick;
            buttonPanel.Controls.Add(newsButton);

            _launchButton = new Button();
            _launchButton.AutoSize = true;
            _launchButton.Text = "Launch";
            _launchButton.Enabled = false;
            _launchButton.DialogResult = DialogResult.OK;
            buttonPanel.Controls.Add(_launchButton);

            var cancelButton = new Button();
            cancelButton.AutoSize = true;
            cancelButton.Text = "Cancel";
            cancelButton.DialogResult = DialogResult.Cancel;
            buttonPanel.Controls.Add(cancelButton);

            controlPanel.Controls.Add(buttonPanel);

            outerPanel.Controls.Add(controlPanel);
            Controls.Add(outerPanel);

            CenterToScreen();
        }

        public bool Run(Updater updater)
        {
            var worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += (object sender, ProgressChangedEventArgs e) => {
                _progressBar.Value = e.ProgressPercentage;
                _status.Text = e.UserState as string;
                this.Refresh();
            };

            updater.StatusChanged += (object sender, UpdaterEventArgs e) => {
                worker.ReportProgress(e.progress, e.status);
            };

            worker.DoWork += (object sender, DoWorkEventArgs e) => {
                updater.Run();
            };

            worker.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) => {
                this._launchButton.Enabled = true;

                if (_autoLaunch.Checked) {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };

            worker.RunWorkerAsync();

            return ShowDialog() == DialogResult.OK;
        }
    }
}

