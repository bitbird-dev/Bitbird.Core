using System;
using System.Threading.Tasks;
using Bitbird.Core.Data.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bitbird.Core.Data.Net.Tests
{
    public class TestEntry : IEquatable<TestEntry>
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public bool Equals(TestEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && Age == other.Age;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TestEntry) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ Age;
            }
        }
    }

    [RedisVersioning(1)]
    public class TestEntryWithVersion : IEquatable<TestEntryWithVersion>
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }

        public bool Equals(TestEntryWithVersion other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Firstname, other.Firstname) && string.Equals(Lastname, other.Lastname);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TestEntryWithVersion) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Firstname != null ? Firstname.GetHashCode() : 0) * 397) ^ (Lastname != null ? Lastname.GetHashCode() : 0);
            }
        }
    }

    [TestClass]
    public class RedisVersioningTests
    {
        private const bool testWithLocalRedis = false;

        [TestMethod]
        public async Task TestWithVersionAsync()
        {
            if (!testWithLocalRedis)
                return;

            var redis = await Redis.ConnectAsync("127.0.0.1:6379,defaultDatabase=15,ssl=False,abortConnect=False,allowAdmin=true");
            await redis.ClearAsync();

            var key1 = "bla";
            var writtenEntry1 = new TestEntryWithVersion
            {
                Firstname = "XX",
                Lastname = "YY"
            };

            await redis.AddOrUpdateAsync(key1, writtenEntry1, null);
            var (readValue, readExists) = await redis.GetAsync<TestEntryWithVersion>(key1);

            Assert.IsTrue(readExists);
            Assert.AreEqual(writtenEntry1, readValue);
        }

        [TestMethod]
        public async Task TestWithoutVersionAsync()
        {
            if (!testWithLocalRedis)
                return;

            var redis = await Redis.ConnectAsync("127.0.0.1:6379,defaultDatabase=15,ssl=False,abortConnect=False,allowAdmin=true");
            await redis.ClearAsync();

            var key1 = "bla";
            var writtenEntry1 = new TestEntry
            {
                Name = "XY",
                Age = 10
            };

            await redis.AddOrUpdateAsync(key1, writtenEntry1, null);
            var (readValue, readExists) = await redis.GetAsync<TestEntry>(key1);

            Assert.IsTrue(readExists);
            Assert.AreEqual(writtenEntry1, readValue);
        }
    }
}
