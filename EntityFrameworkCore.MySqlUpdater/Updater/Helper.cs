using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySqlUpdater
{
    public partial class MySqlUpdater
    {
        /// <summary>
        /// Returns the table count of the given schema
        /// </summary>
        /// <param name="context"></param>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        public static async Task<long> GetTableCount(DbContext context, string schemaName)
        {
            var conn = context.Database.GetDbConnection();
            try
            {
                await conn.OpenAsync();

                string query = $"SELECT count(table_name) as count FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = '{schemaName}';";

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
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Checks wether the updates table is available or not
        /// </summary> 
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<bool> IsUpdatesTableAvailable(DbContext context)
        {
            var conn = context.Database.GetDbConnection();
            try
            {

                await conn.OpenAsync();

                string query = $"SHOW tables LIKE 'updates';";

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

            finally
            {
                conn.Close();
            }

        }

        /// <summary>
        /// Checks if the update is already applied to the db
        /// </summary>
        /// <param name="context"></param>
        /// <param name="filePath"></param>
        /// <param name="hashsum"></param>
        /// <returns></returns>
        private static async Task<SHAStatus> IsUpdateAlreadyApplied(DbContext context, string filePath, string hashsum)
        {
            if(Constants.DebugOutput)
                Console.WriteLine($"FileHash: {hashsum}");

            var conn = context.Database.GetDbConnection();

            try
            {
                var filename = Path.GetFileName(filePath);

                await conn.OpenAsync();

                string query = $"SELECT hash FROM updates WHERE name = '{filename}';";

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = query;
                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        if (!reader.HasRows)
                            return SHAStatus.NotApplied;

                        while (reader.Read())
                        {
                            if (Constants.DebugOutput)
                                Console.WriteLine($"DBHash: {reader.GetValue(0)}");

                            return String.Equals(hashsum, reader.GetString(0), StringComparison.CurrentCultureIgnoreCase) ? SHAStatus.Equals : SHAStatus.Changed;
                        }

                        return SHAStatus.NotApplied;
                    }

                }

            }
            catch
            {
                throw;
            }
            finally
            {
                conn.Close();
            }

        }
    }
}
