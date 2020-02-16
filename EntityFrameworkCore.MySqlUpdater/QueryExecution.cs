using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
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
        public static async Task InsertHash(DbContext context, string filePath, string hash)
        {
            var filename = Path.GetFileName(filePath);
            try
            {
                var conn = context.Database.GetDbConnection();
                string query = $@"INSERT INTO updates (name, hash, state, timestamp, speed) VALUES('{filename}','{hash}','RELEASED', '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',0);";
                if (conn.State != ConnectionState.Open)
                {
                    conn.Close();
                    await conn.OpenAsync();
                }

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = query;
                    await command.ExecuteNonQueryAsync();
                 
                }
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Inserts a new update object into the db
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <param name="hash"></param>
        /// </summary>
        public static async Task InsertHash(DbContext context, string filePath)
        {
            var filename = Path.GetFileName(filePath);
            string content = File.ReadAllText(filePath);
            string hash = CreateSHA1Hash(content);
            try
            {

                var conn = context.Database.GetDbConnection();
                string query = $@"INSERT INTO updates (name, hash, state, timestamp, speed) VALUES('{filename}','{hash}','RELEASED', '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',0);";
                if (conn.State != ConnectionState.Open)
                {
                    conn.Close();
                    await conn.OpenAsync();
                }

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = query;
                    await command.ExecuteNonQueryAsync();
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Updates the speed 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="filePath"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        private static async Task<bool> UpdateSpeed(DbContext context, string filePath, TimeSpan ts)
        {
            try
            {
                var filename = Path.GetFileName(filePath);
                var conn = context.Database.GetDbConnection();
                string query = $@"UPDATE updates SET speed = '{ts.Milliseconds}' WHERE name = '{filename}';";
                if (conn.State != ConnectionState.Open)
                {
                    conn.Close();
                    await conn.OpenAsync();
                }

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = query;
                    await command.ExecuteNonQueryAsync();
                }

                return true;

            }
            catch
            {

                throw;
            }
        }

        public static async Task UpdateHash(DbContext context, string filePath, string hash)
        {
            var conn = context.Database.GetDbConnection();
            var filename = Path.GetFileName(filePath);
            try
            {
                string query = $@"UPDATE updates SET hash = '{hash}' WHERE name = '{filename}';  ";
                if (conn.State != ConnectionState.Open)
                {
                    conn.Close();
                    await conn.OpenAsync();
                }

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = query;
                    await command.ExecuteNonQueryAsync();

                }
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
        public static async Task<TimeSpan> ExecuteQuery(DbContext context, string content)
        {
            try
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                await context.Database.ExecuteSqlRawAsync(content).ConfigureAwait(false);

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;

                return ts;
            }
            catch
            {
                // Try fallback before throwing
                return await ExecuteQueryFallback(context, content).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Fallback method if ef core one fails
        /// </summary>
        /// <param name="context"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        private static async Task<TimeSpan> ExecuteQueryFallback(DbContext context, string content)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                var conn = context.Database.GetDbConnection();

                if (conn.State != ConnectionState.Open)
                {
                    conn.Close();
                    await conn.OpenAsync();
                }

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = content;
                    await command.ExecuteNonQueryAsync();
                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    return ts;
                }
            }
            catch
            {
                stopWatch.Stop();
                throw;
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
            try
            {
                var filename = Path.GetFileName(filePath);
                var conn = context.Database.GetDbConnection();
                string query = $"SELECT hash FROM updates WHERE name = '{filename}';";

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
                        if (!reader.HasRows)
                            return SHAStatus.NOT_APPLIED;

                        while (reader.Read())
                        {
                            if (hashsum == reader.GetString(0))
                                return SHAStatus.EQUALS;
   
                            return SHAStatus.CHANGED;
                        }
                    }

                    return SHAStatus.NOT_APPLIED;
                }

            }
            catch
            {
                throw;
            }

        }
    }
}
