using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace iMessageManager
{
    abstract class MessageManager
    {
        public static SqliteConnection connection { get; private set; }

        public static bool loadMessageDatabase(string path)
        {
            try
            {
                connection = new SqliteConnection($"Data Source={path}");
                connection.Open();
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        public static bool isDatabaseLoaded()
        {
            return connection != null;
        }
    }
}
