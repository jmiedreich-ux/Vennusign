using Vennu.Data;
using Vennu.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddVennuData();

var connectionString = builder.Configuration.GetConnectionString("VennuDatabase")
    ?? throw new InvalidOperationException("Connection string 'VennuDatabase' is required.");

Vennu.Data.DatabaseMigrator.Run(connectionString);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapGet("/", () => Results.Ok(new { status = "ok", service = "Vennu.Api" }));

app.Run();
