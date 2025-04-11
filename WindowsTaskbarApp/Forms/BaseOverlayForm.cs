using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Forms
{
    public class BaseOverlayForm : Form
    {
        private bool isDragging = false;
        private Point dragStartPoint;

        public BaseOverlayForm()
        {
            // Configure the form as an overlay
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.TopMost = true; // Ensure the form is always on top
            this.BackColor = Color.Black;
            this.Opacity = 0.85; // Semi-transparent background

            // Attach mouse events for dragging
            this.MouseDown += BaseOverlayForm_MouseDown;
            this.MouseMove += BaseOverlayForm_MouseMove;
            this.MouseUp += BaseOverlayForm_MouseUp;
        }

        private void BaseOverlayForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPoint = e.Location;
            }
        }

        private void BaseOverlayForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                this.Left += e.X - dragStartPoint.X;
                this.Top += e.Y - dragStartPoint.Y;
            }
        }

        private void BaseOverlayForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }
    }
}