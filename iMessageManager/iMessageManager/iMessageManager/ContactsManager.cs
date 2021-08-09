using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Data.Sqlite;

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
            using(var multiValueCommand = MessageManager.connection.CreateCommand()){
                multiValueCommand.CommandText =
                $@"SELECT record_id FROM addressBook.ABMultiValue
                    WHERE (property = 3 OR property = 4) AND idmatch(value, '{id}')";

                using (var multiValueReader = multiValueCommand.ExecuteReader())
                {
                    if (multiValueReader.Read())
                    {
                        int record_id = multiValueReader.GetInt32(0);
                        using (var personCommand = MessageManager.connection.CreateCommand())
                        {
                            personCommand.CommandText =
                                $@"SELECT FIRST, LAST, IMAGETYPE FROM addressBook.ABPerson
                            WHERE ROWID = '{record_id}'";
                            var personReader = personCommand.ExecuteReader();

                            if (personReader.Read())
                            {
                                Contact contact = new Contact(personReader.IsDBNull(0) ? "" : personReader.GetString(0), personReader.IsDBNull(1) ? "" : personReader.GetString(1));

                                if (!personReader.IsDBNull(2) && personReader.GetString(2) == "PHOTO")
                                {
                                    using (var thumbnailImageCommand = MessageManager.connection.CreateCommand())
                                    {
                                        thumbnailImageCommand.CommandText =
                                            $@"SELECT data FROM addressBookImages.ABThumbnailImage
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

        public static Contact FromReader(SqliteDataReader reader)
        {
            Contact contact = new Contact(reader.GetString(reader.GetOrdinal("first")), reader.GetString(reader.GetOrdinal("last")));

            if (!reader.IsDBNull(reader.GetOrdinal("image_data")))
            {
                byte[] h = null;
                using (MemoryStream mm = new MemoryStream())
                {
                    using (var rs = reader.GetStream(reader.GetOrdinal("image_data")))
                    {
                        rs.CopyTo(mm);
                    }
                    h = mm.ToArray();
                }
                contact.photo = h;
            }

            return contact;
        }

        public static Contact FromHandle(int handleID)
        {
            using (var command = MessageManager.connection.CreateCommand())
            {
                command.CommandText =
                    $@"
                        SELECT *
                        FROM handle_contact_join
                        WHERE handle_id = '{handleID}'";

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Contact contact = new Contact(
                            !reader.IsDBNull(reader.GetOrdinal("first")) ? reader.GetString(reader.GetOrdinal("first")) : "",
                            !reader.IsDBNull(reader.GetOrdinal("last")) ? reader.GetString(reader.GetOrdinal("last")) : ""
                            );
                        if (!reader.IsDBNull(reader.GetOrdinal("image_data")))
                        {
                            byte[] h = null;
                            using (MemoryStream mm = new MemoryStream())
                            {
                                using (var rs = reader.GetStream(reader.GetOrdinal("image_data")))
                                {
                                    rs.CopyTo(mm);
                                }
                                h = mm.ToArray();
                            }
                            contact.photo = h;
                        }
                        return contact;
                    }
                    else
                    {
                        using (var hCommand = MessageManager.connection.CreateCommand())
                        {
                            hCommand.CommandText = $@"
                                                        SELECT id
                                                        FROM sms.handle
                                                        WHERE ROWID = {handleID};
                                                    ";

                            using (var hReader = hCommand.ExecuteReader())
                            {
                                if (hReader.Read())
                                {
                                    return new Contact(hReader.GetString(hReader.GetOrdinal("id")));
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
        }
    }
}
