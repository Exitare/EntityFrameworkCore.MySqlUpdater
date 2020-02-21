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
            _context.SetVerboseOutput(false);
            await _context.CreateUpdatesTable().ConfigureAwait(false);
        }

        public async Task UpdateDB()
        {
            List<string> folders = new List<string> { "../../../../TestProject/SampleData/" };
            _context.SetVerboseOutput(false);
            Console.WriteLine(await _context.ApplyUpdates(folders).ConfigureAwait(false));
        }

        
    }
}
