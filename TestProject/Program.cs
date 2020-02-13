using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MySQLDBUpdater;

namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            DatabaseContext db = new DatabaseContext(null!);
            MySQLDBUpdater.MySQLDBUpdater updater = new MySQLDBUpdater.MySQLDBUpdater(db, new List<string>());
            // db.ApplyUpdates();

        }




    }

    public class DatabaseContext : DbContext
    {


        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {

        }


    }
}
