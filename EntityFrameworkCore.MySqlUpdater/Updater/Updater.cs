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

                        if (!Constants.HashSumTracking)
                        {
                            await ExecuteQuery(context, content);
#if DEBUG
                            Console.WriteLine($"Applied {filePath}");
#endif
                            continue;
                        }


                        SHAStatus applied = await IsUpdateAlreadyApplied(context, filePath, hash);
                        switch (applied)
                        {
                            case SHAStatus.CHANGED:
                                await ExecuteQuery(context, content);
                                await UpdateHash(context, filePath, hash);

#if DEBUG
                                Console.WriteLine($"Applied {filePath}");
#endif
                                break;

                            case SHAStatus.EQUALS:
#if DEBUG
                                Console.WriteLine($"Hashsum for {filePath} did not change!");
#endif
                                continue;

                            case SHAStatus.NOT_APPLIED:

                                TimeSpan ts = await ExecuteQuery(context, content);
                                await InsertHash(context, filePath, hash);
                                await UpdateSpeed(context, filePath, ts);

#if DEBUG
                                Console.WriteLine($"Applied {filePath}");
#endif
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
