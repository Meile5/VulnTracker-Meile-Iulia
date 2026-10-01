using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using VulnTracker.Application.DTOs;
using VulnTracker.Domain.Enums;
using Xunit;

namespace VulnTracker.Tests;

public class FindingsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public FindingsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Then_Get_ReturnsCreatedFinding()
    {
        var request = new CreateFindingRequest
        {
            Title = "SQL injection in login form",
            Description = "Unsanitised username parameter",
            AffectedAsset = "customer-portal",
            Severity = Severity.High,
            CvssScore = 8.1m
        };

        var createResponse = await _client.PostAsJsonAsync("/api/findings", request, _jsonOptions);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<FindingResponse>(_jsonOptions);
        Assert.NotNull(created);
        Assert.Equal(request.Title, created!.Title);

        var getResponse = await _client.GetAsync($"/api/findings/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task Create_WithMissingTitle_ReturnsBadRequest()
    {
        var request = new CreateFindingRequest
        {
            Title = "",
            Description = "desc",
            AffectedAsset = "asset",
            Severity = Severity.Low
        };

        var response = await _client.PostAsJsonAsync("/api/findings", request, _jsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_NonExistentId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/findings/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}