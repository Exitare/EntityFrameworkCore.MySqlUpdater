using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MySQLDBUpdater
{
    public partial class MySQLDBUpdater
    {

        /// <summary>
        /// Checks if the update is already applied to the db
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private async Task<SHAStatus> UpdateAlreadyApplied(string filename, string hashsum)
        {

            string query = $"SELECT hash FROM updates WHERE name = '{filename}';";
            var conn = Context.Database.GetDbConnection();
            await conn.OpenAsync();
            var command = conn.CreateCommand();
            command.CommandText = query;
            var reader = await command.ExecuteReaderAsync();
            Console.WriteLine(reader.HasRows);
            if (!reader.HasRows)
                return SHAStatus.NOT_APPLIED;

            while (reader.Read())
            {
                if (hashsum == reader.GetString(0))
                    return SHAStatus.EQUALS;

                return SHAStatus.CHANGED;
            }

            return SHAStatus.NOT_APPLIED;
        }
    }
}
