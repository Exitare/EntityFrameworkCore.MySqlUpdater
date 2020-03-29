using System;
using Microsoft.EntityFrameworkCore;

namespace TestProject
{
    public class TestContext: DbContext
    {

        public TestContext(DbContextOptions<TestContext> options): base(options)
        { }
    }
}
