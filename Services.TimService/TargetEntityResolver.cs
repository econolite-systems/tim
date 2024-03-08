// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.Entities;
using Econolite.Ode.Models.Entities.Spatial;
using Econolite.Ode.Models.Entities.Types;
using Econolite.Ode.Models.Tim;
using Econolite.Ode.Models.Tim.Messaging;
using Econolite.Ode.TimService.Models;
using Microsoft.Extensions.Logging;

namespace Econolite.Ode.Services.TimService;

public interface ITargetEntityResolver
{
    Task<IEnumerable<TargetEntity>> ResolveTargetEntities(TimRequest request,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TargetEntity>> GetTargetEntities(IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default);
}

public class TargetEntityResolver : ITargetEntityResolver
{
    private readonly IEntityConfigurationService _entityConfigurationService;
    private readonly ILogger<TargetEntityResolver> _logger;

    public TargetEntityResolver(IEntityConfigurationService entityConfigurationService, ILogger<TargetEntityResolver> logger)
    {
        _entityConfigurationService = entityConfigurationService;
        _logger = logger;
    }
    
    public async Task<IEnumerable<TargetEntity>> ResolveTargetEntities(TimRequest request, CancellationToken cancellationToken = default)
    {
        var rsus = new List<TargetEntity>();

        string? intersections;
        switch (request.TargetType)
        {
            case TargetType.None:
                break;
            case TargetType.Target:
            {
                foreach (var target in request.Target.ToArray())
                {
                    var targetEntity = await GetTargetEntity(target, cancellationToken);
                    if (targetEntity is {IsTargetValid: true, IsIntersectionValid: true})
                    {
                        rsus.Add(targetEntity);
                    }
                    else
                    {
                        _logger.LogWarning("Target {Id} for {ItisCode} is not valid", target, request.ItisCode);
                    }
                }
                break;
            }
            case TargetType.Radius:
            {
                var radius = request.Parameters.FirstOrDefault();
                if (radius is not null && request is {Longitude: not null, Latitude: not null})
                {
                
                    var entities = (await _entityConfigurationService.QueryRadiusAsync(
                            new GeoJsonPointFeature() { Coordinates = new[]{ request.Longitude.Value, request.Latitude.Value }},
                            radius,
                            cancellationToken))
                        .ToArray();
                
                    foreach (var entity in entities.Where(e => e.Type.Id == RsuTypeId.Id))
                    {
                        var parent = await _entityConfigurationService.GetByIdAsync(entity.Parent, cancellationToken);
                        if (parent == null || parent.Type.Id != IntersectionTypeId.Id) continue;
                        var targetEntity = entity.ToTargetEntity(parent.Name);
                        if (!targetEntity.IsTargetValid || !targetEntity.IsIntersectionValid) continue;
                        var point = new GeoJsonPointFeature() { Coordinates = new []{ request.Longitude.Value, request.Latitude.Value }};
                        var buffer = point.CreateBufferInMiles(double.Parse(radius));
                        targetEntity.Location = point.Coordinates;
                        if (buffer.Coordinates is null) continue;
                        targetEntity.Region = buffer.Coordinates;
                        rsus.Add(targetEntity);
                    }
                }

                break;
            }
            case TargetType.Downstream:
                intersections = request.Parameters.FirstOrDefault();
                if (intersections is not null && request is {Longitude: not null, Latitude: not null})
                {
                    var entities = (await _entityConfigurationService.QueryDownstreamAsync(
                        new GeoJsonPointFeature() { Coordinates = new []{ request.Longitude.Value, request.Latitude.Value }},
                        intersections,
                        cancellationToken)).ToArray();
                    foreach (var entity in entities)
                    {
                        if (entity.Geometry.Point?.Coordinates is null || entity.Geometry.Point.Coordinates.Length < 2)
                        {
                            _logger.LogWarning("RSU: {Name} has no point geometry", entity.Name);
                            continue;
                        }

                        var targetEntity = entity.ToTargetEntity();
                        if (targetEntity is not {IsTargetValid: true, IsIntersectionValid: true}) continue;
                        var point = new GeoJsonPointFeature() { Coordinates = new []{ entity.Geometry.Point.Coordinates[0], entity.Geometry.Point.Coordinates[1] }};
                        var buffer = point.CreateBufferInMiles(1);
                        targetEntity.Location = point.Coordinates;
                        if (buffer.Coordinates is null)
                        {
                            _logger.LogWarning("RSU: {Name} has no buffer geometry", entity.Name);
                            continue;
                        }
                        targetEntity.Region = buffer.Coordinates;
                        rsus.Add(targetEntity);
                    }
                }
                break;
            case TargetType.Upstream:
                intersections = request.Parameters.FirstOrDefault();
                if (intersections is not null && request is {Longitude: not null, Latitude: not null})
                {
                    var entities = (await _entityConfigurationService.QueryUpstreamAsync(
                        new GeoJsonPointFeature()
                            {Coordinates = new [] {request.Longitude.Value, request.Latitude.Value}},
                        intersections,
                        cancellationToken)).ToArray();
                    foreach (var entity in entities)
                    {
                        if (entity.Geometry.Point?.Coordinates is null || entity.Geometry.Point.Coordinates.Length < 2)
                        {
                            _logger.LogWarning("RSU: {Name} has no point geometry", entity.Name);
                            continue;
                        }

                        var targetEntity = entity.ToTargetEntity();
                        if (targetEntity is not {IsTargetValid: true, IsIntersectionValid: true}) continue;
                        var point = new GeoJsonPointFeature()
                        {
                            Coordinates = new []
                                {entity.Geometry.Point.Coordinates[0], entity.Geometry.Point.Coordinates[1]}
                        };
                        var buffer = point.CreateBufferInMiles(1);
                        targetEntity.Location = point.Coordinates;
                        if (buffer.Coordinates is null)
                        {
                            _logger.LogWarning("RSU: {Name} has no buffer geometry", entity.Name);
                            continue;
                        }
                        
                        targetEntity.Region = buffer.Coordinates;
                        rsus.Add(targetEntity);
                    }
                }
                else
                {
                    _logger.LogWarning("Request {Id} for {ItisCode} is missing longitude and latitude", request.Id, request.ItisCode);
                }

                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }

        return rsus;
    }

