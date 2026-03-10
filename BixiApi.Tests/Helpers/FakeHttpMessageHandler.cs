using System.Net;
using System.Text;

namespace BixiApi.Tests.Helpers;

/// <summary>
/// A test-only HTTP handler that returns preset JSON responses without making
/// any real network calls.
///
/// How it works: you provide a dictionary of URL → JSON string. When BixiService
/// calls HttpClient.GetFromJsonAsync(url), this handler intercepts it and returns
/// the matching JSON string instead of hitting the network.
///
/// This lets us test BixiService's joining, filtering and sorting logic with
/// fully controlled input data, independent of the real BIXI API.
/// </summary>
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, string> _responsesByUrl;

    public FakeHttpMessageHandler(Dictionary<string, string> responsesByUrl)
    {
        _responsesByUrl = responsesByUrl;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri!.ToString();

        if (_responsesByUrl.TryGetValue(url, out var json))
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }

        // Explicit 404 for unmapped URLs makes test failures easy to diagnose.
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent($"No fake response configured for URL: {url}")
        });
    }
}
