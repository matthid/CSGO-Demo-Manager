using System;
using System.Data.SQLite;

namespace Database
{
    public class SqLiteBaseRepository : IDisposable
    {
        public static readonly bool UseDatabaseImpl = true;

        private SQLiteConnection _connection;

        public static string DbFile
        {
            get { return Environment.CurrentDirectory + "\\SimpleDb.sqlite"; }
        }

        public SQLiteConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = SimpleDbConnection();
                _connection.Open();
            }

            return _connection;
        }

        private static SQLiteConnection SimpleDbConnection()
        {
            //if (!File.Exists(DbFile))
            //{
            //    CreateDatabase();
            //}
            
            //return new SQLiteConnection("Data Source=" + DbFile);
            return new SQLiteConnection("Data Source=:memory:");
        }

        protected virtual void InitDatabase()
        {

        }
        
        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}