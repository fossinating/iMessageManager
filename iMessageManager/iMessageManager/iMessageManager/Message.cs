using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace iMessageManager
{
    public class Message
    {
        public int MessageID { get; private set; }
        public string Text { get; private set; }
        public Contact Contact { get; private set; }
        public long Date { get; private set; }
        public bool FromMe { get; private set; }
        public Guid Guid { get; private set; }

        public Message(int messageID, string text, Contact contact, long date, bool fromMe, Guid guid) {
            this.MessageID = messageID;
            this.Text = text;
            this.Contact = contact;
            this.Date = date;
            this.FromMe = fromMe;
            this.Guid = guid;
        }
    }
}
