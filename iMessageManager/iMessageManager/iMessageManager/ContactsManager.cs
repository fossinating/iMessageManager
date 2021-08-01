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

        private static string RegexID(string id)
        {
            string result = "";

            foreach (char sub in id)
            {
                result = result + "[^0-9]*" + sub;
            }

            return "/(" + result + ")/";
        }

        public static Contact GetContact(string id) // id can be either a phone number or an email
        {
            using(var multiValueCommand = MessageManager.contactsConnection.CreateCommand()){
                multiValueCommand.CommandText =
                $@"SELECT record_id FROM ABMultiValue
                    WHERE (property = 3 OR property = 4) AND idmatch(value, '{id}')";

                using (var multiValueReader = multiValueCommand.ExecuteReader())
                {
                    if (multiValueReader.Read())
                    {
                        int record_id = multiValueReader.GetInt32(0);
                        using (var personCommand = MessageManager.contactsConnection.CreateCommand())
                        {
                            personCommand.CommandText =
                                $@"SELECT FIRST, LAST, IMAGETYPE FROM ABPerson
                            WHERE ROWID = '{record_id}'";
                            var personReader = personCommand.ExecuteReader();

                            if (personReader.Read())
                            {
                                Contact contact = new Contact(personReader.IsDBNull(0) ? "" : personReader.GetString(0), personReader.IsDBNull(1) ? "" : personReader.GetString(1));

                                if (!personReader.IsDBNull(2) && personReader.GetString(2) == "PHOTO")
                                {
                                    using (var thumbnailImageCommand = MessageManager.contactsImagesConnection.CreateCommand())
                                    {
                                        thumbnailImageCommand.CommandText =
                                            $@"SELECT data FROM ABThumbnailImage
                                        WHERE record_id = '{record_id}'";

                                        using (var thumbnailImageReader = thumbnailImageCommand.ExecuteReader())
                                        {
                                            if (thumbnailImageReader.Read() && !thumbnailImageReader.IsDBNull(0))
                                            {
                                                byte[] h = null;
                                                using (MemoryStream mm = new MemoryStream())
                                                {
                                                    using (var rs = thumbnailImageReader.GetStream(0))
                                                    {
                                                        rs.CopyTo(mm);
                                                    }
                                                    h = mm.ToArray();
                                                }
                                                contact.photo = h;
                                            }
                                        }
                                    }
                                }
                                return contact;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                    else
                    {
                        return null;
                    }
                } 
            }
        }

        public static Contact FromHandle(int handleID)
        {
            using (var command = MessageManager.messagesConnection.CreateCommand())
            {
                command.CommandText =
                    $@"SELECT id FROM handle
                    WHERE ROWID = '{handleID}'";

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return GetContact(reader.GetString(0));
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }
}
