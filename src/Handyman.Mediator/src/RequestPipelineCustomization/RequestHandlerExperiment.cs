﻿using System.Collections.Generic;
using System.Threading;

namespace Handyman.Mediator.RequestPipelineCustomization
{
    public class RequestHandlerExperiment<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        public RequestHandlerExperimentExecution<TRequest, TResponse> BaselineExecution { get; internal set; }
        public CancellationToken CancellationToken { get; internal set; }
        public IReadOnlyCollection<RequestHandlerExperimentExecution<TRequest, TResponse>> ExperimentalExecutions { get; internal set; }
        public TRequest Request { get; internal set; }
    }
}