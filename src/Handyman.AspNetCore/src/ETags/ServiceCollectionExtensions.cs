﻿using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Handyman.AspNetCore.ETags
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddETags(this IServiceCollection services)
        {
            services.AddSingleton<IETagComparer, ETagComparer>();
            services.AddSingleton<IETagConverter, ETagConverter>();
            services.AddSingleton<IETagValidator, ETagValidator>();
            services.AddSingleton<ETagModelBinder>();
            services.AddSingleton<IActionDescriptorProvider, ETagParameterBinderActionDescriptorProvider>();

#if NETSTANDARD2_0
            services.AddMvcCore(options => options.ModelBinderProviders.Insert(0, new ETagModelBinderProvider()));
#else
            services.AddControllers(options => options.ModelBinderProviders.Insert(0, new ETagModelBinderProvider()));
#endif

            return services;
        }
    }
}