// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Elements;
using Econolite.Ode.Models.Tim.Messaging;
using Econolite.Ode.Monitoring.Events.Extensions;
using Econolite.Ode.Monitoring.Metrics.Extensions;
using Econolite.Ode.Persistence.Mongo;
using Econolite.Ode.Repository.TimService;
using Econolite.Ode.Router.ActionSet.Messaging.Extensions;
using Econolite.Ode.Worker.TimService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((builderContext, services) =>
    {
        services.AddMongo();
        services.AddMetrics(builderContext.Configuration, "Tim Worker")
            .AddUserEventSupport(builderContext.Configuration, _ =>
            {
                _.DefaultSource = "Tim Worker";
                _.DefaultLogName = Econolite.Ode.Monitoring.Events.LogName.SystemEvent;
                _.DefaultCategory = Econolite.Ode.Monitoring.Events.Category.Server;
                _.DefaultTenantId = Guid.Empty;
            });
        services.AddActionSetRouterSource();
        services.AddTransient<IPayloadSpecialist<TimMessage>, JsonPayloadSpecialist<TimMessage>>();
        services.AddTransient<IConsumeResultFactory<Guid, TimMessage>, ConsumeResultFactory<TimMessage>>();

        services.AddTransient<IConsumer<Guid, TimMessage>, Consumer<Guid, TimMessage>>();

        services.AddTransient<ITimRepository, TimRepository>();
        // SignalRepository looks to be for status rather than "RSU entities" now, so likely need different repo to get that info for the commented out worker code
        //services.AddTransient<ISignalRepository, SignalRepository>();

        services.AddHostedService<TimServiceWorker>();
    })
    .Build();

await host.RunAsync();
