using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySqlUpdater
{
    public partial class MySqlUpdater
    {


        /// <summary>
        /// Inserts a new update object into the db
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <param name="hash"></param>
        /// </summary>
        public async void Insert(DbContext context, string name, string hash)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                // TODO: Escape queries
                string query = $@"INSERT INTO updates (name, hash, state, timestamp, speed) VALUES({name},{hash},'RELEASED', {DateTime.UtcNow},0);";
                var conn = context.Database.GetDbConnection();
                await conn.OpenAsync();
                var command = conn.CreateCommand();
                command.CommandText = query;
                await command.ExecuteNonQueryAsync();
                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                await Update(context, name, ts);
            }
            catch
            {
                stopWatch.Stop();
                throw;
            }
        }

        /// <summary>
        /// Updates the speed 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        private async Task<bool> Update(DbContext context, string name, TimeSpan ts)
        {
            try
            {
                // TODO: Escape queries
                string query = $@"UPDATE updates SET spped = {ts.Seconds} WHERE name = {name};";
                var conn = context.Database.GetDbConnection();
                await conn.OpenAsync();
                var command = conn.CreateCommand();
                command.CommandText = query;
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async void Update(DbContext context, string name, string hash)
        {
            try
            {
                // TODO: Escape queries
                string query = $@"UPDATE updates SET hash = {hash} WHERE name = {name};  ";
                var conn = context.Database.GetDbConnection();
                await conn.OpenAsync();
                var command = conn.CreateCommand();
                command.CommandText = query;
                await command.ExecuteNonQueryAsync();

            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Executes the given sql file
        /// </summary>
        /// <param name="context"></param>
        /// <param name="content"></param>
        /// <param name="testSQL"></param>
        /// <returns></returns>
        public static async Task<UpdateStatusCodes> ExecuteQueries(DbContext context, string content, bool testSQL = false)
        {
            if (testSQL)
                if (!ValidateContent(content))
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
        /// <param name="context"></param>
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
        /// <param name="context"></param>
        /// <param name="filename"></param>
        /// <param name="hashsum"></param>
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
