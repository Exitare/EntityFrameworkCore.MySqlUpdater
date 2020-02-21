using System;
using Microsoft.EntityFrameworkCore;

namespace TestProject
{
    public class TestContext: DbContext
    {
        public TestContext()
        {
        }

        public TestContext(DbContextOptions<TestContext> options): base(options)
        { }
    }
}
