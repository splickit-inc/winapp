using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Timers;
using System.Diagnostics;

namespace SplickItOrdersApp
{
    public partial class frmShowMessage : Form
    {
        static Object lastMessageSyncObject = new object();

        int beepc = 0;
        string lastMessage = "";

        public void setText(string text)
        {
            try
            {
                lock(lastMessageSyncObject)
                {
                    lastMessage = text.Replace("\n", "\r\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("SplickIt Remote Printing", "Message Error:" + ex.Message);
            }
        }

        public frmShowMessage()
        {
            InitializeComponent();
        }


        private void txtKey_Click(object sender, EventArgs e)
        {
            this.TopMost = false;
            timer2.Enabled = false;
            this.Opacity = 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                this.Top = 0;
                this.Left = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
                txtKey.SelectionStart = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("SplickIt Remote Printing", "Message Error:" + ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (beepc == 0)
                {
                    this.TopMost = true;
                }
                if (beepc == 0 || beepc == 3 || beepc == 6)
                    Console.Beep(1000, 200);
                beepc++;
                this.Opacity = this.Opacity + 0.02;
                if (this.Opacity > 0.8)
                {
                    this.Opacity = 0.8;
                    timer2.Enabled = false;

                    beepc = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("SplickIt Remote Printing", "Message Error:" + ex.Message);
            }
        }

        private void txtKey_MouseEnter(object sender, EventArgs e)
        {
            this.Opacity = 1;
            timer2.Enabled = false;
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            try
            {
                lock (lastMessageSyncObject)
                {
                    if (String.IsNullOrEmpty(lastMessage))
                        return;
                    txtKey.Text = lastMessage;
                    lastMessage = "";
                }
                Show();
                beepc = 0;
                timer2.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("SplickIt Remote Printing", "Message Error:" + ex.Message);

            }
        }

        private void frmShowMessage_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                txtKey_Click(this, null);
            }
        }
    }
}
