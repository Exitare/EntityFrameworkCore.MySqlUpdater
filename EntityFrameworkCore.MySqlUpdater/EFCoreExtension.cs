
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
        /// <param name="creatueUpdateFolder"></param>
        /// <returns></returns>
        public async static Task<bool> ApplyUpdates(this DbContext db, List<string> folders, bool creatueUpdateFolder =  false)
        {
            if (await MySqlUpdater.IsUpdatesTableAvailable(db))
               return await MySqlUpdater.UpdateDB(db, folders);
            else
                // TODO: if createupdate is false, return with warning.
               return await MySqlUpdater.UpdateDB(db, folders, creatueUpdateFolder);

        }

        public async static Task<UpdateStatusCodes> ApplySQLFile(this DbContext db, string file)
        {
            string ext = Path.GetExtension(file);

            if (ext != ".sql")
                return UpdateStatusCodes.NOT_A_SQL_FILE;

            try
            {
                string content = File.ReadAllText(file);
                return await MySqlUpdater.ExecuteQueries(db, content);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Applies the provided base file to the database. If the db is already filled with tables, this function will return false
        /// and will not execute anything.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="schemaName"></param>
        /// <param name="baseFilePath"></param>
        /// <returns></returns>
        public async static Task<bool> ApplyBaseFile(this DbContext db, string schemaName, string baseFilePath)
        {
            if (await MySqlUpdater.GetTableCount(db, schemaName) == 0)
            {
                try
                {
                    string content = File.ReadAllText(baseFilePath);
                    await MySqlUpdater.ExecuteQueries(db, content);
                    return true;
                } catch
                {
                    throw;
                }
            }
            
            return false;
        }
    }
}
