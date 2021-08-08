using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace iMessageManager
{
    abstract class MessageManager
    {
        public static SqliteConnection connection { get; private set; }
        private static string backupPath;
        public const string preloadPath = "./preloadMessages.db";

        public static string GetPath(string relativePath)
        {
            using (var manifestConnection = new SqliteConnection($"Data Source={Path.Combine(backupPath, "manifest.db")};Mode=ReadOnly"))
            {
                manifestConnection.Open();

                var command = manifestConnection.CreateCommand();
                command.CommandText =
                    $@"
                            SELECT fileID
                            FROM Files
                            WHERE relativePath = '{relativePath}'
                        ";

                var reader = command.ExecuteReader();
                reader.Read();

                var fileID = reader.GetString(0);

                reader.Close();


                string fileDirPath = Path.Combine(backupPath, fileID.Substring(0, 2) + "\\");

                if (Directory.Exists(fileDirPath))
                {
                    string filePath = Path.Combine(fileDirPath, fileID);
                    if (File.Exists(filePath))
                    {
                        return filePath;
                    }
                    else
                    {
                        throw new ArgumentException($"File '{relativePath}' does not exist");
                    }
                }
                else
                {
                    throw new ArgumentException($"File '{relativePath}' does not exist");
                }
            }
        }

        private static SqliteConnection GetConnection(string exactPath)
        {
            return GetConnection(exactPath, "ReadOnly");
        }

        private static SqliteConnection GetConnection(string exactPath, string mode)
        {
            var connection = new SqliteConnection($"Data Source={exactPath}{(mode != "" ? $";Mode={mode}" : "")}");
            connection.Open();
            connection.CreateFunction(
            "idmatch",
            (string id1, string id2)
                => IDMatch(id1,id2));

            return connection;
        }

        private static bool IDMatch(string id1, string id2)
        {
            try
            {
                return id1 != null && id2 != null && (id1 == id2 || Normalize(id1) == Normalize(id2));
            } catch (PhoneNumbers.NumberParseException)
            {
                return false;
            }
        }

        private static string Normalize(string id)
        {
            if (id.Contains("@"))
            {
                return id;
            } else
            {
                var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
                var phoneNumber = phoneNumberUtil.Parse(id, "US");

                return phoneNumberUtil.Format(phoneNumber, PhoneNumbers.PhoneNumberFormat.E164);
            }
        }

        public static bool LoadPreload()
        {
            if (backupPath == null)
            {
                backupPath = Properties.Settings.Default.backupPath;
            }
            connection = GetConnection(preloadPath, "");
            using (SqliteCommand attachCommand = connection.CreateCommand())
            {
                attachCommand.CommandText =
                    $@"
                            ATTACH DATABASE '{GetPath("Library/SMS/sms.db")}' AS sms;
                            ATTACH DATABASE '{GetPath("Library/AddressBook/AddressBook.sqlitedb")}' AS addressBook;
                            ATTACH DATABASE '{GetPath("Library/AddressBook/AddressBookImages.sqlitedb")}' AS addressBookImages;
                        ";

                attachCommand.ExecuteNonQuery();
            }
            return true;
        }

        public static bool LoadBackup(string path)
        {
            InfoBox popup = InfoBox.Show("Loading messages from backup...");
            backupPath = path;
            try
            {
                LoadPreload();

                using (SqliteCommand setupCommands = connection.CreateCommand())
                {
                    setupCommands.CommandText =
                        $@"
                            DROP TABLE IF EXISTS handle_contact_join; 
                            DROP TABLE IF EXISTS messages_master; 
                            CREATE TABLE handle_contact_join AS SELECT H.ROWID AS handle_id, P.first, P.last, I.data AS image_data
								FROM handle H
                                JOIN addressBook.ABMultiValue MV
                                    ON (MV.property = 3 OR MV.property = 4) AND idmatch(H.id, MV.value)
                                JOIN addressBook.ABPerson P
                                    ON MV.record_id = P.ROWID
                                LEFT OUTER JOIN addressBookImages.ABThumbnailImage I
                                    ON I.record_id = MV.record_id
								WHERE I.format = 5;
                            CREATE TABLE messages_master AS select M.ROWID AS message_id, M.text, M.handle_id, M.date, M.is_from_me, M.associated_message_guid, M.associated_message_type, CMJ.chat_id, M.guid
                                FROM sms.message M
								JOIN sms.chat_message_join CMJ
									ON CMJ.message_id = M.ROWID;
                        ";

                    setupCommands.ExecuteNonQuery();
                }
                popup.Close();
                return true;
            }
            catch (SqliteException e)
            {
                popup.Close();
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public static bool IsDatabaseLoaded()
        {
            return connection != null;
        }

        public static List<Message> GetMessagesWhere(string conditional)
        {
            return GetMessagesWhere(conditional, -1, out _);
        }

        public static List<Message> GetMessagesWhere(string conditional, int targetID, out int targetIndex)
        {
            targetIndex = -1;
            List<Message> messages = new List<Message>();
            using (var msgCommand = MessageManager.connection.CreateCommand())
            {
                msgCommand.CommandText =
                $@"
                            SELECT *
                            FROM messages_master
                            WHERE {conditional}
                            ORDER BY date ASC
                        ";
                using (var msgReader = msgCommand.ExecuteReader())
                {
                    while (msgReader.Read())
                    {

                        Contact contact;
                        // if is from me, then no contact needed
                        if (msgReader.GetInt32(msgReader.GetOrdinal("is_from_me")) == 1)
                        {
                            contact = null;
                        }
                        // otherwise, get id from handle
                        else
                        {

                            int handle_id = msgReader.GetInt32(msgReader.GetOrdinal("handle_id"));

                            if (handle_id == 0)
                            {
                                if (!msgReader.IsDBNull(msgReader.GetOrdinal("text")))
                                {
                                    MessageBox.Show($"Found a message (id {msgReader.GetInt32(msgReader.GetOrdinal("message_id"))}) with handle 0 not listed as from me but it has message body \"{msgReader.GetString(1)}\"");
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            contact = ContactsManager.FromHandle(handle_id);

                        }

                        bool valid = true;
                        string[] requiredNonNull = new string[] { "handle_id", "text", "date", "is_from_me", "guid", "message_id"};
                        foreach (string nonNull in requiredNonNull)
                        {
                            if (msgReader.IsDBNull(msgReader.GetOrdinal(nonNull)))
                            {
                                //MessageBox.Show($"Found null value at index {i} of messageID {_messageID}");
                                valid = false;
                                break;
                            }
                        }
                        if (valid)
                        {
                            Message message = new Message(
                                msgReader.GetInt32(msgReader.GetOrdinal("message_id")),
                                msgReader.GetString(msgReader.GetOrdinal("text")),
                                contact,
                                msgReader.GetInt64(msgReader.GetOrdinal("date")),
                                msgReader.GetInt32(msgReader.GetOrdinal("is_from_me")) == 1,
                                msgReader.GetGuid(msgReader.GetOrdinal("guid")));

                            if (targetID == msgReader.GetInt32(0))
                            {
                                targetIndex = messages.Count;
                            }

                            messages.Add(message);
                        }
                    }
                }
            }

            return messages;
        }
    }
}
