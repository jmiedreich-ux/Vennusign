using Vennu.Data;
using Vennu.Data.Extensions;
using Vennu.Api.Hubs;
using Vennu.Api.Notifications;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IScreenUpdateNotifier, SignalRScreenUpdateNotifier>();
builder.Services.AddVennuData();

if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString = builder.Configuration.GetConnectionString("VennuDatabase")
        ?? throw new InvalidOperationException("Connection string 'VennuDatabase' is required.");

    Vennu.Data.DatabaseMigrator.Run(connectionString);
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapHub<VennuHub>("/hubs/vennu");
app.MapGet("/", () => Results.Ok(new { status = "ok", service = "Vennu.Api" }));

app.Run();

public partial class Program;
