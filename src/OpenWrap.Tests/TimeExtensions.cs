using System;

namespace Tests
{
    public static class TimeExtensions
    {
        public static TimeSpan Minutes(this int minutes)
        {
            return TimeSpan.FromMinutes(minutes);
        } 
    }
}