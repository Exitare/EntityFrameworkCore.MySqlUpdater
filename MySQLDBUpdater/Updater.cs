using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySqlUpdater
{
    public partial class MySqlUpdater
    {
        /// <summary>
        /// Updates the database with the sqls files provided by the given folders
        /// </summary>
        /// <param name="context"></param>
        /// <param name="folders"></param>
        /// <param name="createUpdateTable"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateDB(DbContext context, List<string> folders, bool createUpdateTable = false)
        {
            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    Console.WriteLine($"Directory {folder} can not be found! Skipping folder!");
                    continue;
                }


                foreach (string file in Directory.EnumerateFiles(folder, "*.sql").OrderBy(filename => filename))
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        string hash = CreateSHA1Hash(content);

                        if (string.IsNullOrEmpty(content))
                            continue;

                        SHAStatus applied = await IsUpdateAlreadyApplied(context, file, hash);
                        switch (applied)
                        {
                            case SHAStatus.CHANGED:
                                await ExecuteQueries(context, content);
                                break;

                            case SHAStatus.EQUALS:
                                continue;

                            case SHAStatus.NOT_APPLIED:
                                await ExecuteQueries(context, content);
                                continue;

                            default:
                                continue;
                        }

                    }
                    catch
                    {
                        throw;
                    }


                }
              
            }

            return true;
        }

    }
}
