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
    public class RemoveAllValuesUnitTests : PostgreSqlUnitTestFixture
    {
        public RemoveAllValuesUnitTests(PostgreSqlFixture fixture) : base(fixture) { }

        [Fact]
        public void RemoveAllValues_Test1()
        {
            // Arrange
            long? start = default;
            long? end = default;

            // Act
            Connection.RemoveAllValues(start, end);

            // Assert
            // Assert.Equal(default(string), Connection.Read<string>("select your assertion value").Single());
        }

        [Fact]
        public async Task RemoveAllValuesAsync_Test1()
        {
            // Arrange
            long? start = default;
            long? end = default;

            // Act
            await Connection.RemoveAllValuesAsync(start, end);

            // Assert
            // Assert.Equal(default(string), Connection.Read<string>("select your assertion value").Single());
        }

        [Fact]
        public void RemoveAllValues_Test2()
        {
            // Arrange
            long? start = default;
            long? end = default;
            string test = default;

            // Act
            Connection.RemoveAllValues(start, end, test);

            // Assert
            // Assert.Equal(default(string), Connection.Read<string>("select your assertion value").Single());
        }

        [Fact]
        public async Task RemoveAllValuesAsync_Test2()
        {
            // Arrange
            long? start = default;
            long? end = default;
            string test = default;

            // Act
            await Connection.RemoveAllValuesAsync(start, end, test);

            // Assert
            // Assert.Equal(default(string), Connection.Read<string>("select your assertion value").Single());
        }
    }
}
