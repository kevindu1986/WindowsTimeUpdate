using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace WindowsTimeUpdate
{
    public class SystemTimeHelper
    {
        private int year;
        private int month;
        private int day;
        private int hour;
        private int minute;
        private int second;

        public SystemTimeHelper(int year, int month, int day, int hour, int minute, int second)
        {
            this.year = year;
            this.month = month;
            this.day = day;
            this.hour = hour;
            this.minute = minute;
            this.second = second;
        }

        public string Year
        {
            get
            {
                return year.ToString();
            }
        }

        public string Month
        {
            get
            {
                return month.ToString();
            }
        }

        public string Day
        {
            get
            {
                return day.ToString();
            }
        }

        public string Hour
        {
            get
            {
                return hour.ToString();
            }
        }

        public string Minute
        {
            get
            {
                return minute.ToString();
            }
        }

        public string Second
        {
            get
            {
                return second.ToString();
            }
        }

        public void SetValues(string year, string month, string day, string hour, string minute, string second)
        {
            if (!string.IsNullOrEmpty(year))
            {
                this.year = int.Parse(year);
            }

            if (!string.IsNullOrEmpty(month))
            {
                this.month = int.Parse(month);
            }

            if (!string.IsNullOrEmpty(day))
            {
                this.day = int.Parse(day);
            }

            if (!string.IsNullOrEmpty(hour))
            {
                this.hour = int.Parse(hour);
            }

            if (!string.IsNullOrEmpty(minute))
            {
                this.minute = int.Parse(minute);
            }

            if (!string.IsNullOrEmpty(second))
            {
                this.second = int.Parse(second);
            }
        }

        public void UpdateNewDay()
        {
            DateTime time = new DateTime(this.year, this.month, this.day, this.hour, this.minute, this.second);
            time = time.AddDays(1);

            this.year = time.Year;
            this.month = time.Month;
            this.day = time.Day;
        }

        public SystemTime GetSystemTime()
        {
            SystemTime time = new SystemTime();
            time.Year = ushort.Parse(this.Year);
            time.Month = ushort.Parse(this.Month);
            time.Day = ushort.Parse(this.Day);
            time.Hour = ushort.Parse(this.Hour);
            time.Minute = ushort.Parse(this.Minute);
            time.Second = ushort.Parse(this.Second);

            return time;
        }

        public SystemTime GetInternetTime(string serverName)
        {
            DateTime internetTime = GetNetworkTime(serverName);
            SystemTime time = new SystemTime();

            time.Year = ushort.Parse(internetTime.Year.ToString());
            time.Month = ushort.Parse(internetTime.Month.ToString());
            time.Day = ushort.Parse(internetTime.Day.ToString());
            time.Hour = ushort.Parse(internetTime.Hour.ToString());
            time.Minute = ushort.Parse(internetTime.Minute.ToString());
            time.Second = ushort.Parse(internetTime.Second.ToString());

            return time;
        }

        private DateTime GetNetworkTime(string serverName)
        {
            string ntpServer = serverName;//"time.windows.com";
            var ntpData = new byte[48];

            ntpData[0] = 0x1B;

            var addresses = Dns.GetHostEntry(ntpServer).AddressList;
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.Connect(ipEndPoint);
            socket.ReceiveTimeout = 3000;

            socket.Send(ntpData);
            socket.Receive(ntpData);
            socket.Close();

            const byte serverReplyTime = 40;

            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

            return networkDateTime.ToLocalTime();
        }

        // stackoverflow.com/a/3294698/162671
        private uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTime
    {
        public ushort Year;
        public ushort Month;
        public ushort DayOfWeek;
        public ushort Day;
        public ushort Hour;
        public ushort Minute;
        public ushort Second;
        public ushort Millisecond;
    };
}
