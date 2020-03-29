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
            var conn = context.Database.GetDbConnection();
            try
            {

                string query = $@"INSERT INTO updates (name, hash, state, timestamp, speed) VALUES('{filename}','{hash}','RELEASED', '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',0);";

                await conn.OpenAsync();

                using (var command = conn.CreateCommand())
                {
                    // TODO: Add option to sepcify command timeout
                    command.CommandTimeout = 240;
                    command.CommandText = query;
                    await command.ExecuteNonQueryAsync();

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
            var conn = context.Database.GetDbConnection();

            try
            {

                await conn.OpenAsync();
                string query = $@"INSERT INTO updates (name, hash, state, timestamp, speed) VALUES('{filename}','{hash}','RELEASED', '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}',0);";

                using (var command = conn.CreateCommand())
                {
                    command.CommandTimeout = Constants.SQLTimeout; ;
                    command.CommandText = query;
                    await command.ExecuteNonQueryAsync();
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
        /// Updates the speed 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="filePath"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        private static async Task<bool> UpdateSpeed(DbContext context, string filePath, TimeSpan ts)
        {
            var conn = context.Database.GetDbConnection();
            try
            {
                var filename = Path.GetFileName(filePath);

                string query = $@"UPDATE updates SET speed = '{ts.Milliseconds}' WHERE name = '{filename}';";

                await conn.OpenAsync();

                using (var command = conn.CreateCommand())
                {
                    command.CommandTimeout = Constants.SQLTimeout; ;
                    command.CommandText = query;
                    await command.ExecuteNonQueryAsync();
                }

                return true;

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

        public static async Task UpdateHash(DbContext context, string filePath, string hash)
        {
            var conn = context.Database.GetDbConnection();
            var filename = Path.GetFileName(filePath);
            try
            {
                string query = $@"UPDATE updates SET hash = '{hash}' WHERE name = '{filename}';  ";

                await conn.OpenAsync();

                using (var command = conn.CreateCommand())
                {
                    command.CommandTimeout = Constants.SQLTimeout; 
                    command.CommandText = query;
                    await command.ExecuteNonQueryAsync();

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
        /// Executes the given sql file
        /// </summary>
        /// <param name="context"></param>
        /// <param name="content"></param>
        /// <param name="testSQL"></param>
        /// <returns></returns>
        public static async Task<TimeSpan> ExecuteQuery(DbContext context, string content)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
              

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
            finally
            {
                stopWatch.Stop();
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

            var conn = context.Database.GetDbConnection();

            try
            {

                await conn.OpenAsync();
              
                using (var command = conn.CreateCommand())
                {
                   
                    command.CommandTimeout = Constants.SQLTimeout;
                    command.CommandText = content;
                    await command.ExecuteNonQueryAsync();
                    stopWatch.Stop();
                    TimeSpan ts = stopWatch.Elapsed;
                    return ts;
                }
            }
            catch
            {

                throw;
            }
            finally
            {
                stopWatch.Stop();
                conn.Close();
            }


        }
    }
}
