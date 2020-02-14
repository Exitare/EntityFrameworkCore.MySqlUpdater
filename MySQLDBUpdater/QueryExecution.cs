using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySqlUpdater
{
    public partial class MySqlUpdater
    {


        /// <summary>
        /// Inserts a new update object into the db
        /// </summary>
        public async void Insert<T>(DbContext context, string name, string hash)
        {
            try
            {
                string query = $"INSERT INTO updates (name, hash, state, timestamp, speed) VALUES(?,?,?,?,?);";
                var conn = context.Database.GetDbConnection();
                await conn.OpenAsync();
                var command = conn.CreateCommand();
                command.CommandText = query;
                await command.ExecuteNonQueryAsync();

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        /// <summary>
        /// Executes the given sql file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="content"></param>
        /// <param name="testSQL"></param>
        /// <returns></returns>
        public static async Task<UpdateStatusCodes> ExecuteQueries(DbContext context, string content, bool testSQL = false)
        {
            if (testSQL)
                if (!MySqlUpdater.ValidateContent(content))
                    return UpdateStatusCodes.INSECURE_SQL_QUERY;
            try
            {
                await context.Database.ExecuteSqlRawAsync(content);
                return UpdateStatusCodes.SUCCESS;
            }
            catch
            {
                return await ExecuteQueries(context, content);
            }
        }

        /// <summary>
        /// Fallback method if ef core one fails
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        private static async Task<UpdateStatusCodes> ExecuteQueries(DbContext context, string content)
        {
            var conn = context.Database.GetDbConnection();
            await conn.OpenAsync();
            var command = conn.CreateCommand();
            command.CommandText = content;
            try
            {
                await command.ExecuteNonQueryAsync();
                return UpdateStatusCodes.SUCCESS;
            }
            catch
            {
                return UpdateStatusCodes.ERROR_EXECUTING_SQL;
            }
        }


        /// <summary>
        /// Checks if the update is already applied to the db
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private static async Task<SHAStatus> IsUpdateAlreadyApplied(DbContext context, string filename, string hashsum)
        {

            string query = $"SELECT hash FROM updates WHERE name = '{filename}';";
            var conn = context.Database.GetDbConnection();
            await conn.OpenAsync();
            var command = conn.CreateCommand();
            command.CommandText = query;
            var reader = await command.ExecuteReaderAsync();
            if (!reader.HasRows)
                return SHAStatus.NOT_APPLIED;

            while (reader.Read())
            {
                if (hashsum == reader.GetString(0))
                    return SHAStatus.EQUALS;

                return SHAStatus.CHANGED;
            }

            return SHAStatus.NOT_APPLIED;
        }
    }
}
