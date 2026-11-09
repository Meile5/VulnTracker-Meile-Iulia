using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using VulnTracker.Application.Interfaces;

namespace VulnTracker.Infrastructure.Keycloak;

public sealed class KeycloakAdminClient(HttpClient http, IConfiguration cfg) : IKeycloakAdminClient
{
    public static readonly string[] AppRoles = ["analyst", "developer", "pm", "auditor"];

    private static string? _token;
    private static DateTime _expires;
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private string Realm => cfg["Keycloak:Realm"] ?? "vulntracker";
    private string Base => cfg["Keycloak:AdminBaseUrl"]!.TrimEnd('/');
    private string Admin => $"{Base}/admin/realms/{Realm}";

    private async Task<string> TokenAsync(CancellationToken ct)
    {
        await Gate.WaitAsync(ct);
        try
        {
            if (_token is not null && DateTime.UtcNow < _expires) return _token;

            var res = await http.PostAsync(
                $"{Base}/realms/{Realm}/protocol/openid-connect/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = cfg["Authentication:Keycloak:ClientId"]!,
                    ["client_secret"] = cfg["Authentication:Keycloak:ClientSecret"]!,
                }), ct);
            res.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
            _token = doc.RootElement.GetProperty("access_token").GetString();
            _expires = DateTime.UtcNow.AddSeconds(doc.RootElement.GetProperty("expires_in").GetInt32() - 30);
            return _token!;
        }
        finally { Gate.Release(); }
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod m, string url, object? body, CancellationToken ct)
    {
        var req = new HttpRequestMessage(m, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await TokenAsync(ct));
        if (body is not null) req.Content = JsonContent.Create(body);
        return await http.SendAsync(req, ct);
    }

    private async Task<JsonElement> GetJsonAsync(string url, CancellationToken ct)
    {
        var res = await SendAsync(HttpMethod.Get, url, null, ct);
        res.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync(ct));
        return doc.RootElement.Clone();
    }

    public async Task<IReadOnlyList<KeycloakUser>> ListUsersAsync(CancellationToken ct)
    {
        var users = await GetJsonAsync($"{Admin}/users?max=500", ct);

        var byRole = new Dictionary<string, HashSet<string>>();
        foreach (var role in AppRoles)
        {
            var members = await GetJsonAsync($"{Admin}/roles/{role}/users?max=500", ct);
            byRole[role] = members.EnumerateArray().Select(u => u.GetProperty("id").GetString()!).ToHashSet();
        }

        return users.EnumerateArray()
            .Where(u => !u.TryGetProperty("serviceAccountClientId", out _))
            .Where(u => u.GetProperty("enabled").GetBoolean())
            .Select(u =>
            {
                var id = u.GetProperty("id").GetString()!;
                string? S(string n) => u.TryGetProperty(n, out var v) ? v.GetString() : null;
                return new KeycloakUser(Guid.Parse(id), S("username")!, S("email"), S("firstName"), S("lastName"),
                    AppRoles.Where(r => byRole[r].Contains(id)).ToList());
            })
            .OrderBy(u => u.Username)
            .ToList();
    }

    public async Task<bool> UserExistsAsync(Guid id, CancellationToken ct)
    {
        var res = await SendAsync(HttpMethod.Get, $"{Admin}/users/{id}", null, ct);
        return res.IsSuccessStatusCode;
    }

    public async Task SetRolesAsync(Guid userId, IReadOnlyCollection<string> roles, CancellationToken ct)
    {
        var wanted = roles.Where(AppRoles.Contains).ToHashSet();

        var current = (await GetJsonAsync($"{Admin}/users/{userId}/role-mappings/realm", ct))
            .EnumerateArray().Select(r => r.GetProperty("name").GetString()!)
            .Where(AppRoles.Contains).ToHashSet();

        async Task<List<JsonElement>> RepsAsync(IEnumerable<string> names)
        {
            var list = new List<JsonElement>();
            foreach (var n in names) list.Add(await GetJsonAsync($"{Admin}/roles/{n}", ct));
            return list;
        }

        var add = await RepsAsync(wanted.Except(current));
        var remove = await RepsAsync(current.Except(wanted));

        if (add.Count > 0)
            (await SendAsync(HttpMethod.Post, $"{Admin}/users/{userId}/role-mappings/realm", add, ct)).EnsureSuccessStatusCode();
        if (remove.Count > 0)
            (await SendAsync(HttpMethod.Delete, $"{Admin}/users/{userId}/role-mappings/realm", remove, ct)).EnsureSuccessStatusCode();
    }
}
