using System;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using EntityFrameworkCore.MySqlUpdater;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FluentAssertions;

namespace TestProject
{
    public class UnitTest1
    {
        [Fact]
        public async Task ApplyBaseFile()
        {
            var options = new DbContextOptionsBuilder<TestContext>()
               .UseMySql("server=127.0.0.1;port=3306;database=test;uid=test;pwd=test")
               .Options;

            var context = new TestContext(options);
            
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);
            var path = Path.Combine(dirPath, "../../../SQL/Base/");
            var result = await context.ApplyBaseFile("accounts", path);
            result.Should().BeTrue();
        }


        [Fact]
        public async Task ApplyUpdates()
        {

            var options = new DbContextOptionsBuilder<TestContext>()
             .UseMySql("server=127.0.0.1;port=3306;database=test;uid=test;pwd=test")
             .Options;

            var context = new TestContext(options);


            List<string> folders = new List<string> { "./SQL/Updates/Account" };
            var result = await context.ApplyUpdates(folders, true).ConfigureAwait(false);
            result.Should().BeTrue();
        }


        [Fact]
        public async Task IsTablePopulated()
        {

            var options = new DbContextOptionsBuilder<TestContext>()
             .UseMySql("server=127.0.0.1;port=3306;database=test;uid=test;pwd=test")
             .Options;

            var context = new TestContext(options);


            var result = await context.IsSchemaPopulated("test");
            result.Should().BeTrue();

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
