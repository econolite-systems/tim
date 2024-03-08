// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Microsoft.Extensions.DependencyInjection;

namespace Econolite.Ode.Services.TimService;

public static class TimServiceExtension
{
    public static IServiceCollection AddTimRequestService(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<IEntityConfigurationService, EntityConfigurationService>();
        services.AddSingleton<ITargetEntityResolver, TargetEntityResolver>();
        services.AddScoped<ITimRequestService, TimRequestService>();

        return services;
    }
}
