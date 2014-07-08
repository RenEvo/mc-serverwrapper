using System;

namespace Wrapper
{
    public static class TimeSpanExtensions
    {
        public static string ToFormattedString(this TimeSpan source)
        {
            var weeks = Math.Truncate((decimal)source.Days / 7);
            var days = Math.Truncate(source.Days - (weeks * 7));

            return string.Format("{0}w {1}d {2:0#}:{3:0#}:{4:0#}", weeks, days, source.Hours, source.Minutes, source.Seconds);
        }
    }
}
