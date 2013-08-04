using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;

namespace WinMaitreDImporter
{
    public static class Logger
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Log(String msg)
        {
            if (ConfigData.Current.IsActivityLogging)
            {
                try
                {
                    String filepath = Path.Combine(ConfigData.Current.DebugActivityPath, "WinMaitreDImporterLog.txt");

                    using (StreamWriter writer = File.AppendText(filepath))
                    {
                        writer.Write("[" + DateTime.Now.ToLocalTime() + "] ");
                        writer.Write(msg);
                        writer.Write("\r\n");
                        writer.Close();
                    }
                }
                catch (Exception) { }
            }
        }
    }
}
