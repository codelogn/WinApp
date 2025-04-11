using System;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Forms
{
    public class TextToSpeechForm : Form
    {
        private TextBox textArea;
        private Button speakButton;
        private SpeechSynthesizer speechSynthesizer;

        public TextToSpeechForm()
        {
            // Initialize the form
            this.Text = "Text to Speech";
            this.Size = new System.Drawing.Size(800, 600);

            // Create a TextBox for text input
            textArea = new TextBox
            {
                Multiline = true,
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(760, 450),
                ScrollBars = ScrollBars.Vertical
            };

            // Create a Button to trigger text-to-speech
            speakButton = new Button
            {
                Text = "Speak",
                Location = new System.Drawing.Point(10, 470),
                Size = new System.Drawing.Size(100, 30)
            };
            speakButton.Click += SpeakButton_Click;

            // Initialize the SpeechSynthesizer
            speechSynthesizer = new SpeechSynthesizer();

            // Add controls to the form
            this.Controls.Add(textArea);
            this.Controls.Add(speakButton);
        }

        private void SpeakButton_Click(object sender, EventArgs e)
        {
            string text = textArea.Text;

            if (!string.IsNullOrWhiteSpace(text))
            {
                try
                {
                    // Cancel any ongoing speech
                    speechSynthesizer.SpeakAsyncCancelAll();

                    // Start speaking the text asynchronously
                    speechSynthesizer.SpeakAsync(text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to read the text. Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please enter some text to read.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Dispose of the SpeechSynthesizer when the form is closed
            speechSynthesizer.Dispose();
            base.OnFormClosing(e);
        }
    }
}