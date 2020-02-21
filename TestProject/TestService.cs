using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EntityFrameworkCore.MySqlUpdater;


namespace TestProject
{
    public class TestService
    {
        private TestContext _context;
        public TestService(TestContext context)
        {
            _context = context;
        }


        public async Task CreateUpdatesFolder()
        {
            await _context.CreateUpdatesTable().ConfigureAwait(false);
        }

        public async Task UpdateDB()
        {
            List<string> folders = new List<string> { "../../../../TestProject/SampleData/" };
            Console.WriteLine($"Tracking: {await _context.ApplyUpdates(folders, true).ConfigureAwait(false)}");
        }

        public async Task UpdateDBWithoutTracking()
        {
            List<string> folders = new List<string> { "../../../../TestProject/SampleData/" };
            Console.WriteLine($"Without tracking: {await _context.ApplyUpdates(folders, false).ConfigureAwait(false)}");
        }

        public async Task ApplyFile()
        {
            Console.WriteLine("Apply file");
            await _context.ApplySQLFile("../../../../TestProject/SampleData/test2.sql",false);
        }

    }
}
