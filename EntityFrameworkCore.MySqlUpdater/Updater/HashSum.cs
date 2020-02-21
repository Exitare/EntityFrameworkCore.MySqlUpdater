using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EntityFrameworkCore.MySqlUpdater
{
    public partial class MySqlUpdater
    {

        /// <summary>
        /// Creates a SHA1 Hashsum 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string CreateSHA1Hash(string content)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(content));
                return string.Concat(hash.Select(b => b.ToString("x2").ToUpper()));

            }
        }
    }
}
