using System;
using System.Windows.Forms;

namespace MyNamespace
{
    public partial class MyForm : Form
    {
        private System.Windows.Forms.TextBox statusTextBox;

        public MyForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.statusTextBox = new System.Windows.Forms.TextBox();
            this.statusTextBox.Multiline = true;
            this.statusTextBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusTextBox.Height = 100;
            this.Controls.Add(this.statusTextBox);

            InitializeMenu();
        }

        private void InitializeMenu()
        {
            // Menu initialization code here
        }
    }
}