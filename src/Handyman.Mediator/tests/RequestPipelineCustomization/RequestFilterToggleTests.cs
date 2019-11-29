﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Handyman.Mediator.RequestPipelineCustomization;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Handyman.Mediator.Tests.RequestPipelineCustomization
{
    public class RequestFilterToggleTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ShouldToggleRequestFilter(bool toggleEnabled)
        {
            var toggle = new RequestFilterToggle<Request, object> { Enabled = toggleEnabled };
            var toggledFilter = new ToggledEnabledRequestFilter();
            var fallbackFilter = new ToggleDisabledRequestFilter();

            var services = new ServiceCollection();

            services.AddScoped<IMediator>(x => new Mediator(x));
            services.AddSingleton<IRequestFilterToggle<Request, object>>(toggle);
            services.AddSingleton<IRequestFilter<Request, object>>(toggledFilter);
            services.AddSingleton<IRequestFilter<Request, object>>(fallbackFilter);
            services.AddTransient<IRequestHandler<Request, object>, RequestHandler>();

            var mediator = services.BuildServiceProvider().GetService<IMediator>();

            await mediator.Send(new Request());

            toggle.RequestFilterType.ShouldBe(typeof(ToggledEnabledRequestFilter));
            toggledFilter.Executed.ShouldBe(toggleEnabled);
            fallbackFilter.Executed.ShouldBe(!toggleEnabled);
        }

        [RequestFilterToggle(typeof(ToggledEnabledRequestFilter), ToggleDisabledFilterType = typeof(ToggleDisabledRequestFilter))]
        private class Request : IRequest<object> { }

        private class ToggledEnabledRequestFilter : IRequestFilter<Request, object>
        {
            public bool Executed { get; set; }

            public Task<object> Execute(RequestPipelineContext<Request> context, RequestFilterExecutionDelegate<object> next)
            {
                Executed = true;
                return next();
            }
        }

        private class ToggleDisabledRequestFilter : IRequestFilter<Request, object>
        {
            public bool Executed { get; set; }

            public Task<object> Execute(RequestPipelineContext<Request> context, RequestFilterExecutionDelegate<object> next)
            {
                Executed = true;
                return next();
            }
        }

        private class RequestFilterToggle<TRequest, TResponse> : IRequestFilterToggle<TRequest, TResponse>
            where TRequest : IRequest<TResponse>
        {
            public bool Enabled { get; set; }
            public Type RequestFilterType { get; set; }

            public Task<bool> IsEnabled(Type requestFilterType, RequestPipelineContext<TRequest> context)
            {
                RequestFilterType = requestFilterType;
                return Task.FromResult(Enabled);
            }
        }

        private class RequestHandler : RequestHandler<Request, object>
        {
            protected override object Handle(Request request, CancellationToken cancellationToken)
            {
                return null;
            }
        }
    }
}