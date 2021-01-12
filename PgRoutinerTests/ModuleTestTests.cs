using System;
using Xunit;
using PgRoutiner;

namespace PgRoutinerTests
{
    public class ModuleTestTests
    {
        protected readonly string NL = Environment.NewLine;

        [Fact]
        public void RoutineModuleTest1()
        {
            var module = new RoutineModule(new Settings { Namespace = "TestNamespace", SourceHeader = null, OutputDir="dir" });

            module.AddItems($"item1{NL}");

            var expect = @"using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Norm;
using NpgsqlTypes;
using Npgsql;

namespace TestNamespace.Dir
{
item1
}
";
            Assert.Equal(expect, module.ToString());
        }

        [Fact]
        public void RoutineModuleTest2()
        {
            var module = new RoutineModule(new Settings { Namespace = "TestNamespace", SourceHeader = null, OutputDir = "/dir1/dir2" });

            module.AddItems($"item1{NL}", $"item2{NL}");

            var expect = @"using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Norm;
using NpgsqlTypes;
using Npgsql;

namespace TestNamespace.Dir1.Dir2
{
item1

item2
}
";
            Assert.Equal(expect, module.ToString());
        }

    }
}
