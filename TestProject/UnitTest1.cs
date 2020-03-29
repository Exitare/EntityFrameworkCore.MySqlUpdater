using System;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using EntityFrameworkCore.MySqlUpdater;
using System.Collections.Generic;

namespace TestProject
{
    public class UnitTest1
    {
        [Fact]
        public async Task ApplyBaseFile()
        {
            var options = new DbContextOptionsBuilder<TestContext>()
               .UseMySql("server=127.0.0.1;port=3306;database=test;uid=trinity;pwd=trinity20")
               .Options;

            var context = new TestContext(options);


     
            await context.ApplyBaseFile("test", "../../../../TestProject/Base/world_database_05.sql");
        }


        [Fact]
        public async Task ApplyUpdates()
        {

            var options = new DbContextOptionsBuilder<TestContext>()
             .UseMySql("server=127.0.0.1;port=3306;database=test;uid=trinity;pwd=trinity20")
             .Options;

            var context = new TestContext(options);


            List<string> folders = new List<string> { "../../../../TestProject/Updates/" };
            Assert.Equal(UpdateStatusCodes.SUCCESS, await context.ApplyUpdates(folders, true).ConfigureAwait(false));
        }


        [Fact]
        public async Task IsTablePopulated()
        {

            var options = new DbContextOptionsBuilder<TestContext>()
             .UseMySql("server=127.0.0.1;port=3306;database=test;uid=trinity;pwd=trinity20")
             .Options;

            var context = new TestContext(options);


            Assert.True(await context.IsSchemaPopulated("test"));
            
        }

        [Fact]
        public async Task TestUpdateDBWithoutHashSumTracking()
        {
          /*  var options = new DbContextOptionsBuilder<TestContext>()
               .UseMySql("server=127.0.0.1;port=3306;database=test;uid=root;pwd=root")
               .Options;

            // Run the test against one instance of the context
            var context = new TestContext(options);


            var service = new TestService(context);
            await service.UpdateDBWithoutTracking(); */
        }

    



    }
}
