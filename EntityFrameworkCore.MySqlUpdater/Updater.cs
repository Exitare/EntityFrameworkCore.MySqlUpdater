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
        public static async Task<UpdateStatusCodes> UpdateDB(DbContext context, List<string> folders)
        {
            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    Console.WriteLine($"Directory {folder} can not be found! Skipping folder!");
                    continue;
                }

                foreach (string filePath in Directory.EnumerateFiles(folder, "*.sql").OrderBy(filename => filename))
                {
                    try
                    {
                        string content = File.ReadAllText(filePath);
                        string hash = CreateSHA1Hash(content);

                        if (string.IsNullOrEmpty(content))
                            continue;

                        SHAStatus applied = await IsUpdateAlreadyApplied(context, filePath, hash);
                        switch (applied)
                        {
                            case SHAStatus.CHANGED:
                                Console.WriteLine($"Hashsum for {filePath} changed! Updating...!");
                                await ExecuteQuery(context, content);
                                await UpdateHash(context, filePath, hash);
                                break;

                            case SHAStatus.EQUALS:
                                continue;

                            case SHAStatus.NOT_APPLIED:
                                Console.WriteLine($"Applying {filePath}");
                                TimeSpan ts = await ExecuteQuery(context, content);
                                await InsertHash(context, filePath, hash);
                                await UpdateSpeed(context, filePath, ts);

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

            return UpdateStatusCodes.SUCCESS;
        }

    }
}
