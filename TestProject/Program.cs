using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;


namespace TestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            DatabaseContext db = new DatabaseContext(null!);
         
        }




    }

    public class DatabaseContext : DbContext
    {


        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {

        }


    }
}
