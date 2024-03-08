// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Econolite.Ode.Authorization;
using Econolite.Ode.Messaging.Elements;
using Econolite.Ode.Models.Entities;
using Econolite.Ode.Models.Entities.Types;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Econolite.Ode.Services.TimService;

public class EntityConfigurationService : IEntityConfigurationService
{
    private const int MILLISECONDS_DELAY_AFTER_ADD = 50;
    private const int MILLISECONDS_ABSOLUTE_EXPIRATION = 750;
    private readonly ITokenHandler _tokenHandler;
    private readonly HttpClient _client;
    private readonly string _url;
    private readonly IMemoryCache _cache;
    
    public EntityConfigurationService(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        HttpClient client)
    {
        _tokenHandler = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ITokenHandler>();;
        _client = client;
        _url = configuration.GetValue("Services:Configuration", "http://localhost:5138")!;
        _cache = serviceProvider.GetRequiredService<IMemoryCache>();
    }
    
    public async Task<IEnumerable<EntityNode>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var url = $"{_url}/entities/?ids={string.Join("&ids=",ids.ToArray())}";
        var entities = await GetOrCreateEntityNodesCacheAsync(url, cancellationToken);
        return entities ?? Array.Empty<EntityNode>();
    }
    
    public async Task<EntityNode?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var url = $"{_url}/entities/{id.ToString()}";
        var entity = await GetOrCreateEntityNodeCacheAsync(url, cancellationToken);
        return entity;
    }
    
    public async Task<IEnumerable<EntityNode>> GetNodesByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        var url = $"{_url}/entities/types/{type}";
        var signals = await GetOrCreateEntityNodesCacheAsync(url, cancellationToken);
        return signals ?? Array.Empty<EntityNode>();
    }
    
    public async Task<IEnumerable<EntityNode>> GetIntersectionByIdAsync(Guid intersectionId, CancellationToken cancellationToken = default)
    {
        var url = $"{_url}/entities/intersection/{intersectionId.ToString()}";
        var entities = await GetOrCreateEntityNodesCacheAsync(url, cancellationToken);
        return entities ?? Array.Empty<EntityNode>();
    }

    public async Task<IEnumerable<EntityNode>> QueryIntersectionsWithinRadiusDistanceAsync(GeoJsonPointFeature value,
        int miles, CancellationToken cancellationToken = default)
    {
        var url = $"{_url}/entities/intersections/query/radius/{miles}";
        var json = JsonContent.Create(value, new MediaTypeHeaderValue("application/json"), JsonPayloadSerializerOptions.Options);
        var signals = await GetOrCreatePostEntityNodesCacheAsync(url, json, cancellationToken);
        return signals?.ToArray() ?? Array.Empty<EntityNode>();;
    }
    
    public async Task<IEnumerable<EntityNode>> QueryIntersectingByTypeAsync(GeoJsonLineStringFeature value,
        string type, CancellationToken cancellationToken = default)
    {
        var url = $"{_url}/entities/query/{type}";
        var json = JsonContent.Create(value, new MediaTypeHeaderValue("application/json"), JsonPayloadSerializerOptions.Options);
        var entities = await GetOrCreatePostEntityNodesCacheAsync(url, json, cancellationToken);
        return entities?.ToArray() ?? Array.Empty<EntityNode>();
    }
    
    public async Task<IEnumerable<EntityNode>> QueryIntersectingGeoFencesAsync(GeoJsonPointFeature value, CancellationToken cancellationToken = default)
    {
        var url = $"{_url}/entities/query/geofence";
        _client.DefaultRequestHeaders.Authorization = await _tokenHandler.GetAuthHeaderAsync(cancellationToken);
        var json = JsonContent.Create(value, new MediaTypeHeaderValue("application/json"), JsonPayloadSerializerOptions.Options);
        var response = await _client.PostAsync(url, json, cancellationToken);
        var content = response.Content.ReadAsStringAsync();
        var entities = JsonSerializer.Deserialize<IEnumerable<EntityNode>>(content.Result, JsonPayloadSerializerOptions.Options);
        return entities?.ToArray() ?? Array.Empty<EntityNode>();
    }
    
    public async Task<IEnumerable<EntityNode>> QueryIntersectingGeoFencesByTypeAsync(GeoJsonPointFeature value, string type, CancellationToken cancellationToken = default)
    {
        var url = $"{_url}/entities/query/geofence/{type}";
        var json = JsonContent.Create(value, new MediaTypeHeaderValue("application/json"), JsonPayloadSerializerOptions.Options);
        var entities = await GetOrCreatePostEntityNodesCacheAsync(url, json, cancellationToken);
        return entities?.ToArray() ?? Array.Empty<EntityNode>();
    }

    public async Task<IEnumerable<EntityNode>> QueryIntersectingApproachesAsync(GeoJsonLineStringFeature value, CancellationToken cancellationToken = default)
    {
        var approaches = (await QueryIntersectingByTypeAsync(value, ApproachTypeId.Name, cancellationToken));
        return approaches?.ToArray() ?? Array.Empty<EntityNode>();
    }

    private async Task<EntityNode?> GetOrCreateEntityNodeCacheAsync(string url,
        CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(url, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(MILLISECONDS_ABSOLUTE_EXPIRATION);
            await Task.Delay(MILLISECONDS_DELAY_AFTER_ADD, cancellationToken);
            return await GetEntityNodeAsync(url, cancellationToken);
        });
    }
    
    private async Task<IEnumerable<EntityNode>?> GetOrCreateEntityNodesCacheAsync(string url,
        CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(url, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(MILLISECONDS_ABSOLUTE_EXPIRATION);
            await Task.Delay(MILLISECONDS_DELAY_AFTER_ADD, cancellationToken);
            return await GetEntityNodesAsync(url, cancellationToken);
        });
    }
    
    private async Task<EntityNode?> GetEntityNodeAsync(string url, CancellationToken cancellationToken = default)
    {
     _client.DefaultRequestHeaders.Authorization = await _tokenHandler.GetAuthHeaderAsync(cancellationToken);
     var response = await _client.GetAsync(url, cancellationToken);
     var content = await response.Content.ReadAsStringAsync(cancellationToken);
     var entity = JsonSerializer.Deserialize<EntityNode>(content, JsonPayloadSerializerOptions.Options);
     return entity;
    }
    
    private async Task<IEnumerable<EntityNode>> GetEntityNodesAsync(string url, CancellationToken cancellationToken = default)
    {
        _client.DefaultRequestHeaders.Authorization = await _tokenHandler.GetAuthHeaderAsync(cancellationToken);
        var response = await _client.GetAsync(url, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var entities = JsonSerializer.Deserialize<IEnumerable<EntityNode>>(content, JsonPayloadSerializerOptions.Options);
        return entities ?? Array.Empty<EntityNode>();
    }
    
    private async Task<EntityNode?> GetOrCreatePostEntityNodeCacheAsync(string url, JsonContent json, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(url, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(MILLISECONDS_ABSOLUTE_EXPIRATION);
            await Task.Delay(MILLISECONDS_DELAY_AFTER_ADD, cancellationToken);
            return await PostEntityNodeAsync(url, json, cancellationToken);
        });
    }
    
    private async Task<EntityNode?> PostEntityNodeAsync(string url, JsonContent json, CancellationToken cancellationToken = default)
    {
        _client.DefaultRequestHeaders.Authorization = await _tokenHandler.GetAuthHeaderAsync(cancellationToken);
        var response = await _client.PostAsync(url, json, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var entity = JsonSerializer.Deserialize<EntityNode>(content, JsonPayloadSerializerOptions.Options);
        return entity;
    }
    
    private async Task<IEnumerable<EntityNode>?> GetOrCreatePostEntityNodesCacheAsync(string url, JsonContent json, CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrCreateAsync(url, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(MILLISECONDS_ABSOLUTE_EXPIRATION);
            await Task.Delay(MILLISECONDS_DELAY_AFTER_ADD, cancellationToken);
            return await PostEntityNodesAsync(url, json, cancellationToken);
        });
    }
    
    private async Task<IEnumerable<EntityNode>> PostEntityNodesAsync(string url, JsonContent json, CancellationToken cancellationToken = default)
    {
        _client.DefaultRequestHeaders.Authorization = await _tokenHandler.GetAuthHeaderAsync(cancellationToken);
        var response = await _client.PostAsync(url, json, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<EntityNode>();
        }
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var entities = JsonSerializer.Deserialize<IEnumerable<EntityNode>>(content, JsonPayloadSerializerOptions.Options);
        return entities ?? Array.Empty<EntityNode>();
    }
    
    public async Task<IEnumerable<EntityNode>> QueryRadiusAsync(GeoJsonPointFeature value,
        string miles, CancellationToken cancellationToken = default)
    {
        var url = $"{_url}/entities/intersections/query/radius/{miles}";
        var json = JsonContent.Create(value, new MediaTypeHeaderValue("application/json"), JsonPayloadSerializerOptions.Options);
        var entities = await GetOrCreatePostEntityNodesCacheAsync(url, json, cancellationToken);
        return entities?.ToArray() ?? Array.Empty<EntityNode>();
    }
    
    public async Task<IEnumerable<EntityNode>> QueryDownstreamAsync(GeoJsonPointFeature value,
        string intersections, CancellationToken cancellationToken = default)
    {
        var url = $"{_url}/entities/downstream/{intersections}";
        var json = JsonContent.Create(value, new MediaTypeHeaderValue("application/json"), JsonPayloadSerializerOptions.Options);
        var entities = await GetOrCreatePostEntityNodesCacheAsync(url, json, cancellationToken);
        return entities?.ToArray() ?? Array.Empty<EntityNode>();
    }
    
    public async Task<IEnumerable<EntityNode>> QueryUpstreamAsync(GeoJsonPointFeature value,
        string intersections, CancellationToken cancellationToken = default)
    {
        var url = $"{_url}/entities/upstream/{intersections}";
        var json = JsonContent.Create(value, new MediaTypeHeaderValue("application/json"), JsonPayloadSerializerOptions.Options);
        var entities = await GetOrCreatePostEntityNodesCacheAsync(url, json, cancellationToken);
        return entities?.ToArray() ?? Array.Empty<EntityNode>();
    }
}

