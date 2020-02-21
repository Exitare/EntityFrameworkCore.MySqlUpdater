using System;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject
{
    public class UnitTest1
    {
        [Fact]
        public async Task TestUpdateTableCreation()
        {
            var options = new DbContextOptionsBuilder<TestContext>()
               .UseMySql("server=127.0.0.1;port=3306;database=test;uid=root;pwd=root")
               .Options;

            // Run the test against one instance of the context
            var context = new TestContext(options);

            
            var service = new TestService(context);
            await service.CreateUpdatesFolder();
            await service.UpdateDB();
        }


        [Fact]
        public async Task TestApplyUpdates()
        {
            var options = new DbContextOptionsBuilder<TestContext>()
               .UseMySql("server=127.0.0.1;port=3306;database=test;uid=root;pwd=root")
               .Options;

            // Run the test against one instance of the context
            var context = new TestContext(options);


            var service = new TestService(context);
            await service.UpdateDB();


        }
    }
}
