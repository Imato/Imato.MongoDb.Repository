using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Imato.MongoDb.Repository.Test
{
    public class BaseRepositoryTest
    {
        private IRepository<TestEntity> repository = null!;
        private IMongoDb db = Utils.GetDb();

        private TestEntity[] entries = new TestEntity[] {
            new TestEntity
            {
                Name = "Test entry 1",
                Date = DateTime.Now,
                Count = 1
            },
            new TestEntity
            {
                Name = "Test entry 2",
                Date = DateTime.Now.AddMinutes(-1),
                Count = 2
            },
        };

        [SetUp]
        public void SetUp()
        {
            db.Clean();
            repository = db.GetRepository<TestEntity>();
        }

        [Test]
        public async Task CreateAsync()
        {
            var r1 = await repository.CreateAsync(entries[0]);
            Assert.IsNotEmpty(r1.Id);
            Assert.IsNotNull(r1);
            Assert.AreEqual(entries[0].Name, r1.Name);
        }

        [Test]
        public async Task FindAsync()
        {
            await repository.CreateAsync(entries);
            var r1 = await repository.FindAsync(x => x.Id == entries[0].Id);
            Assert.AreEqual(entries[0].Name, r1?.Name);
            Assert.AreEqual(entries[0].Count, r1?.Count);
        }

        [Test]
        public async Task UpdateAsync()
        {
            await repository.CreateAsync(entries);
            var e = entries[0].Copy();
            e.Name = "Test copy";
            await repository.UpdateAsync(e);
            var r = await repository.FindAsync(x => x.Id == e.Id);
            Assert.AreEqual(e.Name, r?.Name);
        }

        [Test]
        public async Task FindAllAsync()
        {
            await repository.CreateAsync(entries);
            var r = (await repository.FindAllAsync()).ToList();
            Assert.AreEqual(entries.Length, r?.Count());
            Assert.AreEqual(entries[0].Count, r?.First().Count);
        }

        [Test]
        public async Task DeleteAsync()
        {
            await repository.CreateAsync(entries);
            var r1 = await repository.FindAllAsync();
            Assert.AreEqual(entries.Length, r1?.Count());
            await repository.DeleteAsync(entries[0]);
            var r2 = await repository.FindAllAsync();
            Assert.AreEqual(1, r2?.Count());
            var r3 = await repository.FindAsync(x => x.Name == entries[1].Name);
            Assert.AreEqual(entries[1].Count, r3?.Count);
        }
    }
}