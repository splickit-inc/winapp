using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Timers;

namespace RemotePrint_Desktop
{

    public partial class frmShowMessage : Form
    {
        //private System.Timers.Timer timer1 = null;
        int beepc = 0;
        string lastMessage = "";
        public void setText(string text)
        {
             lastMessage = text;
        
        }
    
        public frmShowMessage()
        {
            InitializeComponent();


          //  timer1 = new System.Timers.Timer(100);//ping after 3 seconds
        //    timer1.Elapsed += new ElapsedEventHandler(this.timer1_Tick);

        }


        private void textBox1_Click(object sender, EventArgs e)
        {
            this.TopMost = false;
            timer2.Enabled=false;
            this.Opacity = 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Top = 0;
            this.Left = Screen.PrimaryScreen.WorkingArea.Width - this.Width;
              textBox1.SelectionStart = 0;

        }

        private void timer1_Tick(object sender, EventArgs e)
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
                timer2.Enabled=false;

                beepc = 0;
            }
        }



        private void textBox1_MouseEnter(object sender, EventArgs e)
        {
            this.Opacity = 1;
            timer2.Enabled=false;

        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            if (lastMessage != "")
            {
                Show();
                textBox1.Text = lastMessage;
                lastMessage = "";
                beepc = 0;
                timer2.Enabled = true;
            }
        }

        private void frmShowMessage_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            textBox1_Click(this, null);
        }
    }
}
