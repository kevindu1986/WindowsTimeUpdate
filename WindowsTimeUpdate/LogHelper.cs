using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WindowsTimeUpdate
{
    public static class LogHelper
    {
        public static void WriteLog(string message)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Logs.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + " ---- " + message);
                sw.Flush();
                sw.Close();
            }
            catch (Exception logex)
            {

            }
        }

        public static void WriteLog(Exception ex)
        {
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\Logs.txt", true);

                string innerMessage = "No inner message";
                if (ex.InnerException != null)
                {
                    innerMessage = ex.InnerException.Message;
                }

                sw.WriteLine(DateTime.Now.ToString() + " ---- " + ex.Message + " \n Inner Message: " + innerMessage);
                sw.Flush();
                sw.Close();
            }
            catch (Exception logex)
            {
                WriteLog("Error in WriteLog: '" + logex.Message + "'");
            }
        }
    }
}
