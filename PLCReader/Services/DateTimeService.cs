using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLCReader.Services
{
    class DateTimeService
    {
        // https://en.wikipedia.org/wiki/Unix_time
        // http://codeclimber.net.nz/archive/2007/07/10/convert-a-unix-timestamp-to-a-net-datetime/

        private static readonly DateTime unixTimeStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified);

        public DateTime UnixTimeStart => unixTimeStart;

        public DateTime UnixTimeToDateTime(long unixTimestamp)
        {
            return unixTimeStart.AddMilliseconds(unixTimestamp);
        }

        public long DateTimeToUnixTime(DateTime dateTime)
        {
            return (long)dateTime.Subtract(unixTimeStart).TotalMilliseconds;
        }

        public DateTime Now()
        {
            return DateTime.UtcNow;
        }

        public long UnixTimeNow()
        {
            DateTime now = DateTime.UtcNow;
            return DateTimeToUnixTime(now);
        }
    }
}
