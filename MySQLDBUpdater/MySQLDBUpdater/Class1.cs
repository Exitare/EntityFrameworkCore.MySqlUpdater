using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MySQLDBUpdater
{
    public class MySQLDBUpdater
    {
        DbContext Context { get; }
        List<string> Folders { get; set; }

        public MySQLDBUpdater(DbContext context, List<string> folders)
        {
            Context = context;
            Folders = folders;
        }


        public async Task<bool> UpdateDB()
        {
            foreach(string folder in Folders)
            {
                try
                {
                    foreach (string file in Directory.EnumerateFiles(folder, "*.sql").OrderBy(filename => filename))
                    {
                        string content = File.ReadAllText(file);
                        string fileHash = HashSum.CreateSHA1Hash(content);

                        if (string.IsNullOrEmpty(content))
                            continue;

                        if(!await UpdateAlreadyApplied(file))
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
        public async Task<long> GetTableCount(string schemaName)
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



        /// <summary>
        /// Execute the given sql file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="content"></param>
        /// <param name="testSQL"></param>
        /// <returns></returns>
        public async Task<SQLStatusCodes> ExecuteQueries(string content, bool testSQL = false)
        {
            if (testSQL)
                if (!ValidateContent(content))
                    return SQLStatusCodes.INSECURE_SQL_QUERY;
            try
            {
                await Context.Database.ExecuteSqlRawAsync(content);
                return SQLStatusCodes.SUCCESS;
            }
            catch
            {
                return await ExecuteQueries(content);
            }
        }

       


        /// <summary>
        /// Returns the occurence of a given word in a given content string
        /// </summary>
        /// <param name="content"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        private int GetWordOccurence(string content, string searchTerm)
        {
            string[] source = content.Split(new char[] { '.', '?', '!', ' ', ';', ':', ',' }, StringSplitOptions.RemoveEmptyEntries);

            // Create the query.  Use ToLowerInvariant to match "data" and "Data"   
            var matchQuery = from word in source
                             where word.ToLowerInvariant() == searchTerm.ToLowerInvariant()
                             select word;

            return matchQuery.Count();
        }


        /// <summary>
        /// Validates the sql content.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private bool ValidateContent(string content, uint occurenceCount = 10)
        {
            if (content.ToUpper().Contains("DROP * "))
                return false;

            if (content.ToUpper().Contains("DELETE * "))
                return false;

            if (content.ToUpper().Contains("TRUNCATE"))
                return false;


            if (GetWordOccurence(content, "delete") >= occurenceCount)
                return false;

            if (GetWordOccurence(content, "DROP *") >= occurenceCount)
                return false;


            return true;
        }


        /// <summary>
        /// Fallback method if ef core one fails
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        private async Task<SQLStatusCodes> ExecuteQueries(string content)
        {
            var conn = Context.Database.GetDbConnection();
            await conn.OpenAsync();
            var command = conn.CreateCommand();
            command.CommandText = content;
            try
            {
                await command.ExecuteNonQueryAsync();
                return SQLStatusCodes.SUCCESS;
            }
            catch
            {
                return SQLStatusCodes.ERROR_EXECUTING_SQL;
            }
        }

        /// <summary>
        /// Checks if the update is already applied to the db
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private async Task<bool> UpdateAlreadyApplied(string filename)
        {

            string query = $"SELECT name, hash  FROM updates WHERE name = '{filename}';";
            var conn = Context.Database.GetDbConnection();
            await conn.OpenAsync();
            var command = conn.CreateCommand();
            command.CommandText = query;
            var reader = await command.ExecuteReaderAsync();
            Console.WriteLine(reader.HasRows);
            return (reader.HasRows) ? true : false;
        }


    }
}
