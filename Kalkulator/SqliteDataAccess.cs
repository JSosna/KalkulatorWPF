using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kalkulator
{
    class SqliteDataAccess
    {
        public static string[] Load_Numbers()
        {
            using (IDbConnection connection = new SQLiteConnection(Load_Connection_String()))
            {
                var output = connection.Query<string>("SELECT Number FROM SavedNumbers", new DynamicParameters());
                return output.ToArray();
            }
        }

        public static void Save_Number(string num)
        {
            using (IDbConnection connection = new SQLiteConnection(Load_Connection_String()))
            {
                connection.Execute($"INSERT INTO SavedNumbers (Number) VALUES ('{num}')");
            }
        }

        public static void Delete_Number(string num)
        {
            using (IDbConnection connection = new SQLiteConnection(Load_Connection_String()))
            {
                connection.Execute($"DELETE FROM SavedNumbers WHERE number='{num}'");
            }
        }

        private static string Load_Connection_String(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}
