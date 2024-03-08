// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Microsoft.Extensions.DependencyInjection;

namespace Econolite.Ode.Repository.TimService;

public static class TimRepositoryExtension
{
    public static IServiceCollection AddTimRepository(this IServiceCollection services)
    {
        services.AddTransient<ITimRepository, TimRepository>();

        return services;
    }
    
    public static IServiceCollection AddTimRsuStatusRepository(this IServiceCollection services)
    {
        services.AddTransient<ITimRsuStatusRepository, TimRsuStatusRepository>();

        return services;
    }
}
