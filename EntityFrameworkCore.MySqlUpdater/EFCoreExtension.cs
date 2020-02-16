
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySqlUpdater
{
    public static class MyDbContext
    {

        /// <summary>
        /// Applies all sql files, found in the specific folders
        /// </summary>
        /// <param name="db"></param>
        /// <param name="folders"></param>
        /// <param name="createUpdateFolder"></param>
        /// <returns></returns>
        public async static Task<UpdateStatusCodes> ApplyUpdates(this DbContext db, List<string> folders)
        {
            if (await MySqlUpdater.IsUpdatesTableAvailable(db))
                return await MySqlUpdater.UpdateDB(db, folders);
            else
            {
                Console.WriteLine("No updates folder detected! Aborting! Please specify createUpdateFolder = true, to create the update folder!");
                return UpdateStatusCodes.UPDATE_FOLDER_MISSING;
            }
        }

        /// <summary>
        /// Applies the given single sql file
        /// </summary>
        /// <param name="db"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async static Task<UpdateStatusCodes> ApplySQLFile(this DbContext db, string file)
        {
            string ext = Path.GetExtension(file);

            if (ext != ".sql")
                return UpdateStatusCodes.NOT_A_SQL_FILE;

            try
            {
                if (await MySqlUpdater.IsUpdatesTableAvailable(db))
                {

                    string content = File.ReadAllText(file);
                    TimeSpan ts = await MySqlUpdater.ExecuteQuery(db, content);
                    await MySqlUpdater.InsertHash(db, file);
                    return UpdateStatusCodes.SUCCESS;
                }

                return UpdateStatusCodes.UPDATE_FOLDER_MISSING;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Creates the required updates folder.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public async static Task<bool> CreateUpdatesFolder(this DbContext db)
        {
            if (!await MySqlUpdater.IsUpdatesTableAvailable(db))
                await CreateUpdatesFolder(db);

            return true;
        }

        /// <summary>
        /// Applies the provided base file to the database. If the db is already filled with tables, this function will return false
        /// and will not execute anything.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="schemaName"></param>
        /// <param name="baseFilePath"></param>
        /// <returns></returns>
        public async static Task<UpdateStatusCodes> ApplyBaseFile(this DbContext db, string schemaName, string baseFilePath)
        {
            if (await MySqlUpdater.GetTableCount(db, schemaName) == 0)
            {
                try
                {
                    string content = File.ReadAllText(baseFilePath);
                    await MySqlUpdater.ExecuteQuery(db, content);
                    return UpdateStatusCodes.SUCCESS;
                } catch
                {
                    throw;
                }
            }
            
            return UpdateStatusCodes.SCHEMA_NOT_EMPTY;
        }
    }
}
