using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iMessageManager
{
    public class Conversation
    {
        public List<Contact> Members { get; private set; }
        public string Name { get; private set; }

        public Conversation(string name, Contact contact) : this(name, new List<Contact> { contact }) { }

        public Conversation(string name, List<Contact> members)
        {
            this.Name = name;
            this.Members = members;
        }

        public static Conversation FromChatID(int chatID)
        {
            string display_name = null;
            List<Contact> members = new List<Contact>();
            using (var nameCommand = MessageManager.connection.CreateCommand())
            {
                nameCommand.CommandText =
                    $@"
                        SELECT display_name FROM sms.chat
                        WHERE ROWID = {chatID};
                    ";
                using (var nameReader = nameCommand.ExecuteReader())
                {
                    if (nameReader.Read())
                    {
                        if (!nameReader.IsDBNull(0))
                        {
                            display_name = nameReader.GetString(0);
                        }
                    }
                }
            }
            using (var handlesCommand = MessageManager.connection.CreateCommand())
            {
                handlesCommand.CommandText =
                    $@"
                        SELECT handle_id FROM sms.chat_handle_join
                        WHERE chat_id = {chatID};
                    ";
                using (var handlesReader = handlesCommand.ExecuteReader())
                {
                    while (handlesReader.Read())
                    {
                        if (!handlesReader.IsDBNull(0))
                        {
                            members.Add(ContactsManager.FromHandle(handlesReader.GetInt32(0)));
                        }
                    }
                }
            }

            if (display_name == null || display_name == "")
            {
                if (members.Count == 1 && members[0] != null)
                {
                    display_name = members[0].displayName;
                } else
                {
                    display_name = "Unnamed Conversation";
                }
            }

            return new Conversation(display_name, members);
        }
    }
}
