using System;

namespace TinySharpZip
{
    public abstract class ZipEntry
    {
        DateTime lastModified;

        internal ZipEntry()
        {
            lastModified = DateTime.Now;
        }

        public DateTime LastModified
        {
            get { return lastModified; }
            set { lastModified = value; }
        }

        internal uint GetLastModifiedDateTime()
        {
            uint year = (uint)lastModified.Year;
            uint month = (uint)lastModified.Month;
            uint day = (uint)lastModified.Day;
            uint hour = (uint)lastModified.Hour;
            uint minute = (uint)lastModified.Minute;
            uint second = (uint)lastModified.Second;

            if (year < 1980)
            {
                year = 1980;
                month = 1;
                day = 1;
                hour = 0;
                minute = 0;
                second = 0;
            }
            else if (year > 2107)
            {
                year = 2107;
                month = 12;
                day = 31;
                hour = 23;
                minute = 59;
                second = 59;
            }

            uint dosTime = ((year - 1980) & 0x7f) << 25 |
                           (month << 21) |
                           (day << 16) |
                           (hour << 11) |
                           (minute << 5) |
                           (second >> 1);
            return dosTime;
        }

        internal void SetLastModifiedDateTime(uint dosTime)
        {
            uint second = Math.Min(59, 2 * (dosTime & 0x1f));
            uint minute = Math.Min(59, (dosTime >> 5) & 0x3f);
            uint hour = Math.Min(23, (dosTime >> 11) & 0x1f);
            uint month = Math.Max(1, Math.Min(12, ((dosTime >> 21) & 0xf)));
            uint year = ((dosTime >> 25) & 0x7f) + 1980;
            int day = Math.Max(1, Math.Min(DateTime.DaysInMonth((int)year, (int)month), (int)((dosTime >> 16) & 0x1f)));
            lastModified = new DateTime((int)year, (int)month, day, (int)hour, (int)minute, (int)second);
        }
    }
}