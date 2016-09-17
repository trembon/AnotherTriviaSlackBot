using AnotherTriviaSlackBot.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnotherTriviaSlackBot.DAL
{
    public class DataDBContext : DbContext
    {
        public DbSet<UserStats> UserStats { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder(this.Database.Connection.ConnectionString);

            string connString = builder.DataSource;
            if (connString.StartsWith("|DataDirectory|", StringComparison.InvariantCultureIgnoreCase))
                connString = GetDataDirectory() + connString.Substring("|DataDirectory|".Length);

            if (!File.Exists(connString))
                SQLiteConnection.CreateFile(connString);

            if (this.Database.Connection.State == ConnectionState.Open)
            {
                CreateTables(this.Database.Connection);
            }
            else
            {
                using (var conn = new SQLiteConnection(this.Database.Connection.ConnectionString))
                {
                    conn.Open();
                    CreateTables(conn);
                }
            }
        }

        private void CreateTables(DbConnection conn)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='stats'";
                if (cmd.ExecuteScalar() == null)
                {
                    cmd.CommandText = "create table stats(user_id nvarchar, score int)";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static string GetDataDirectory()
        {
            string text = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
            if (string.IsNullOrEmpty(text))
            {
                text = AppDomain.CurrentDomain.BaseDirectory;
            }
            return text;
        }
    }
}
