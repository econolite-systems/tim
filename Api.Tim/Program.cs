// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Authorization.Extensions;
using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Elements;
using Econolite.Ode.Messaging.Extensions;
using Econolite.Ode.Models.Tim.Messaging;
using Econolite.Ode.Monitoring.Events.Extensions;
using Econolite.Ode.Monitoring.HealthChecks.Kafka.Extensions;
using Econolite.Ode.Monitoring.HealthChecks.Mongo.Extensions;
using Econolite.Ode.Monitoring.Metrics.Extensions;
using Econolite.Ode.Persistence.Mongo;
using Econolite.Ode.Repository.TimService;
using Econolite.Ode.Services.TimService;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Econolite.Ode.Api.Tim;
using Econolite.Ode.Commands.Rsu.Messaging.Extensions;
using Econolite.Ode.Common.Scheduler.Base.Extensions;
using Econolite.Ode.Router.ActionSet.Messaging.Extensions;
using Monitoring.AspNet.Metrics;

var builder = WebApplication.CreateBuilder(args);
var AllOrigins = "_allOrigins";

Audit.Core.Configuration.Setup()
    .UseCustomProvider(new AuditMongoDataProvider(config => config
        .ConnectionString(builder.Configuration.GetConnectionString("Mongo"))
        .Database(builder.Configuration["Mongo:DbName"])
        .Collection(builder.Configuration["Collections:Audit"])
        // This is important!
        .SerializeAsBson(true)
    ));

builder.Services.AddMessaging();

builder.Services.AddMetrics(builder.Configuration, "Tim Api")
    .ConfigureRequestMetrics(c =>
    {
        c.RequestCounter = "Requests";
        c.ResponseCounter = "Responses";
    })
    .AddUserEventSupport(builder.Configuration, _ =>
    {
        _.DefaultSource = "Tim Api";
        _.DefaultLogName = Econolite.Ode.Monitoring.Events.LogName.SystemEvent;
        _.DefaultCategory = Econolite.Ode.Monitoring.Events.Category.Server;
        _.DefaultTenantId = Guid.Empty;
    });
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddAudit();
builder.Services.AddLogging();
builder.Services.AddTokenHandler(options =>
{
    options.Authority = builder.Configuration.GetValue("Authentication:Authority",
        "https://keycloak.cosysdev.com/auth/realms/moundroad")!;
    options.ClientId = builder.Configuration.GetValue("Authentication:ClientId", "")!;
    options.ClientSecret = builder.Configuration.GetValue("Authentication:ClientSecret", "")!;
});
builder.Services.AddActionSetRouterSource();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllOrigins,
        policy =>
        {
            policy.AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed(_ => true)
                .AllowCredentials();
        });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMvc(config =>
{
    config.Filters.Add(new AuthorizeFilter());
});

builder.Services.AddAuthenticationJwtBearer(builder.Configuration, builder.Environment.IsDevelopment());

builder.Services.AddSwaggerGen(c =>
{
#if DEBUG
    var basePath = AppDomain.CurrentDomain.BaseDirectory;
    var fileName = typeof(Program).GetTypeInfo().Assembly.GetName().Name + ".xml";
    c.IncludeXmlComments(Path.Combine(basePath, fileName));
#endif
    
    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
    });
                
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme,
                },
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header,
            },
            new List<string>()
        },
    });
});


builder.Services.AddMongo();
builder.Services.AddTimerFactory();
builder.Services.AddTimRequestService();
builder.Services.AddTimRepository();
builder.Services.AddTimRsuStatusRepository();

builder.Services.AddTransient<IProducer<Guid, TimMessage>, Producer<Guid, TimMessage>>();

builder.Services.Configure<MessageFactoryOptions<TimMessage>>(_ =>
{
    _.FuncBuildPayloadElement = _ => new BaseJsonPayload<TimMessage>(_);
});
builder.Services.AddTransient<IMessageFactory<Guid, TimMessage>, MessageFactory<TimMessage>>();
builder.Services.AddRsuCommandRequestSink(builder.Configuration);
builder.Services.AddRsuCommandResponseSource(builder.Configuration);
builder.Services.AddActionSetRouterSource(builder.Configuration);
builder.Services.AddHealthChecks()
    .AddProcessAllocatedMemoryHealthCheck(maximumMegabytesAllocated: 1024, name: "Process Allocated Memory", tags: new[] { "memory" })
    .AddKafkaHealthCheck()
    .AddMongoDbHealthCheck();
builder.Services.AddHostedService<TimCommandResponseHandler>();
builder.Services.AddHostedService<ActionRequestHandler>();
builder.Services.AddHostedService<MessageHandler>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(AllOrigins);
app.UseRouting();
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.AddRequestMetrics();
app.UseHealthChecksPrometheusExporter("/metrics");
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/healthz", new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    endpoints.MapControllers();
});

app.Run();
