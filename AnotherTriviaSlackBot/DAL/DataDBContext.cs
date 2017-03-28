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
        /// <summary>
        /// Gets the user stats database set.
        /// </summary>
        /// <value>
        /// The user stats.
        /// </value>
        public DbSet<UserStats> UserStats { get; set; }

        /// <summary>
        /// Gets the bad questions database set.
        /// </summary>
        /// <value>
        /// The bad questions.
        /// </value>
        public DbSet<BadQuestion> BadQuestions { get; set; }

        /// <summary>
        /// This method is called when the model for a derived context has been initialized, but
        /// before the model has been locked down and used to initialize the context.  The default
        /// implementation of this method does nothing, but it can be overridden in a derived class
        /// such that the model can be further configured before it is locked down.
        /// </summary>
        /// <param name="modelBuilder">The builder that defines the model for the context being created.</param>
        /// <remarks>
        /// Typically, this method is called only once when the first instance of a derived context
        /// is created.  The model for that context is then cached and is for all further instances of
        /// the context in the app domain.  This caching can be disabled by setting the ModelCaching
        /// property on the given ModelBuidler, but note that this can seriously degrade performance.
        /// More control over caching is provided through use of the DbModelBuilder and DbContextFactory
        /// classes directly.
        /// </remarks>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // parse the connection string
            SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder(this.Database.Connection.ConnectionString);

            // replace |DataDirectory| with the current working folder, if present
            string connString = builder.DataSource;
            if (connString.StartsWith("|DataDirectory|", StringComparison.InvariantCultureIgnoreCase))
                connString = GetDataDirectory() + connString.Substring("|DataDirectory|".Length);

            // create the database file if it does not exist
            if (!File.Exists(connString))
                SQLiteConnection.CreateFile(connString);

            // use existing connection or open the connection and create the tables
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

        /// <summary>
        /// Creates the tables in the database
        /// </summary>
        /// <param name="conn">The connection.</param>
        private void CreateTables(DbConnection conn)
        {
            using (var cmd = conn.CreateCommand())
            {
                // create the stats table, if it does not exist
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='stats'";
                if (cmd.ExecuteScalar() == null)
                {
                    cmd.CommandText = "create table stats(user_id nvarchar, score int)";
                    cmd.ExecuteNonQuery();
                }

                // create the bad_questions table, if it does not exist
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='bad_questions'";
                if (cmd.ExecuteScalar() == null)
                {
                    cmd.CommandText = "create table bad_questions(questions_id nvarchar, user_id nvarchar)";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Gets the current working directory.
        /// </summary>
        /// <returns></returns>
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
