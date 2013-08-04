using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Forms;
using System.Net;

namespace WinMaitreDImporter
{
    public static class TestHelper
    {
        public static void TestUri(String server, String key)
        {
            String requestUriStr = UriHelper.GetServiceUri(server, key);
            HttpWebRequest request = (HttpWebRequest)
                WebRequest.Create(requestUriStr);
            try
            {
                request.Timeout = ConfigData.Current.Timeout_mSec; // 10 seconds
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

        public static ExternalOrder GenerateTestOrder()
        {
            Random r = new Random();
            Boolean isOrder = r.Next(0, 10) < 3;
            if (!isOrder)
            {
                return null;
            }
            ExternalOrder ord = new ExternalOrder();
            XmlSerializer serializer = new XmlSerializer(typeof(ExternalOrder));
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = true;
            try
            {
                XmlReader xmlReader = XmlReader.Create("order.xml", settings);
                xmlReader.Read();
                ord = (ExternalOrder)serializer.Deserialize(xmlReader);
                xmlReader.Close();
            }
            catch (Exception) { }
            int num = r.Next(1, 1000);
            ord.Ref = "ORDER " + num.ToString("D3");
            return ord;
        }
    }
}
