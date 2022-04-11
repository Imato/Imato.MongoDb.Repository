using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imato.MongoDb.Repository.Test
{
    public class MongoDbTests
    {
        private IMongoDb db = Utils.GetDb();
        private AppName app = new AppName();

        private ILogEntry[] logs = new ILogEntry[]
        {
            new LogEntry
            {
                Message = "Log entry 1"
            },
            new LogEntry
            {
                Message = "Log entry 2",
                Level = LogLevel.Debug
            },
            new LogEntry
            {
                Message = "Log entry 3",
                Level = LogLevel.Error
            },
            new LogEntry
            {
                Message = "Log entry 4",
                Level = LogLevel.Info
            },
            new LogEntry
            {
                Message = "Log entry 5",
                Level = LogLevel.Warning
            },
            new LogEntry
            {
                Message = "Log entry 6",
                Level = LogLevel.Error,
                Source = "Test source",
                Parameters = new { id = 1, str = "Test" }
            },
        };

        [SetUp]
        public void SetUp()
        {
            db.Clean();
        }

        [Test]
        public async Task GetParameterAsync()
        {
            var exists = await db.GetParameterAsync<AppName>();
            Assert.IsNull(exists);
            await db.SetParameterAsync(app);
            exists = await db.GetParameterAsync<AppName>();
            Assert.AreEqual(app.Name, exists?.Name);
            Assert.AreEqual(app.Version, exists?.Version);
        }

        [Test]
        public async Task UpdateParameterAsync()
        {
            await db.SetParameterAsync(app);
            await db.SetParameterAsync(new AppName
            {
                Name = app.Name,
                Version = "1.0.0"
            });
            var exists = await db.GetParameterAsync<AppName>();
            Assert.AreEqual(app.Name, exists?.Name);
            Assert.AreEqual("1.0.0", exists?.Version);
        }

        [Test]
        public async Task GetParametersListAsync()
        {
            var list = new List<AppName>();
            list.Add(app);
            list.Add(new AppName
            {
                Name = "New app",
                Version = "1.0.0"
            });
            list.Add(new AppName
            {
                Name = "Test app",
                Version = "6.0.1"
            });
            await db.SetParameterAsync(list.AsEnumerable());
            var result = (await db.GetParameterValuesAsync<AppName>()).ToArray();
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual(app.Name, result[0].Name);
            Assert.AreEqual(app.Version, result[0].Version);
        }

        [Test]
        public async Task SetParamaterByNameAsync()
        {
            var cfg = new TestConfig
            {
                LastStart = DateTime.UtcNow
            };
            await db.SetParameterAsync(cfg, "LastStartDate");
            var dbCfg = await db.GetParameterAsync<TestConfig>("LastStartDate");
            Assert.AreEqual(0, (int)(cfg.LastStart - dbCfg.LastStart).TotalSeconds);
        }

        [Test]
        public async Task WriteLogAsync()
        {
            var log = new LogEntry
            {
                Message = "Test message",
                Parameters = app,
                Source = "Tests",
                Level = LogLevel.Warning
            };
            await db.WriteLogAsync(log);
            var logs = (await db.GetLogsAsync()).ToArray();
            Assert.AreEqual(1, logs.Length);
            Assert.AreEqual(log.Message, logs[0].Message);
            Assert.AreEqual(log.Level, logs[0].Level);
        }

        [Test]
        public async Task GetLogsAsync()
        {
            await db.WriteLogsAsync(logs);
            var resutl = (await db.GetLogsAsync(level: LogLevel.Warning)).ToArray();
            Assert.AreEqual(3, resutl.Length);
        }
    }
}