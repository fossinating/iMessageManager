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
        public static SqliteConnection messagesConnection { get; private set; }
        public static SqliteConnection contactsConnection { get; private set; }
        public static SqliteConnection contactsImagesConnection { get; private set; }
        private static string backupPath;

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

        private static SqliteConnection GetConnection(string relativePath)
        {
            var connection = new SqliteConnection($"Data Source={GetPath(relativePath)};Mode=ReadOnly");
            connection.Open();
            connection.CreateFunction(
            "idmatch",
            (string id1, string id2)
                => id1 == id2 || Normalize(id1) == Normalize(id2));

            return connection;
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

        public static bool LoadBackup(string path)
        {
            backupPath = path;
            try
            {
                using (var manifestConnection = new SqliteConnection($"Data Source={Path.Combine(path, "manifest.db")};Mode=ReadOnly"))
                {
                    manifestConnection.Open();

                    messagesConnection = GetConnection("Library/SMS/sms.db");
                    contactsConnection = GetConnection("Library/AddressBook/AddressBook.sqlitedb");
                    contactsImagesConnection = GetConnection("Library/AddressBook/AddressBookImages.sqlitedb");
                    return true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public static bool IsDatabaseLoaded()
        {
            return messagesConnection != null;
        }
    }
}
