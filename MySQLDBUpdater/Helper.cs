using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySqlUpdater
{
    public partial class MySqlUpdater
    { 
  



        /// <summary>
        /// Returns the table count of the given schema
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        public static async Task<long> GetTableCount(DbContext context, string schemaName)
        {
            string query = $"SELECT count(table_name) as count FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = '{schemaName}';";
            var conn = context.Database.GetDbConnection();
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

        /// <summary>
        /// Checks wether the updates table is available or not
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> IsUpdatesTableAvailable(DbContext context)
        {
            string query = $"SHOW tables LIKE 'updates';";
            var conn = context.Database.GetDbConnection();
            await conn.OpenAsync();
            var command = conn.CreateCommand();
            command.CommandText = query;
            var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            return (reader.HasRows) ? true : false;
        }

        /// <summary>
        /// Creates the 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<bool> CreateUpdateTable(DbContext context)
        {
            string query = $"SHOW tables LIKE 'updates';";
            var conn = context.Database.GetDbConnection();
            await conn.OpenAsync();
            var command = conn.CreateCommand();
            command.CommandText = query;
            await command.ExecuteNonQueryAsync();
            return true;
        }

     
    }
}
