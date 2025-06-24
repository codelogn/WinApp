using System;
using System.Windows.Forms;
using System.Media;

namespace WindowsTaskbarApp.Tools.AudioPlayer
{
    public class AudioPlayerForm : Form
    {
        private Button browseButton;
        private Button playButton;
        private TextBox filePathTextBox;
        private OpenFileDialog openFileDialog;
        private string selectedFilePath;
        private SoundPlayer soundPlayer;

        public AudioPlayerForm()
        {
            this.Text = "Audio Player";
            this.Size = new System.Drawing.Size(400, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            filePathTextBox = new TextBox
            {
                Dock = DockStyle.Top,
                ReadOnly = true,
                PlaceholderText = "Select an audio file (.wav)"
            };

            browseButton = new Button
            {
                Text = "Browse",
                Dock = DockStyle.Top
            };
            browseButton.Click += BrowseButton_Click;

            playButton = new Button
            {
                Text = "Play",
                Dock = DockStyle.Top,
                Enabled = false
            };
            playButton.Click += PlayButton_Click;

            openFileDialog = new OpenFileDialog
            {
                Filter = "WAV files (*.wav)|*.wav|All files (*.*)|*.*",
                Title = "Select an audio file"
            };

            this.Controls.Add(playButton);
            this.Controls.Add(browseButton);
            this.Controls.Add(filePathTextBox);
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFilePath = openFileDialog.FileName;
                filePathTextBox.Text = selectedFilePath;
                playButton.Enabled = true;
            }
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            try
            {
                soundPlayer = new SoundPlayer(selectedFilePath);
                soundPlayer.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to play audio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
