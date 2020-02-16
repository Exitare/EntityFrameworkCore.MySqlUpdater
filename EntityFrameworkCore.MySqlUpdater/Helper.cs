using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Threading.Tasks;

namespace EntityFrameworkCore.MySqlUpdater
{
    public partial class MySqlUpdater
    {
        /// <summary>
        /// Returns the table count of the given schema
        /// </summary>
        /// <param name="schemaName"></param>
        /// <returns></returns>
        public static async Task<long> GetTableCount(DbContext context, string schemaName)
        {
            try
            {
                var conn = context.Database.GetDbConnection();
                string query = $"SELECT count(table_name) as count FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = '{schemaName}';";

                if(conn.State != ConnectionState.Open)
                {
                    conn.Close();
                    await conn.OpenAsync();
                }


                using (var command = conn.CreateCommand())
                {
                    command.CommandText = query;

                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {

                        while (reader.Read())
                        {
                            return (reader.GetInt64(0));
                        }

                    }

                    return 0;
                }

            }
            catch
            {
                throw;
            }



        }

        /// <summary>
        /// Checks wether the updates table is available or not
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> IsUpdatesTableAvailable(DbContext context)
        {
          
            try
            {
                var conn = context.Database.GetDbConnection();

                string query = $"SHOW tables LIKE 'updates';";

                if (conn.State != ConnectionState.Open)
                {
                    conn.Close();
                    await conn.OpenAsync();
                }

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = query;
                    using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                    {
                        return (reader.HasRows) ? true : false;
                    }
                }

            }
            catch
            {

                throw;
            }

        }

        /// <summary>
        /// Creates the updates table
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task<bool> CreateUpdatesFolder(DbContext context)
        {
            try
            {
                var conn = context.Database.GetDbConnection();

                string query = $"DROP TABLE IF EXISTS `updates`;" +
                    $" CREATE TABLE `updates` ( " +
                    $"`name` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT 'filename with extension of the update.', " +
                    $" `hash` char(40) CHARACTER SET utf8 COLLATE utf8_general_ci NULL DEFAULT '' COMMENT 'sha1 hash of the sql file.',  " +
                    $"`state` enum('RELEASED','ARCHIVED') CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL DEFAULT 'RELEASED' COMMENT 'defines if an update is released or archived.', " +
                    $" `timestamp` timestamp(0) NOT NULL DEFAULT CURRENT_TIMESTAMP(0) COMMENT 'timestamp when the query was applied.'," +
                    $" `speed` int (10) UNSIGNED NOT NULL DEFAULT 0 COMMENT 'time the query takes to apply in ms.',  " +
                    $"PRIMARY KEY(`name`) USING BTREE) ENGINE = MyISAM CHARACTER SET = utf8 COLLATE = utf8_general_ci COMMENT = 'List of all applied updates in this database.' ROW_FORMAT = Dynamic;";

                if (conn.State != ConnectionState.Open)
                {
                    conn.Close();
                    await conn.OpenAsync();
                }

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = query;
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    return true;
                }

            }
            catch
            {

                throw;
            }
        }

        /// <summary>
        /// Creates the 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task CreateUpdateTable(DbContext context)
        {
            var conn = context.Database.GetDbConnection();
            try
            {
                string query = $"SHOW tables LIKE 'updates';";

                if (conn.State != ConnectionState.Open)
                {
                    conn.Close();
                    await conn.OpenAsync();
                }

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = query;
                    await command.ExecuteNonQueryAsync();

                    return;
                }
            }
            catch
            {

                throw;
            }

        }
    }
}