    private static Guid GetIntersectionId(EntityNode entity)
    {
        var id = Guid.Empty;
        
        if (entity.Geometry.Point?.Properties?.Intersection is not null)
        {
            id = entity.Geometry.Point.Properties.Intersection.Value;
        }
        else if (entity.Geometry.LineString is not null && entity.Geometry.LineString.Properties?.Intersection is not null)
        {
            id = entity.Geometry.LineString.Properties.Intersection.Value;
        }
        else if (entity.Geometry.Polygon?.Properties?.Intersection is not null)
        {
            id = entity.Geometry.Polygon.Properties.Intersection.Value;
        }

        return id;
    }

    public async Task<IEnumerable<TargetEntity>> GetTargetEntities(IEnumerable<Guid> ids,
        CancellationToken cancellationToken)
    {
        var result = new List<TargetEntity>();
        var intersections = await _entityConfigurationService.GetByIdsAsync(ids, cancellationToken);
        foreach (var intersection in intersections.ToArray())
        {
            var rsu = intersection.Children.FirstOrDefault(e => e.Type.Id == RsuTypeId.Id);
            if (rsu is not null)
            {
                result.Add(new TargetEntity()
                {
                    IntersectionId = intersection.Id,
                    IntersectionName = intersection.Name,
                    IsIntersectionValid = true,
                    TargetId = rsu.Id,
                    TargetName = rsu.Name,
                    IsTargetValid = true
                });
            }
            else
            {
                result.Add(new TargetEntity()
                {
                    IntersectionId = intersection.Id,
                    IntersectionName = intersection.Name,
                    IsIntersectionValid = true,
                    TargetId = Guid.Empty,
                    IsTargetValid = false
                });
            }
        }
        return result;
    }
    
    private async Task<TargetEntity> GetTargetEntity(Guid target, CancellationToken cancellationToken)
    {
        var result = new TargetEntity();
        if (target == Guid.Empty)
        {
            return result;
        }
                
        var entities = (await _entityConfigurationService.GetIntersectionByIdAsync(target, cancellationToken)).ToArray();
        if (!entities.Any())
        {
            result.IntersectionId = target;
            result.IsIntersectionValid = false;
            return result;
        }
        var intersection = entities.FirstOrDefault(e => e.Type.Id == IntersectionTypeId.Id);
        var rsu = entities.FirstOrDefault(e => e.Type.Id == RsuTypeId.Id);
        if (rsu is not null && intersection is not null)
        {
            var point = new GeoJsonPointFeature() { Coordinates = rsu.Geometry.Point?.Coordinates ?? new double[]{ 0, 0 }};
            var buffer = point.CreateBufferInMiles(1);
            result.Location = point.Coordinates;
            result.Region = buffer.Coordinates!;
            result.IntersectionId = intersection.Id;
            result.IntersectionName = intersection.Name;
            result.IsIntersectionValid = true;
            result.TargetId = rsu.Id;
            result.TargetName = rsu.Name;
            result.IsTargetValid = true;
        }
        else if (intersection is not null && rsu is null)
        {
            result.IntersectionId = target;
            result.IntersectionName = intersection.Name;
            result.IsIntersectionValid = true;
            result.TargetId = Guid.Empty;
            result.IsTargetValid = false;
        }
        else
        {
            result.TargetId = target;
            result.IsIntersectionValid = false;
            result.IsTargetValid = false;
        }

        return result;
    }
}

public static class TargetEntityExtensions
{
    public static TargetEntity ToTargetEntity(this EntityNode entity, string parentName = "")
    {
        return new TargetEntity()
        {
            IntersectionId = entity.Parent,
            IntersectionName = parentName,
            IsIntersectionValid = true,
            TargetId = entity.Id,
            TargetName = entity.Name,
            IsTargetValid = true
        };
    }
}
