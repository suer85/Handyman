﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Handyman.Mediator.RequestPipelineCustomization
{
    public interface IRequestFilterSelector
    {
        Task SelectFilters<TRequest, TResponse>(List<IRequestFilter<TRequest, TResponse>> filters, RequestPipelineContext<TRequest> context)
            where TRequest : IRequest<TResponse>;
    }
}