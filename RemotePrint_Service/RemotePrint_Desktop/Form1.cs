using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.IO;
using ClassLibrary;
using System.ServiceProcess;
using System.Threading;
using ServiceStack.Redis;
using System.Timers;



namespace RemotePrint_Desktop
{
    public partial class Form1 : Form
    {
        public bool fromTray = false;
        string appPath = "";
        public Form1()
        {
            InitializeComponent();

           
            //load printers
            PrintDocument prtdoc = new PrintDocument();
            string strDefaultPrinter = prtdoc.PrinterSettings.PrinterName;
            foreach (String strPrinter in PrinterSettings.InstalledPrinters)
            {
                comboBox1.Items.Add(strPrinter);
                if (strPrinter == strDefaultPrinter)
                {
                    comboBox1.SelectedIndex = comboBox1.Items.IndexOf(strPrinter);
                }
            }

            //load config file
            appPath = Path.GetDirectoryName(Application.CommonAppDataPath);
            IniFile file = new IniFile(appPath + "/app.ini");

            //server
            textBox2.Text = file.IniReadValue("Settings", "Server");
            //notification
            if (file.IniReadValue("Settings", "Notification") == "No") radioButton2.Checked = true;
            else
                radioButton1.Checked = true;

            string printer = file.IniReadValue("Settings", "Printer");
            if (comboBox1.Items.IndexOf(printer) > -1)
            {
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf(printer);
            }

            //key
            textBox1.Text = file.IniReadValue("Settings", "Key");
            textBox3.Text = file.IniReadValue("Settings", "Redis");
            if (textBox1.Text == "")
            {
                textBox1.Text = System.Guid.NewGuid().ToString();
                textBox2.Text = "https://test.splickit.com/app2/messagemanager/getnextmessagebymerchantid/";
                textBox3.Text = "pweb01.splickit.com";
                Save();
            }
            else
            {
                // textBox1.ReadOnly = true;
                button1.Enabled = false;
            }


        }

        private void Form1_Load(object sender, EventArgs e)
        {

            

        }
        private void Save()
        {
            //save ini
            appPath = Path.GetDirectoryName(Application.CommonAppDataPath);
            IniFile file = new IniFile(appPath + "/app.ini");
            file.IniWriteValue("Settings", "Key", textBox1.Text);
            file.IniWriteValue("Settings", "Server", textBox2.Text);
            file.IniWriteValue("Settings", "Redis", textBox3.Text);
            if (radioButton1.Checked) file.IniWriteValue("Settings", "Notification", "Yes");
            else
                file.IniWriteValue("Settings", "Notification", "No");
            file.IniWriteValue("Settings", "Printer", comboBox1.Text);
            if (!fromTray)
                RestartService("SplickItRemotePrint", 30000);
            //textBox1.ReadOnly = true;
            button1.Enabled = false;
        }
        private void button1_Click(object sender, EventArgs e)
        {
     
            Save();


            MessageBox.Show("Saved!");

        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;
            Save();
            string ret = Code.GetOrders("",appPath);
            button2.Enabled = true;
            Cursor.Current = Cursors.Default;
            MessageBox.Show(ret);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
            textBox3.Visible = true;
            label4.Visible = true;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
            textBox3.Visible = false;
            label4.Visible = false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }


        public static void RestartService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                try
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                }
                catch
                {
                    // ...
                }
                // count the rest of the timeout
                int millisec2 = Environment.TickCount;
                timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch
            {
                // ...
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }


        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!fromTray)
            {
               
                Application.Exit();
            }
        }


    }
}
