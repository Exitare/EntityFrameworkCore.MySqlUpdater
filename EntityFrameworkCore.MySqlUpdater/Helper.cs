using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
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
            try
            {
                var conn = context.Database.GetDbConnection();
                string query = $"SELECT count(table_name) as count FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = '{schemaName}';";

                if(conn.State != ConnectionState.Open)
                {
                    conn.Close();
                    await conn.OpenAsync();
                }


                using (var command = conn.CreateCommand())
                {
                    command.CommandText = query;

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {

                        while (reader.Read())
                        {
                            return (reader.GetInt64(0));
                        }

                    }

                    return 0;
                }

            }
            catch
            {
                throw;
            }



        }

        /// <summary>
        /// Checks wether the updates table is available or not
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> IsUpdatesTableAvailable(DbContext context)
        {
            var conn = context.Database.GetDbConnection();

            try
            {
                string query = $"SHOW tables LIKE 'updates';";

                if (conn.State != ConnectionState.Open)
                {
                    conn.Close();
                    await conn.OpenAsync();
                }

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = query;
                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        return (reader.HasRows) ? true : false;
                    }
                }

            }
            catch
            {

                throw;
            }

        }

        /// <summary>
        /// Creates the 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<bool> CreateUpdateTable(DbContext context)
        {
            var conn = context.Database.GetDbConnection();
            try
            {
                string query = $"SHOW tables LIKE 'updates';";

                if (conn.State != ConnectionState.Open)
                {
                    conn.Close();
                    await conn.OpenAsync();
                }

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = query;
                    await command.ExecuteNonQueryAsync();

                    return true;
                }
            }
            catch
            {

                throw;
            }

        }


        /// <summary>
        /// Extracts the filename from a given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileName(string path)
        {
            return path.Split('/').Last();
        }



    }
}