public interface IEntityConfigurationService
{
    Task<IEnumerable<EntityNode>> GetNodesByTypeAsync(string type, CancellationToken cancellationToken = default);
    Task<IEnumerable<EntityNode>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<EntityNode?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<EntityNode>> GetIntersectionByIdAsync(Guid intersectionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EntityNode>> QueryIntersectionsWithinRadiusDistanceAsync(GeoJsonPointFeature value, int miles, CancellationToken cancellationToken = default);
    Task<IEnumerable<EntityNode>> QueryIntersectingByTypeAsync(GeoJsonLineStringFeature value, string type, CancellationToken cancellationToken = default);
    Task<IEnumerable<EntityNode>> QueryIntersectingGeoFencesAsync(GeoJsonPointFeature value, CancellationToken cancellationToken = default);
    Task<IEnumerable<EntityNode>> QueryIntersectingGeoFencesByTypeAsync(GeoJsonPointFeature value, string type, CancellationToken cancellationToken = default);
    Task<IEnumerable<EntityNode>> QueryIntersectingApproachesAsync(GeoJsonLineStringFeature route, CancellationToken cancellationToken = default);
    Task<IEnumerable<EntityNode>> QueryRadiusAsync(GeoJsonPointFeature value, string miles, CancellationToken cancellationToken = default);
    Task<IEnumerable<EntityNode>> QueryDownstreamAsync(GeoJsonPointFeature value, string intersections, CancellationToken cancellationToken = default);
    Task<IEnumerable<EntityNode>> QueryUpstreamAsync(GeoJsonPointFeature value, string intersections, CancellationToken cancellationToken = default);
}
