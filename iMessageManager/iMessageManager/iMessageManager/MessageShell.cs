using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iMessageManager
{
    class MessageShell
    {
        public int MessageID { get; private set; }

        public MessageShell(int MessageID)
        {
            this.MessageID = MessageID;
        }
    }
}
