using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Web;
using System.Windows.Forms;
using System.Configuration;

namespace SplickItOrdersApp
{
    public class Helper
    {
        public static bool GetOrders(CustomerConfig configData, out bool hasOrder, out string retVal)
        {
            bool linkActive = false;
            retVal = "OK";
            hasOrder = false;
            string resp = String.Empty;

            resp = String.Empty;
            //EventLog.WriteEntry("SplickIt Remote Printing", "HTTP Request");
            // prepare the web page we will be asking for
            String requestUriStr = "https://" + configData.Server +
                "/app2/messagemanager/getnextmessagebymerchantid/" +
                configData.Key +
                "/winapp/";
            HttpWebRequest request = (HttpWebRequest)
                WebRequest.Create(requestUriStr);
            try
            {
                request.Timeout = configData.Timeout_mSec; // 10 seconds
                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Get the stream associated with the response.
                    Stream receiveStream = response.GetResponseStream();

                    // Pipes the stream to a higher level stream reader with the required encoding format. 
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    resp = HttpUtility.UrlDecode(readStream.ReadToEnd());
                    hasOrder = resp.Contains("<order_id>");
                    if (hasOrder)
                    {
                        order ord = new order();
                        // Read a purchase order.
                        XmlSerializer serializer = new XmlSerializer(typeof(order));

                        XmlReaderSettings settings = new XmlReaderSettings();
                        settings.ConformanceLevel = ConformanceLevel.Fragment;
                        settings.IgnoreWhitespace = true;
                        settings.IgnoreComments = true;

                        XmlReader xmlReader = XmlReader.Create(new StringReader(resp), settings);
                        xmlReader.Read();

                        ord = (order)serializer.Deserialize(xmlReader);
                        RawPrinterHelper.SendStringToPrinter(configData.Printer, ord.order_details);
                        retVal = ord.order_details;
                    }
                    else
                    {
                        retVal = "NO Order";
                    }
                    readStream.Close();
                    linkActive = true;
                }
                else
                {
                    //error
                    retVal = "Error:" + response.StatusDescription;
                    linkActive = false;
                }
                response.Close();
            }
            catch (System.Net.WebException ex)
            {
                retVal = "Error:" + ex.Message;
                linkActive = false;
            }

            return linkActive;
        }

        public static CustomerConfig LoadConfigData()
        {
            CustomerConfig configData = new CustomerConfig();
            //load config file
            string appPath = Path.GetDirectoryName(Application.CommonAppDataPath);

            IniFile file = new IniFile(appPath + "/app.ini");
            configData.Key = file.IniReadValue("Settings", "Key");

            //server
            configData.Server = file.IniReadValue("Settings", "Server").ToLower();

            //printer
            configData.Printer = file.IniReadValue("Settings", "Printer");

            Int32 timeperiod_sec = 0;
            String strTimePeriod = ConfigurationManager.AppSettings.Get("Timeperiod_Sec"); 
            Boolean gotTimeperiod = Int32.TryParse(strTimePeriod, out timeperiod_sec);
            if (!gotTimeperiod || timeperiod_sec < 1)
            {
                configData.Timeperiod_mSec = 30000; // 30 seconds
                configData.Timeout_mSec = 10000; // 10 secs
            }
            else
            {
                configData.Timeperiod_mSec = timeperiod_sec * 1000;
                configData.Timeout_mSec = configData.Timeperiod_mSec/2;
            }

            if (configData.Timeout_mSec > 10000)
            {
                configData.Timeout_mSec = 10000;
            }

            return configData;
        }

        public static void SaveConfigData(CustomerConfig configData)
        {
            //save ini
            string appPath = Path.GetDirectoryName(Application.CommonAppDataPath);
            IniFile file = new IniFile(appPath + "/app.ini");
            file.IniWriteValue("Settings", "Key", configData.Key);

            file.IniWriteValue("Settings", "Server", configData.Server);
            file.IniWriteValue("Settings", "Printer", configData.Printer);
        }

        public static void TestUri(CustomerConfig configData, String server, String key)
        {
            String requestUriStr = "https://" + server +
                "/app2/messagemanager/getnextmessagebymerchantid/" +
                key +
                "/winapp/";
            HttpWebRequest request = (HttpWebRequest)
                WebRequest.Create(requestUriStr);
            try
            {
                request.Timeout = configData.Timeout_mSec; // 10 seconds
                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    MessageBox.Show("Url Validated (200)");
                }
                else
                {
                    MessageBox.Show("Incorrect URL: " + response.StatusDescription);
                }
                response.Close();
            }
            catch (System.Net.WebException ex)
            {
                MessageBox.Show("Incorrect URL: " + ex.Message);
            }
        }
    }
}
