using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Thor.Core;
using Thor.Core.Abstractions;
using Thor.Core.Session.Abstractions;
using Xunit;

namespace Thor.Extensions.Hosting.Tests
{
    public class BackgroundServiceBaseTest
    {
        [Fact]
        public async Task BackgroundServiceBase_WhenExecute_ShouldPass()
        {
            // arrange
            var backgroundService = new BackgroundServiceTest();
            var host = HostHelper.Build(backgroundService);

            // act
            var hostRun = host.RunAsync();
            var executed = await backgroundService.WaitExecution;

            // assert
            Assert.True(executed);
        }

        [Fact]
        public void BackgroundServiceBase_WhenException_ShouldFireEvent()
        {
            // arrange
            var backgroundService = new BackgroundServiceTest {ShouldFail = true};
            var host = HostHelper.Build(backgroundService);
            List<TelemetryEvent> events = new List<TelemetryEvent>();

            // act
            ApplicationEventSource.Log.Listen(listener =>
            {
                var hostRun = host.RunAsync();
                Task.Delay(100).GetAwaiter().GetResult();

                events.AddRange(listener
                    .OrderedEvents
                    .Select(e => EventWrittenEventArgsExtensions.Map(e, "test"))
                    .ToArray());
            });

            // assert
            Assert.NotNull(events
                .FirstOrDefault(e => e.Message == "Unhandled exception occurred."));
        }
    }
}