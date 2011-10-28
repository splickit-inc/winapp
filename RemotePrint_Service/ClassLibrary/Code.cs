using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Web;
namespace ClassLibrary
{
    public class Code
    {
        public static string lastID = "";
        public static string GetOrders(string xmlOrder, String appPath)
        {
            //EventLog.WriteEntry("SplickIt Remote Printing", xmlOrder);
            string ret = "OK";

            IniFile file = new IniFile(appPath + "/app.ini");
            //key
            string key = file.IniReadValue("Settings", "Key");

            //server
            string server = file.IniReadValue("Settings", "Server");
            //printer
            string printer = file.IniReadValue("Settings", "Printer");
            if (!server.EndsWith("/")) server += "/";

            string s = "";
            if (xmlOrder != "") s = xmlOrder;
            else
            {
                //EventLog.WriteEntry("SplickIt Remote Printing", "HTTP Request");
                // prepare the web page we will be asking for
                HttpWebRequest request = (HttpWebRequest)
                    WebRequest.Create(server + key + "/windowsservice/");

                // execute the request
                HttpWebResponse response = (HttpWebResponse)
                request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Get the stream associated with the response.
                    Stream receiveStream = response.GetResponseStream();

                    // Pipes the stream to a higher level stream reader with the required encoding format. 
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    s = readStream.ReadToEnd();
                    readStream.Close();

                }
                else
                {
                    //error
                    ret = "Error:" + response.StatusDescription;

                }

                response.Close();

            }
            s = HttpUtility.UrlDecode(s);
            if (s.Contains("<order>") && !s.Contains("<results>no open orders</results>"))
            {

                //EventLog.WriteEntry("SplickIt Remote Printing", "Printing");
                order ord = new order();
                // Read a purchase order.
                XmlSerializer serializer = new XmlSerializer(typeof(order));


                var settings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment, IgnoreWhitespace = true, IgnoreComments = true };


                var xmlReader = XmlReader.Create(new StringReader(s), settings);
                xmlReader.Read();

                ord = (order)serializer.Deserialize(xmlReader);
                RawPrinterHelper.SendStringToPrinter(printer, ord.order_details);
                lastID = ord.message_id;
                ret = ord.order_details;
                //EventLog.WriteEntry("SplickIt Remote Printing", "Printed:" + ord.order_details);
        

            }
            else
            {
                //error
                ret = s;
            }

            //EventLog.WriteEntry("SplickIt Remote Printing", ret);
        

            return ret;
        }
    }

}
