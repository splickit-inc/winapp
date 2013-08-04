using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.IO;
using System.Threading;
using System.Timers;
using System.Net;
using System.Xml;
using System.Xml.Serialization;

namespace WinMaitreDImporter
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();

            //server
            txtServer.Text = ConfigData.Current.Server;
            txtKey.Text = ConfigData.Current.Key;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtServer.Text) ||
                String.IsNullOrEmpty(txtKey.Text))
            {
                MessageBox.Show("Please provide valid configuration values.");
                return;
            }

            //save ini
            ConfigData.Current.Key = txtKey.Text;
            ConfigData.Current.Server = txtServer.Text.Replace("/", "").ToLower();

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

            TestHelper.TestUri(server, txtKey.Text);
        }

    }
}
