﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Handyman.Mediator.Tests
{
    public class MediatorExtensionsTests
    {
        [Fact]
        public async Task ShouldPublishEventWithoutCancellationToken()
        {
            var services = new ServiceCollection().AddTransient<IEventHandler<TestEvent>>(_ => new TestEventHandler());
            var mediator = new Mediator(services.BuildServiceProvider());

            var @event = new TestEvent();

            await mediator.Publish(@event);

            @event.Handeled.ShouldBeTrue();
        }

        [Fact]
        public async Task ShouldSendRequestWithoutCancellationToken()
        {
            var services = new ServiceCollection().AddTransient<IRequestHandler<TestRequest, string>>(_ => new TestRequestHandler());
            var mediator = new Mediator(services.BuildServiceProvider());

            (await mediator.Send(new TestRequest { Response = "success" })).ShouldBe("success");
        }

        private class TestEvent : IEvent
        {
            public bool Handeled { get; set; }
        }

        private class TestEventHandler : EventHandler<TestEvent>
        {
            protected override void Handle(TestEvent @event)
            {
                @event.Handeled = true;
            }
        }

        private class TestRequest : IRequest<string>
        {
            public string Response { get; set; }
        }

        private class TestRequestHandler : RequestHandler<TestRequest, string>
        {
            protected override string Handle(TestRequest request, CancellationToken cancellationToken) => request.Response;
        }
    }
}