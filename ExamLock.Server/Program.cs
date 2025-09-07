using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var allowedDevices = new ConcurrentDictionary<string, bool>();
var users = new ConcurrentDictionary<string, string>();
users["student1"] = "password";

var adminKeys = new ConcurrentDictionary<string, bool>();
adminKeys["123456789"] = true; // example one-time key

var logs = new ConcurrentDictionary<string, List<string>>();

app.MapPost("/register", (DeviceRegistration reg) =>
{
    if (allowedDevices.ContainsKey(reg.Hwid))
    {
        return Results.BadRequest("Device already registered.");
    }
    if (!adminKeys.TryRemove(reg.AdminKey, out _))
    {
        return Results.BadRequest("Invalid admin key.");
    }
    allowedDevices[reg.Hwid] = true;
    return Results.Ok();
});

app.MapPost("/login", (LoginRequest login) =>
{
    if (!allowedDevices.ContainsKey(login.Hwid))
    {
        return Results.BadRequest("Device not registered.");
    }
    if (users.TryGetValue(login.Username, out var pass) && pass == login.Password)
    {
        return Results.Ok(new { Token = Guid.NewGuid().ToString() });
    }
    return Results.Unauthorized();
});

app.MapGet("/questions", () =>
{
    var questions = new[]
    {
        "What is 2+2?",
        "What is the capital of France?"
    };
    return Results.Ok(questions);
});

app.MapPost("/log", (LogEntry entry) =>
{
    var userLogs = logs.GetOrAdd(entry.Username, _ => new List<string>());
    userLogs.Add($"{DateTime.UtcNow:o} - {entry.Message}");
    return Results.Ok();
});

app.Run("http://0.0.0.0:5000");

record DeviceRegistration(string Hwid, string AdminKey);
record LoginRequest(string Hwid, string Username, string Password);
record LogEntry(string Username, string Message);
