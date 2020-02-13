﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MySQLDBUpdater
{
    public partial class DBUpdater
    {
   
        public async Task<bool> UpdateDB()
        {
            foreach(string folder in Folders)
            {
                try
                {
                    foreach (string file in Directory.EnumerateFiles(folder, "*.sql").OrderBy(filename => filename))
                    {
                        string content = File.ReadAllText(file);
                        string hashSum = HashSum.CreateSHA1Hash(content);

                        if (string.IsNullOrEmpty(content))
                            continue;
                        SHAStatus shaStatus = await UpdateAlreadyApplied(file, hashSum);
                        if(shaStatus == SHAStatus.NOT_APPLIED)
                        {

                        } else if(shaStatus == SHAStatus.EQUALS)
                        {

                        } else
                        {

                        }
                    }

                } catch (Exception ex)
                {

                }
            }

            return true;
        }

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
        public static async Task<bool> UpdatesTableAvailable(DbContext context)
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
        /// Execute the given sql file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="content"></param>
        /// <param name="testSQL"></param>
        /// <returns></returns>
        public async Task<UpdateStatusCodes> ExecuteQueries(string content, bool testSQL = false)
        {
            if (testSQL)
                if (!ValidateContent(content))
                    return UpdateStatusCodes.INSECURE_SQL_QUERY;
            try
            {
                await Context.Database.ExecuteSqlRawAsync(content);
                return UpdateStatusCodes.SUCCESS;
            }
            catch
            {
                return await ExecuteQueries(content);
            }
        }

        /// <summary>
        /// Fallback method if ef core one fails
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        private async Task<UpdateStatusCodes> ExecuteQueries(string content)
        {
            var conn = Context.Database.GetDbConnection();
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

    }
}
