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
        public static async Task<bool> UpdateDb(DbContext context, List<string> folders)
        {
            foreach (var folder in folders)
            {
                if (!Directory.Exists(folder))
                {
                    if (Constants.DebugOutput)
                        Console.WriteLine($"Directory {folder} can not be found! Skipping folder!");
                    continue;
                }

                foreach (var filePath in Directory.EnumerateFiles(folder, "*.sql").OrderBy(filename => filename))
                {
                    var content = File.ReadAllText(filePath);
                    var hash = CreateSHA1Hash(content);

                    if (string.IsNullOrEmpty(content))
                        continue;

                    if (!Constants.HashSumTracking)
                    {
                        await ExecuteQuery(context, content);

                        if (Constants.DebugOutput)
                            Console.WriteLine($"Applied {filePath}");

                        continue;
                    }


                    UpdateStatus applied = await IsUpdateAlreadyApplied(context, filePath, hash);
                    switch (applied)
                    {
                        case UpdateStatus.Changed:
                            await ExecuteQuery(context, content);
                            await UpdateHash(context, filePath, hash);

                            if (Constants.DebugOutput)
                                Console.WriteLine($"Applied {filePath}");

                            break;

                        case UpdateStatus.Equals:
                            if (Constants.DebugOutput)
                                Console.WriteLine($"Hash Sum for {filePath} did not change!");
                            continue;

                        case UpdateStatus.NotApplied:
                            await ExecuteQuery(context, content);
                            await InsertHash(context, filePath, hash);

                            if (Constants.DebugOutput)
                                Console.WriteLine($"Applied {filePath}");

                            continue;


                        default:
                            continue;
                    }

                }

            }

            return true;
        }

    }
}
