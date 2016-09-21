using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WindowsTimeUpdate
{
    partial class DateTimeService : ServiceBase
    {
        private Timer timer;
        private bool syncSuccessfully;
        private SystemTimeHelper systemTimeHelper;
        private int loopTimes;
        private string serverName;

        public DateTimeService()
        {
            InitializeComponent();
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool SetLocalTime(ref SystemTime sysTime);

        protected override void OnStart(string[] args)
        {
            //System.Diagnostics.Debugger.Launch();
            LogHelper.WriteLog("***********************************************************************");
            LogHelper.WriteLog("*                      == WINDOWS TIME UPDATE ==                      *");
            LogHelper.WriteLog("*                    Version: 1.0 - Author: Hao Du                    *");
            LogHelper.WriteLog("*                    Date   : 21 Sep 2016                             *");
            LogHelper.WriteLog("***********************************************************************");

            syncSuccessfully = false;
            systemTimeHelper = new SystemTimeHelper(2016, 9, 17, 00, 00, 00);
            loopTimes = 0;
            serverName = "time.windows.com";

            LogHelper.WriteLog("Opening registry 'Computer\\HKEY_LOCAL_MACHINE\\SOFTWARE\\WindowsTime'...");
            RegistryKey windowsTimeKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WindowsTime", true);
            if (windowsTimeKey == null)
            {
                LogHelper.WriteLog("Creating new registry values...");
                windowsTimeKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\WindowsTime", RegistryKeyPermissionCheck.ReadWriteSubTree);
                windowsTimeKey.SetValue("Year", systemTimeHelper.Year, RegistryValueKind.String);
                windowsTimeKey.SetValue("Month", systemTimeHelper.Month, RegistryValueKind.String);
                windowsTimeKey.SetValue("Day", systemTimeHelper.Day, RegistryValueKind.String);
                windowsTimeKey.SetValue("Hour", systemTimeHelper.Hour, RegistryValueKind.String);
                windowsTimeKey.SetValue("Minute", systemTimeHelper.Minute, RegistryValueKind.String);
                windowsTimeKey.SetValue("Second", systemTimeHelper.Second, RegistryValueKind.String);
                windowsTimeKey.SetValue("TimeInterval", "100000", RegistryValueKind.String); //100 secs
                windowsTimeKey.SetValue("Server", serverName, RegistryValueKind.String);
                LogHelper.WriteLog("Creating new registry values... Done!");
            }
            else
            {
                LogHelper.WriteLog("Getting registry values...");
                systemTimeHelper.SetValues(windowsTimeKey.GetValue("Year").ToString(),
                                    windowsTimeKey.GetValue("Month").ToString(),
                                    windowsTimeKey.GetValue("Day").ToString(),
                                    windowsTimeKey.GetValue("Hour").ToString(),
                                    windowsTimeKey.GetValue("Minute").ToString(),
                                    windowsTimeKey.GetValue("Second").ToString());
                LogHelper.WriteLog("Getting registry values... Done");
            }

            LogHelper.WriteLog("Datetime in registry is " + systemTimeHelper.Year
                                                        + "-" + systemTimeHelper.Month
                                                        + "-" + systemTimeHelper.Day
                                                        + " " + systemTimeHelper.Hour
                                                        + ":" + systemTimeHelper.Minute
                                                        + ":" + systemTimeHelper.Day);

            serverName = windowsTimeKey.GetValue("Server").ToString();
            LogHelper.WriteLog("Server Name is " + serverName);

            LogHelper.WriteLog("Creating Ticker...");
            object timeIntervalValue = windowsTimeKey.GetValue("TimeInterval");
            int timeInterval = 300000;
            if (timeIntervalValue != null)
            {
                timeInterval = int.Parse(timeIntervalValue.ToString());
            }
            windowsTimeKey.Close();

            timer = new Timer();
            timer.Interval = timeInterval;
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
            LogHelper.WriteLog("Creating Ticker... Done");
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!syncSuccessfully)
            {
                try
                {
                    timer.Enabled = false;

                    loopTimes++;
                    LogHelper.WriteLog("Loop " + loopTimes.ToString());

                    LogHelper.WriteLog("Apptemping to update with Registry Value...");

                    SystemTime updatedTime = systemTimeHelper.GetSystemTime();
                    SetLocalTime(ref updatedTime);

                    LogHelper.WriteLog("Apptemping to update with Internet Value...");

                    updatedTime = systemTimeHelper.GetInternetTime(serverName);
                    SetLocalTime(ref updatedTime);

                    DateTime latestUpdateTime = DateTime.Now;
                    systemTimeHelper.SetValues(latestUpdateTime.Year.ToString(), latestUpdateTime.Month.ToString(), latestUpdateTime.Day.ToString()
                                            , latestUpdateTime.Hour.ToString(), latestUpdateTime.Minute.ToString(), latestUpdateTime.Second.ToString());

                    RegistryKey windowsTimeKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WindowsTime", true);
                    windowsTimeKey.SetValue("Year", systemTimeHelper.Year, RegistryValueKind.String);
                    windowsTimeKey.SetValue("Month", systemTimeHelper.Month, RegistryValueKind.String);
                    windowsTimeKey.SetValue("Day", systemTimeHelper.Day, RegistryValueKind.String);
                    windowsTimeKey.SetValue("Hour", systemTimeHelper.Hour, RegistryValueKind.String);
                    windowsTimeKey.SetValue("Minute", systemTimeHelper.Minute, RegistryValueKind.String);
                    windowsTimeKey.SetValue("Second", systemTimeHelper.Second, RegistryValueKind.String);
                    windowsTimeKey.Close();
                    LogHelper.WriteLog("System Datetime after updating Successfully was " + systemTimeHelper.Year
                                                       + "-" + systemTimeHelper.Month
                                                       + "-" + systemTimeHelper.Day
                                                       + " " + systemTimeHelper.Hour
                                                       + ":" + systemTimeHelper.Minute
                                                       + ":" + systemTimeHelper.Day);

                    syncSuccessfully = true;
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(ex);

                    systemTimeHelper.UpdateNewDay();
                    LogHelper.WriteLog("Trying with new Registry Datetime: " + systemTimeHelper.Year
                                                       + "-" + systemTimeHelper.Month
                                                       + "-" + systemTimeHelper.Day
                                                       + " " + systemTimeHelper.Hour
                                                       + ":" + systemTimeHelper.Minute
                                                       + ":" + systemTimeHelper.Day);
                    syncSuccessfully = false;
                    timer.Enabled = true;
                }
            }
        }

        protected override void OnStop()
        {
            // TODO:Nothing to do
            timer.Enabled = false;
        }
    }
}
