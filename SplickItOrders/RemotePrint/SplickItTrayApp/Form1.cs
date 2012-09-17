using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.IO;
using SplickItOrdersApp;
using System.Threading;
using System.Timers;
using System.Net;
using System.Xml;
using System.Xml.Serialization;

namespace SplickItOrdersApp
{
    public partial class Form1 : Form
    {
        CustomerConfig configData = new CustomerConfig();

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

            configData = Helper.LoadConfigData();

            //server
            txtServer.Text = configData.Server;
            if (comboBox1.Items.IndexOf(configData.Printer) > -1)
            {
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf(configData.Printer);
            }

            txtKey.Text = configData.Key;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtServer.Text) ||
                String.IsNullOrEmpty(txtKey.Text) ||
                String.IsNullOrEmpty(comboBox1.Text))
            {
                MessageBox.Show("Please provide valid configuration values.");
                return;
            }

            //save ini
            configData.Key = txtKey.Text;
            configData.Server = txtServer.Text.Replace("/", "").ToLower();
            configData.Printer = comboBox1.Text;
            Helper.SaveConfigData(configData);

            //txtKey.ReadOnly = true;
            button1.Enabled = false;
            MessageBox.Show("Saved!");
            DialogResult = DialogResult.OK;
        }


        private void txtKey_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        private void txtServer_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = true;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            
            Cursor.Current = Cursors.WaitCursor;
            //Save();
            order ord = new order();
            XmlSerializer serializer = new XmlSerializer(typeof(order));
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = true;
            XmlReader xmlReader = XmlReader.Create("order.xml", settings);
            xmlReader.Read();
            ord = (order)serializer.Deserialize(xmlReader);
            RawPrinterHelper.SendStringToPrinter(comboBox1.SelectedItem.ToString(), ord.order_details);
            Cursor.Current = Cursors.Default;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtServer.Text))
            {
                MessageBox.Show("Please type in a valid server name.");
                return;
            }

            String server = txtServer.Text.Replace("/", "").ToLower();
            if (server.StartsWith("http"))
            {
                MessageBox.Show("Error: please provide server name without the HTTP protocol.");
                return;
            }

            Helper.TestUri(configData, server, txtKey.Text);
        }

    }
}
