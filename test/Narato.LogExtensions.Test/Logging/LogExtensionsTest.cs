using Narato.LogExtensions.Logging;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Narato.LogExtensions.Test.Logging
{
    public class LogExtensionsTest
    {
        [Fact]
        public async Task DoesntThrowWhenNotNeededGenericTest()
        {
            var genericTask = Task.FromResult("meep");

            Assert.Equal("meep", await genericTask.LogOnException());
        }

        [Fact]
        public async Task DoesntThrowWhenNotNeededNonGenericTest()
        {
            var task = Task.Factory.StartNew(() => { });
            await task.LogOnException();
            //the fact that it didnt throw anything means the test was a success
        }

        [Fact]
        public async Task ThrowsAndLogsWhenExceptionIsThrownGenericTest()
        {
            var memoryTarget = InitLogManager();

            var throwingTask = Task.Factory.StartNew<string>(() => { throw new Exception("meep"); });

            var ex = await Assert.ThrowsAsync<Exception>(async () => await throwingTask.LogOnException());
            Assert.Equal("meep", ex.Message);

            //read the logs here
            var logs = memoryTarget.Logs;

            Assert.Equal(1, logs.Count);
            Assert.Contains("LogExtensions", logs.First());
            Assert.Contains("early stack", logs.First());
        }

        [Fact]
        public async Task ThrowsAndLogsWhenExceptionIsThrownTest()
        {
            var memoryTarget = InitLogManager();

            var throwingTask = Task.FromException(new Exception("meep"));

            var ex = await Assert.ThrowsAsync<Exception>(async () => await throwingTask.LogOnException());
            Assert.Equal("meep", ex.Message);

            //read the logs here
            var logs = memoryTarget.Logs;

            Assert.Equal(1, logs.Count);
            Assert.Contains("LogExtensions", logs.First());
            Assert.Contains("early stack", logs.First());
        }

        private MemoryTarget InitLogManager()
        {
            //first clear the config
            LogManager.Configuration = new LoggingConfiguration();

            //init with empty configuration. Add one target and one rule
            var configuration = new LoggingConfiguration();
            var memoryTarget = new MemoryTarget { Name = "mem" };

            configuration.AddTarget(memoryTarget);
            configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, memoryTarget));
            LogManager.Configuration = configuration;
            return memoryTarget;
        }
    }
}
