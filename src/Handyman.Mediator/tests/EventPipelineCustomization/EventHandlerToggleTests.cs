﻿using System;
using System.Threading.Tasks;
using Handyman.Mediator.EventPipelineCustomization;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Handyman.Mediator.Tests.EventPipelineCustomization
{
    public class EventHandlerToggleTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ShouldToggleEventHandler(bool toggleEnabled)
        {
            var toggle = new EventHandlerToggle { Enabled = toggleEnabled };
            var toggledHandler = new ToggleEnabledEventHandler();
            var fallbackHandler = new ToggleDisabledEventHandler();

            var services = new ServiceCollection();

            services.AddScoped<IMediator>(x => new Mediator(x));
            services.AddSingleton<IEventHandlerToggle<Event>>(toggle);
            services.AddSingleton<IEventHandler<Event>>(toggledHandler);
            services.AddSingleton<IEventHandler<Event>>(fallbackHandler);

            await services.BuildServiceProvider().GetService<IMediator>().Publish(new Event());

            toggle.EventHandlerType.ShouldBe(typeof(ToggleEnabledEventHandler));
            toggledHandler.Executed.ShouldBe(toggleEnabled);
            fallbackHandler.Executed.ShouldBe(!toggleEnabled);
        }

        [EventHandlerToggle(typeof(ToggleEnabledEventHandler), ToggleDisabledHandlerType = typeof(ToggleDisabledEventHandler))]
        private class Event : IEvent { }

        private class ToggleEnabledEventHandler : EventHandler<Event>
        {
            public bool Executed { get; set; }

            protected override void Handle(Event @event)
            {
                Executed = true;
            }
        }

        private class ToggleDisabledEventHandler : EventHandler<Event>
        {
            public bool Executed { get; set; }

            protected override void Handle(Event @event)
            {
                Executed = true;
            }
        }

        private class EventHandlerToggle : IEventHandlerToggle<Event>
        {
            public bool Enabled { get; set; }
            public Type EventHandlerType { get; set; }

            public Task<bool> IsEnabled(Type eventHandlerType, EventPipelineContext<Event> context)
            {
                EventHandlerType = eventHandlerType;
                return Task.FromResult(Enabled);
            }
        }
    }
}