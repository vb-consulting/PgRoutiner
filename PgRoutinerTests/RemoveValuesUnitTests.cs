// <auto-generated />
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using PgRoutiner.Test;
using Xunit;
using Norm;

namespace PgRoutinerTests
{
    public class RemoveValuesUnitTests : PostgreSqlUnitTestFixture
    {
        public RemoveValuesUnitTests(PostgreSqlFixture fixture) : base(fixture) { }

        [Fact]
        public void RemoveValues_Test1()
        {
            // Arrange
            long? start = default;
            long? end = default;

            // Act
            Connection.RemoveValues(start, end);

            // Assert
            // Assert.Equal(default(string), Connection.Read<string>("select your assertion value").Single());
        }

        [Fact]
        public async Task RemoveValuesAsync_Test1()
        {
            // Arrange
            long? start = default;
            long? end = default;

            // Act
            await Connection.RemoveValuesAsync(start, end);

            // Assert
            // Assert.Equal(default(string), Connection.Read<string>("select your assertion value").Single());
        }
    }
}