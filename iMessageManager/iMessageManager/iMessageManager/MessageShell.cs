using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iMessageManager
{
    class MessageShell
    {
        public string MessageID { get; private set; }

        public MessageShell(string MessageID)
        {
            this.MessageID = MessageID;
        }
    }
}
