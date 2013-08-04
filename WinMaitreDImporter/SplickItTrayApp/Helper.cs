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
using System.Text.RegularExpressions;

namespace WinMaitreDImporter
{
    public class Helper
    {
        public static bool GetOrders(out bool hasOrder)
        {
            bool linkActive = true;
            ExternalOrder ord = null;
            hasOrder = false;
            try
            {
                ord = GetServiceOrder();
                //ord = GenerateTestOrder();
                hasOrder = ord != null;
            }
            catch (Exception)
            {
                linkActive = false;
                Logger.Log("Link Inactive.");
            }

            if (hasOrder)
            {
                String xmlFilename = String.Empty;
                if (!String.IsNullOrEmpty(ord.Ref))
                {
                    xmlFilename = ord.Ref.Replace(" ", "") + ".XML";
                }
                else
                {
                    xmlFilename = "ORDER" + ord.Invoice + ".XML";
                }

                if (!String.IsNullOrEmpty(xmlFilename))
                {
                    try
                    {
                        String filepath = Path.Combine(ConfigData.Current.OrderPath, xmlFilename);
                        XmlSerializer serializer = new XmlSerializer(typeof(ExternalOrder));
                        using (Stream writer = new FileStream(filepath, FileMode.Create))
                        {
                            serializer.Serialize(writer, ord);
                            writer.Close();
                            Logger.Log("Order saved on Disk: " + xmlFilename);
                        }
                    }
                    catch (Exception e)
                    {
                        if (!String.IsNullOrEmpty(e.Message))
                        {
                            Logger.Log("Failed to save order file: " + e.Message);
                        }
                    }
                }
            }
            return linkActive;
        }

        private static ExternalOrder GetServiceOrder()
        {
            string resp = String.Empty;
            ExternalOrder ord = null;

            String requestUriStr = UriHelper.GetServiceUri(ConfigData.Current.Server, ConfigData.Current.Key);
            Logger.Log("Checking for order: " + requestUriStr);
            HttpWebRequest request = (HttpWebRequest)
                WebRequest.Create(requestUriStr);
            
            request.Timeout = ConfigData.Current.Timeout_mSec; // 10 seconds
            // execute the request
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // Get the stream associated with the response.
                using (Stream receiveStream = response.GetResponseStream())
                {
                    // Pipes the stream to a higher level stream reader with the required encoding format. 
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        resp = HttpUtility.UrlDecode(readStream.ReadToEnd());
                        if (resp.Contains("<ExternalOrder"))
                        {
                            // Read a purchase order.
                            XmlSerializer serializer = new XmlSerializer(typeof(ExternalOrder));

                            XmlReaderSettings settings = new XmlReaderSettings();
                            settings.ConformanceLevel = ConformanceLevel.Fragment;
                            settings.IgnoreWhitespace = true;
                            settings.IgnoreComments = true;

                            XmlReader xmlReader = XmlReader.Create(new StringReader(resp), settings);
                            xmlReader.Read();

                            ord = (ExternalOrder)serializer.Deserialize(xmlReader);
                            Logger.Log("External order received, invoice #: " + ord.Invoice);
                        }
                        else if (String.IsNullOrEmpty(resp))
                        {
                            Logger.Log("No order message found.");
                        }
                        else
                        {
                            Logger.Log("Invalid order message received.");
                        }
                        readStream.Close();
                    }
                    receiveStream.Close();
                }
            }
            else
            {
                Logger.Log("Invalid Http Response " + response.StatusCode.ToString() + " from " + requestUriStr);
                throw new Exception("Invalid Http Response.");
            }
            return ord;
        }

        public static void PostAnswer(string xmlFilePath)
        {
            String xmlResp = String.Empty;
            using (StreamReader reader = new StreamReader(xmlFilePath))
            {
                xmlResp = reader.ReadToEnd();
                reader.Close();
            }

            if (!String.IsNullOrEmpty(xmlResp))
            {
                String postReqUriStr = UriHelper.PostServiceUri(ConfigData.Current.Server, ConfigData.Current.Key);
                HttpWebRequest request = (HttpWebRequest)
                    WebRequest.Create(postReqUriStr);

                request.Timeout = ConfigData.Current.Timeout_mSec; // 10 seconds
                request.Method = "POST";
                request.ContentType = "application/xml;charset=utf-8";

                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                    try
                    {
                        writer.Write(xmlResp);
                        writer.Close();
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            Logger.Log("Answer file transmitted: " + postReqUriStr);
                            if (ConfigData.Current.DeleteAnswerfile)
                            {
                                File.Delete(xmlFilePath);
                                Logger.Log("Answer file deleted: " + xmlFilePath);
                            }
                        }
                        else
                        {
                            Logger.Log("Unable to transmit answer file: " + xmlFilePath +
                                " to the server URL: " + postReqUriStr);
                        }
                        response.Close();
                    }
                    catch (Exception ex)
                    {
                        if (!String.IsNullOrEmpty(ex.Message))
                        {
                            Logger.Log("Error transmitting answer file: " + ex.Message);
                        }
                    }
                }
            }
            else
            {
                Logger.Log("Empty answer file: " + xmlFilePath);
            }
        }
    }
}
