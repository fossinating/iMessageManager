using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace iMessageManager
{
    class Message
    {
        public int messageID { get; private set; }
        public string text { get; private set; }
        public Contact contact { get; private set; }
        public long date { get; private set; }
        public bool fromMe { get; private set; }
        public Guid guid { get; private set; }

        public Message(int messageID, string text, Contact contact, long date, bool fromMe, Guid guid) {
            this.messageID = messageID;
            this.text = text;
            this.contact = contact;
            this.date = date;
            this.fromMe = fromMe;
            this.guid = guid;
        }
        
        public MessageViewer GetMessageViewer()
        {
            return new MessageViewer(text, (contact != null) ? contact.photo : "", fromMe, Util.cocoaToReadable(date));
        }
    }
}
