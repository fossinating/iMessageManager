using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace iMessageManager
{
    abstract class ContactsManager
    {
        public static List<Contact> contacts { get; private set; }

        // REDO USING BACKED UP CONTACTS UNDER ADDRESS BOOK

        public static bool getContactsFromVCard(string path)
        {
            contacts = new List<Contact>();
            StreamReader file = new StreamReader(path);
            string line;
            Contact currentContact = new Contact();
            while ((line = file.ReadLine()) != null)
            {
                if (line.StartsWith("BEGIN:VCARD"))
                {
                    currentContact = new Contact();
                } else if (line.StartsWith("FN:"))
                {
                    currentContact.fullName = line.Substring(3);
                } else if (line.StartsWith("N:"))
                {
                    string[] name = line.Substring(2).Split(';');
                    currentContact.firstName = name[1];
                    currentContact.lastName = name[0];
                } else if (line.StartsWith("TEL:") || line.StartsWith("TEL;"))
                {
                    var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
                    var phoneNumber = phoneNumberUtil.Parse(line.Substring(line.IndexOf(":") + 1), "US");

                    currentContact.phoneNumbers.Add(phoneNumberUtil.Format(phoneNumber, PhoneNumbers.PhoneNumberFormat.E164));
                } else if (line.StartsWith("EMAIL:"))
                {
                    currentContact.emails.Add(line.Substring(line.IndexOf(":") + 1));
                } else if (line.StartsWith("PHOTO:"))
                {
                    currentContact.photo = line.Substring(6) + file.ReadLine().Substring(1);
                } else if (line.StartsWith("END:VCARD"))
                {
                    contacts.Add(currentContact);
                }
            }
            return true;
        }

        public static Contact getContact(string id) // id can be either a phone number or an email
        {
            foreach (Contact contact in contacts)
            {
                foreach(string contact_id in contact.phoneNumbers.Concat(contact.emails).ToList())
                {
                    if (contact_id.ToLower() == id.ToLower())
                    {
                        return contact;
                    }
                }
            }
            return null;
        }
    }
}
