using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Configuration;

namespace WinMaitreDImporter
{
    public class ConfigData
    {
        public string Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
                string appPath = Path.GetDirectoryName(Application.CommonAppDataPath);
                IniFile file = new IniFile(appPath + "/WinMaitreDImporter.ini");
                file.IniWriteValue("Settings", "Key", key);
            }
        }

        public string Server
        {
            get
            {
                return server;
            }
            set
            {
                server = value;
                string appPath = Path.GetDirectoryName(Application.CommonAppDataPath);
                IniFile file = new IniFile(appPath + "/WinMaitreDImporter.ini");
                file.IniWriteValue("Settings", "Server", server);
            }
        }

        public Int32 Timeperiod_mSec
        {
            get { return timeperiod_mSec; }
        }

        public Int32 Timeout_mSec
        {
            get { return timeout_mSec; }
        }

        public string OrderPath
        {
            get { return orderPath; }
        }

        public bool DeleteAnswerfile
        {
            get { return deleteAnswerFile; }
        }

        public bool IsActivityLogging
        {
            get { return isActivityLogging; }
        }

        public string DebugActivityPath
        {
            get { return debugActivityPath; }
        }

        public static ConfigData Current
        {
            get
            {
                return configData;
            }
        }

        private string key;
        private string server;
        private Int32 timeperiod_mSec;
        private Int32 timeout_mSec;
        private string orderPath;
        private bool deleteAnswerFile;
        private bool isActivityLogging;
        private string debugActivityPath;

        private static readonly ConfigData configData = new ConfigData();

        private ConfigData()
        {
            //load config file
            string appPath = Path.GetDirectoryName(Application.CommonAppDataPath);

            IniFile file = new IniFile(appPath + "/WinMaitreDImporter.ini");
            key = file.IniReadValue("Settings", "Key");

            //server
            server = file.IniReadValue("Settings", "Server").ToLower();

            try
            {
                orderPath = ConfigurationManager.AppSettings.Get("OrderPath");
            }
            catch (Exception)
            {
                MessageBox.Show("Error reading OrderPath from the Config file.");
                Application.Exit();
            }
            if (String.IsNullOrEmpty(orderPath))
            {
                orderPath = @"C:\POSERA\MaitreD\DATA\Web";
            }

            int timeperiod_sec = 0;
            String strTimePeriod = String.Empty;
            try
            {
                strTimePeriod = ConfigurationManager.AppSettings.Get("Timeperiod_Sec");
            }
            catch (Exception) { };

            Boolean gotTimeperiod = Int32.TryParse(strTimePeriod, out timeperiod_sec);
            if (!gotTimeperiod || timeperiod_sec < 1)
            {
                timeperiod_mSec = 30000; // 30 seconds
                timeout_mSec = 10000; // 10 secs
            }
            else
            {
                timeperiod_mSec = timeperiod_sec * 1000;
                timeout_mSec = timeperiod_mSec / 2;
            }

            if (timeout_mSec > 10000)
            {
                timeout_mSec = 10000;
            }

            String deleteFile = null;
            try
            {
                deleteFile = ConfigurationManager.AppSettings.Get("DeleteAnswerFile").ToLower();
            }
            catch (Exception) {};

            if (String.IsNullOrEmpty(deleteFile))
            {
                deleteAnswerFile = true;
            }
            else
            {
                deleteAnswerFile = !(String.Compare(deleteFile, "false") == 0);
            }

            String activityLog = null;
            try
            {
                activityLog = ConfigurationManager.AppSettings.Get("DebugActivity").ToLower();
            }
            catch (Exception) {}
            if (String.IsNullOrEmpty(activityLog))
            {
                isActivityLogging = false;
            }
            else
            {
                isActivityLogging = (String.Compare(activityLog, "true") == 0);
            }

            debugActivityPath = null;
            try
            {
                debugActivityPath = ConfigurationManager.AppSettings.Get("DebugActivityPath");
            }
            catch (Exception) { }
            if (String.IsNullOrEmpty(debugActivityPath))
            {
                debugActivityPath = @"C:\POSERA\MaitreD\DATA\Web";
            }
        }

    }
}
