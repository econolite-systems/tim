// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using System.Collections.Immutable;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Econolite.Ode.Api.JpoOdeTim;

public class JpoOdeTimApiException : Exception
{
    public JpoOdeTimApiException(string message)
        : base(message)
    {
    }
}

public class JpoOdeTimClient
{
    private readonly HttpClient _client;
    private readonly Uri _baseUri;

    public JpoOdeTimClient(Uri baseUri)
    {
        _client = new HttpClient();
        _baseUri = baseUri;
    }

    private Uri MakeUri(
        IReadOnlyCollection<string> path,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>>? parameters = null
    )
    {
        var builder = new UriBuilder(_baseUri)
        {
            Path = string.Join('/', path)
        };

        if (parameters is not null)
        {
            builder.Query = string.Join(
                '&',
                parameters.SelectMany(kv => kv.Value.Select(val => $"{kv.Key}={val}")));
        }

        return builder.Uri;
    }

    private static async Task<Exception> GetApiException(HttpResponseMessage response)
    {
        var errorResponse = await response.Content.ReadFromJsonAsync<JpoOdeErrorResponse>();
        Debug.Assert(errorResponse is not null);

        return new JpoOdeTimApiException(errorResponse.Error);
    }

    private static async Task<JpoOdePostPutResponse> ProcessPostPutOkResponse(HttpResponseMessage response)
    {
        Debug.Assert(response.StatusCode == HttpStatusCode.OK);
        var res = await response.Content.ReadFromJsonAsync<JpoOdePostPutResponse>();
        Debug.Assert(res is not null);

        return res;
    }

    private static async Task<JpoOdePostPutResponse> ProcessPostPutResponse(HttpResponseMessage response)
    {
        return response.StatusCode switch
        {
            HttpStatusCode.OK => await ProcessPostPutOkResponse(response),
            HttpStatusCode.BadRequest => throw await GetApiException(response),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public async Task<JpoOdePostPutResponse> Create(JpoOdeTimPost post)
    {
        var uri = MakeUri(new[] { "tim" });

        var response = await _client.PostAsJsonAsync(uri, post);
        return await ProcessPostPutResponse(response);
    }

    public async Task<JpoOdePostPutResponse> Update(JpoOdeTimPut put)
    {
        var uri = MakeUri(new[] { "tim" });

        var response = await _client.PutAsJsonAsync(uri, put);
        return await ProcessPostPutResponse(response);
    }

    private static async Task<JpoOdeDeleteResponse> ProcessDeleteOkResponse(HttpResponseMessage response)
    {
        Debug.Assert(response.StatusCode == HttpStatusCode.OK);
        var res = await response.Content.ReadFromJsonAsync<JpoOdeDeleteRawResponse>();
        Debug.Assert(res is not null);

        return new JpoOdeDeleteResponse(int.Parse(res.DeletedMsg));
    }

    private static async Task<JpoOdeDeleteResponse> ProcessDeleteResponse(HttpResponseMessage response)
    {
        return response.StatusCode switch
        {
            HttpStatusCode.OK => await ProcessDeleteOkResponse(response),
            HttpStatusCode.RequestTimeout => throw await GetApiException(response),
            HttpStatusCode.InternalServerError => throw await GetApiException(response),
            HttpStatusCode.BadRequest => throw await GetApiException(response),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static async Task<JpoOdeQueryResponse> ProcessQueryOkResponse(HttpResponseMessage response)
    {
        Debug.Assert(response.StatusCode == HttpStatusCode.OK);
        var res = await response.Content.ReadFromJsonAsync<JpoOdeQueryRawResponse>();
        Debug.Assert(res is not null);

        var indices = JsonSerializer.Deserialize<List<int>>(res.IndicesSet);
        Debug.Assert(indices is not null);

        return new JpoOdeQueryResponse(indices.ToImmutableHashSet());
    }

    public async Task<JpoOdeDeleteResponse> Delete(int index)
    {
        var uri = MakeUri(new[] { "tim" },
            parameters: new Dictionary<string, IReadOnlyCollection<string>>
            {
                ["index"] = new[] { index.ToString() },
            });

        var response = await _client.DeleteAsync(uri);
        return await ProcessDeleteResponse(response);
    }

    private static async Task<JpoOdeQueryResponse> ProcessQueryResponse(HttpResponseMessage response)
    {
        return response.StatusCode switch
        {
            HttpStatusCode.OK => await ProcessQueryOkResponse(response),
            HttpStatusCode.InternalServerError => throw await GetApiException(response),
            HttpStatusCode.BadRequest => throw await GetApiException(response),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public async Task<JpoOdeQueryResponse> Query(JpoOdeTimQuery query)
    {
        var uri = MakeUri(new[] { "tim", "query" });

        var response = await _client.PostAsJsonAsync(uri, query);
        return await ProcessQueryResponse(response);
    }
}
