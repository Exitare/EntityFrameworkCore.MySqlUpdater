
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using EntityFrameworkCore.MySqlUpdater.Service;

namespace EntityFrameworkCore.MySqlUpdater
{
    public static class MyDbContext
    {
        /// <summary>
        /// Applies the provided base file to the database. If the db is already filled with tables, this function will return false
        /// and will not execute anything.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="schemaName"></param>
        /// <param name="baseFilePath"></param>
        /// <param name="timeOut"></param>
        /// <param name="debugOutput"></param>
        /// <returns></returns>
        public  static async Task<bool> ApplyBaseFile(this DbContext db, string schemaName, string baseFilePath, uint timeOut = 60, bool debugOutput = false)
        {
            Constants.DebugOutput = debugOutput;
            Constants.SqlTimeout = timeOut;

            if (await MySqlUpdater.GetTableCount(db, schemaName) != 0)
            {
                if (debugOutput)
                    Console.WriteLine("Schema is not empty, skipping!");
                return true;
            }


            try
            {
                string content = File.ReadAllText(baseFilePath);
                await MySqlUpdater.ExecuteQuery(db, content);

                return true;
            }
            catch(Exception ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); 
                throw;
            }
        }

        /// <summary>
        /// Applies all sql files, found in the specific folders
        /// </summary>
        /// <param name="db"></param>
        /// <param name="folders"></param>
        /// <param name="timeOut"></param>
        /// <param name="hashSumTracking">Activate the hashsum tracking</param>
        /// <param name="debugOutput"></param>
        /// <returns></returns>
        public static async Task<bool> ApplyUpdates(this DbContext db, List<string> folders,
            bool hashSumTracking = true, uint timeOut = 60, bool debugOutput = false)
        {
            if (timeOut <= 0)
                timeOut = 60;

            Constants.HashSumTracking = hashSumTracking;
            Constants.DebugOutput = debugOutput;
            Constants.SqlTimeout = timeOut;

            if (!hashSumTracking)
                return await MySqlUpdater.UpdateDb(db, folders);

            if (await MySqlUpdater.IsUpdatesTableAvailable(db))
                return await MySqlUpdater.UpdateDb(db, folders);

            if (debugOutput)
                Console.WriteLine(
                    "No updates table detected! Aborting! Please use CreateUpdatesTable() to create the required table or set hashSumTracking = false.");
            return false;


        }

        /// <summary>
        /// Applies the given single sql file
        /// </summary>
        /// <param name="db"></param>
        /// <param name="filePath"></param>
        /// <param name="hashSumTracking"></param>
        /// <param name="timeOut"></param>
        /// <param name="debugOutput"></param>
        /// <returns></returns>
        public  static async Task<bool> ApplySQLFile(this DbContext db, string filePath, bool hashSumTracking = true, uint timeOut = 60, bool debugOutput = false)
        {
            if (timeOut <= 0)
                timeOut = 60;


            Constants.DebugOutput = debugOutput;
            Constants.SqlTimeout = timeOut;
            Constants.HashSumTracking = hashSumTracking;

            string ext = Path.GetExtension(filePath);
            if (ext != ".sql")
            {
                if(debugOutput)
                    Console.WriteLine($"{filePath} is not an sql file!");
                return false;
            }
                

            if (!File.Exists(filePath))
            {
                if(debugOutput)
                    Console.WriteLine($"Could not locate file {filePath}!");
                return false;
            }

            try
            {
                string content = File.ReadAllText(filePath);

                // Check for emtpy file
                if (string.IsNullOrWhiteSpace(content))
                {
                    if (debugOutput)
                        Console.WriteLine($"{filePath} has empty content!");
                    return false;
                }
                   

                // Check if hashSumTracking is deactivated
                if (!hashSumTracking)
                {
                    await MySqlUpdater.ExecuteQuery(db, content);
                    return true;
                }


                if (!await MySqlUpdater.IsUpdatesTableAvailable(db))
                {
                    Console.WriteLine("No updates table detected! Aborting! Please use CreateUpdatesTable() to create the required table or set hashSumTracking = false.");
                    return false;
                }


                if(debugOutput)
                    Console.WriteLine($"Applying {Path.GetFileName(filePath)}");

                await MySqlUpdater.ExecuteQuery(db, content);

                await MySqlUpdater.InsertHash(db, filePath);
                return true;


            }
            catch
            {
                throw;
            }
        }

       /// <summary>
       /// Creates the updates table.
       /// </summary>
       /// <param name="db"></param>
       /// <param name="timeOut"></param>
       /// <param name="debugOutput"></param>
       /// <returns></returns>
        public  static async Task<bool> CreateUpdatesTable(this DbContext db, uint timeOut = 60, bool debugOutput = false)
        {
            if (timeOut <= 0)
                timeOut = Constants.SqlTimeout;


            Constants.DebugOutput = debugOutput;
            Constants.SqlTimeout = timeOut;

            if (await MySqlUpdater.IsUpdatesTableAvailable(db)) {
                if(debugOutput)
                    Console.WriteLine("Updates table already exist!");
                return false;
            }
            
            var conn = db.Database.GetDbConnection();

            try
            {

                await conn.OpenAsync();

                string query = $"DROP TABLE IF EXISTS `updates`;" +
                    $" CREATE TABLE `updates` ( " +
                    $"`name` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT 'filename with extension of the update.', " +
                    $"`hash` char(40) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT '' COMMENT 'sha1 hash of the sql file.',  " +
                    $"`state` enum('RELEASED','ARCHIVED') CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL DEFAULT 'RELEASED' COMMENT 'defines if an update is released or archived.', " +
                    $"`timestamp` timestamp(0) NOT NULL DEFAULT CURRENT_TIMESTAMP(0) COMMENT 'timestamp when the query was applied.'," +
                    $"PRIMARY KEY(`name`) USING BTREE) ENGINE = MyISAM CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = 'List of all applied updates in this database.' ROW_FORMAT = Dynamic;";


                using (var command = conn.CreateCommand())
                {
                    command.CommandTimeout = (int)Constants.SqlTimeout;
                    command.CommandText = query;
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    return true;
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
        /// Checks if the schema is populated or not.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="schemaName"></param>
        /// <param name="debugOutput"></param>
        /// <returns></returns>
        public  static async Task<bool> IsSchemaPopulated(this DbContext db, string schemaName, bool debugOutput = false)
        {
            Constants.DebugOutput = debugOutput;
            return (await MySqlUpdater.GetTableCount(db, schemaName) != 0);
        }


        
    }
}
