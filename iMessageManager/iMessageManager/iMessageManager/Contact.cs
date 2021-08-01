using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iMessageManager
{
    class Contact
    {
        public string fullName;
        public List<string> phoneNumbers = new List<string>();
        public List<string> emails = new List<string>();
        public string firstName;
        public string lastName;
        public byte[] photo;

        public Contact(string firstName, string lastName, byte[] photo)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.fullName = firstName + " " + lastName;
            this.photo = photo;
        }

        public Contact(string firstName, string lastName) : this(firstName, lastName, new byte[0]) { }
    }
}
