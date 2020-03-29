
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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
        /// <returns></returns>
        public async static Task<UpdateStatusCodes> ApplyBaseFile(this DbContext db, string schemaName, string baseFilePath, int timeOut = 60, bool debugOutput = false)
        {
            Constants.DebugOutput = debugOutput;
            Constants.SQLTimeout = timeOut;

            if (await MySqlUpdater.GetTableCount(db, schemaName) == 0)
            {
                try
                {
                    string content = File.ReadAllText(baseFilePath);
                    await MySqlUpdater.ExecuteQuery(db, content);
                    return UpdateStatusCodes.SUCCESS;
                }
                catch
                {
                    throw;
                }
            }

            return UpdateStatusCodes.SCHEMA_NOT_EMPTY;
        }

        /// <summary>
        /// Applies all sql files, found in the specific folders
        /// </summary>
        /// <param name="db"></param>
        /// <param name="folders"></param>
        /// <param name="createUpdateFolder"></param>
        /// <param name="hashsumTracking">Activate the hashsum tracking</param>
        /// <returns></returns>
        public async static Task<UpdateStatusCodes> ApplyUpdates(this DbContext db, List<string> folders, bool hashSumTracking = true, int timeOut = 60, bool debugOutput = false)
        {
            if (timeOut <= 0)
                timeOut = 60;

            Constants.HashSumTracking = hashSumTracking;
            Constants.DebugOutput = debugOutput;
            Constants.SQLTimeout = timeOut;

            if (!hashSumTracking)
                return await MySqlUpdater.UpdateDB(db, folders);

            if (await MySqlUpdater.IsUpdatesTableAvailable(db))
                return await MySqlUpdater.UpdateDB(db, folders);
            else
            {
                if (debugOutput)
                    Console.WriteLine("No updates table detected! Aborting! Please call CreateUpdatesTable() to create the required table or set hashSumTracking = false ");
                return UpdateStatusCodes.UPDATE_TABLE_MISSING;
            }

        }

        /// <summary>
        /// Applies the given single sql file
        /// </summary>
        /// <param name="db"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async static Task<UpdateStatusCodes> ApplySQLFile(this DbContext db, string filePath, bool hashSumTracking = true, int timeOut = 60, bool debugOutput = false)
        {
            if (timeOut <= 0)
                timeOut = 60;


            Constants.DebugOutput = debugOutput;
            Constants.SQLTimeout = timeOut;
            Constants.HashSumTracking = hashSumTracking;

            string ext = Path.GetExtension(filePath);
            if (ext != ".sql")
                return UpdateStatusCodes.NOT_A_SQL_FILE;

            if (!File.Exists(filePath))
            {
                if(debugOutput)
                    Console.WriteLine($"Could not locate file {filePath}!");
                return UpdateStatusCodes.FILE_NOT_FOUND;
            }

            try
            {
                string content = File.ReadAllText(filePath);

                // Check for emtpy file
                if (string.IsNullOrWhiteSpace(content))
                    return UpdateStatusCodes.EMPTY_CONTENT;


                // Check if hashSumTracking is deactivated
                if (!hashSumTracking)
                {
                    await MySqlUpdater.ExecuteQuery(db, content);
                    return UpdateStatusCodes.SUCCESS;
                }


                if (!await MySqlUpdater.IsUpdatesTableAvailable(db))
                    return UpdateStatusCodes.UPDATE_TABLE_MISSING;

                if(debugOutput)
                    Console.WriteLine($"Applying {Path.GetFileName(filePath)}");

                TimeSpan ts = await MySqlUpdater.ExecuteQuery(db, content);

                await MySqlUpdater.InsertHash(db, filePath);
                return UpdateStatusCodes.SUCCESS;


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
        /// <returns></returns>
        public async static Task<bool> CreateUpdatesTable(this DbContext db, int timeOut = 60, bool debugOutput = false)
        {
            if (timeOut <= 0)
                timeOut = 60;


            Constants.DebugOutput = debugOutput;
            Constants.SQLTimeout = timeOut;

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
                    $" `hash` char(40) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT '' COMMENT 'sha1 hash of the sql file.',  " +
                    $"`state` enum('RELEASED','ARCHIVED') CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL DEFAULT 'RELEASED' COMMENT 'defines if an update is released or archived.', " +
                    $" `timestamp` timestamp(0) NOT NULL DEFAULT CURRENT_TIMESTAMP(0) COMMENT 'timestamp when the query was applied.'," +
                    $" `speed` int (10) UNSIGNED NOT NULL DEFAULT 0 COMMENT 'time the query takes to apply in ms.',  " +
                    $"PRIMARY KEY(`name`) USING BTREE) ENGINE = MyISAM CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = 'List of all applied updates in this database.' ROW_FORMAT = Dynamic;";


                using (var command = conn.CreateCommand())
                {
                    //TODO: Add option to specify timeout
                    command.CommandTimeout = Constants.SQLTimeout;
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

        public async static Task<bool> IsSchemaPopulated(this DbContext db, string schemaName, bool debugOutput = false)
        {
            Constants.DebugOutput = debugOutput;
            return (await MySqlUpdater.GetTableCount(db, schemaName) == 0) ? false : true;
        }


        
    }
}
