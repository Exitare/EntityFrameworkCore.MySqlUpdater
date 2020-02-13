
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MySQLDBUpdater
{
    public static class MyDbContext
    {
        [DbFunction("CHARINDEX")]
        public static int? CharIndex(string toSearch, string target) => throw new Exception();

      
        public static void ApplyUpdates(this DbContext db)
        {
            return;
        }
    }
}
