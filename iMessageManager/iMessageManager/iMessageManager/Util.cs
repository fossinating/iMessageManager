using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iMessageManager
{
    abstract class Util
    {
        public static string cocoaToReadable(long timestamp)
        {
            DateTime dateTime = new DateTime(2001, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dateTime.AddTicks(timestamp/100).ToLocalTime().ToString();
        }
    }
}
