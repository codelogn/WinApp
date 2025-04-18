using System;
using System.Windows.Forms;
using WindowsTaskbarApp.Jobs;

namespace WindowsTaskbarApp.Forms
{
    public partial class BackgroundJobsForm : Form
    {
        private readonly BackgroundJobs backgroundJobs;

        public BackgroundJobsForm(BackgroundJobs backgroundJobs)
        {
            InitializeComponent();
            this.backgroundJobs = backgroundJobs;
        }

        private void InitializeComponent()
        {
            this.Text = "Background Jobs";
            this.Size = new System.Drawing.Size(300, 150);

            var startButton = new Button
            {
                Text = "Start Job",
                Location = new System.Drawing.Point(50, 50),
                Size = new System.Drawing.Size(80, 30)
            };
            startButton.Click += StartButton_Click;

            var stopButton = new Button
            {
                Text = "Stop Job",
                Location = new System.Drawing.Point(150, 50),
                Size = new System.Drawing.Size(80, 30)
            };
            stopButton.Click += StopButton_Click;

            this.Controls.Add(startButton);
            this.Controls.Add(stopButton);
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            backgroundJobs.Start();
            MessageBox.Show("Background jobs started.", "Info");
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            backgroundJobs.Stop();
            MessageBox.Show("Background jobs stopped.", "Info");
        }
    }
}