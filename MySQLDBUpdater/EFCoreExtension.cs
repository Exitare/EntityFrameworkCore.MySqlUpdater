
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MySQLDBUpdater
{
    public static class MyDbContext
    {
      
        /// <summary>
        /// Applies all sql files, found in the specific folders
        /// </summary>
        /// <param name="db"></param>
        /// <param name="folders"></param>
        /// <returns></returns>
        public async static Task<bool> ApplyUpdates(this DbContext db, List<string> folders)
        {
            return true;
        }

        public async static Task<bool> ApplyBaseFiles(this DbContext db, string baseFilePath)
        {
            return true;
        }
    }
}
