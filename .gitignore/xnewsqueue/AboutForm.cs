namespace xnewsqueue
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;

    public class AboutForm : Form
    {
        private Container components = null;
        private Label label1;
        private LinkLabel Link_Email;

        public AboutForm()
        {
            this.InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.label1 = new Label();
            this.Link_Email = new LinkLabel();
            base.SuspendLayout();
            this.label1.Font = new Font("Microsoft Sans Serif", 15.75f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.label1.Location = new Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new Size(160, 0x18);
            this.label1.TabIndex = 0;
            this.label1.Text = "Coded by Azaril";
            this.Link_Email.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.Link_Email.Location = new Point(8, 40);
            this.Link_Email.Name = "Link_Email";
            this.Link_Email.Size = new Size(0xa8, 0x18);
            this.Link_Email.TabIndex = 1;
            this.Link_Email.TabStop = true;
            this.Link_Email.Text = "william@archbell.com";
            this.Link_Email.TextAlign = ContentAlignment.MiddleCenter;
            this.Link_Email.LinkClicked += new LinkLabelLinkClickedEventHandler(this.Link_Email_LinkClicked);
            this.AutoScaleBaseSize = new Size(5, 13);
            base.ClientSize = new Size(0xb2, 0x4f);
            base.Controls.Add(this.Link_Email);
            base.Controls.Add(this.label1);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "AboutForm";
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = "About";
            base.ResumeLayout(false);
        }

        private void Link_Email_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("mailto:william@archbell.com");
        }
    }
}

