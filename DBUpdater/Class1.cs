using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MySQLDatabaseUpdater
{
    public class MySQLDBUpdater
    {

        DbContext Context { get; }

        public MySQLDBUpdater(DbContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Returns the table count of the given schema
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        public async Task<long> getTableCount(string schemaName)
        {
            string query = $"SELECT count(table_name) as count FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = '{schemaName}';";
            var conn = Context.Database.GetDbConnection();
            await conn.OpenAsync();
            var command = conn.CreateCommand();
            command.CommandText = query;
            var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            while (reader.Read())
            {
                return (reader.GetInt64(0));
            }

            return 0;
        }

    }
}
