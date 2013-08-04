using System;
using System.Collections.Generic;
using System.Text;

namespace WinMaitreDImporter
{
    public static class UriHelper
    {
        public static String GetServiceUri(String server, string key)
        {
            return "https://" + server +
                "/app2/messagemanager/getnextmessagebymerchantid/" +
                key +
                "/windowsservice/";
        }

        public static String PostServiceUri(String server, string key)
        {
            return "https://" + server +
                "/app2/messagemanager/" +
                key +
                "/windowsservice/callback";
        }
    }
}
