using Microsoft.EntityFrameworkCore;
using MiniCloudIDE_Backend.Data;
using MiniCloudIDE_Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev",
        policy => policy.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

builder.Services.AddScoped<IScriptHistoryService, ScriptHistoryService>();
builder.Services.AddSingleton<IPythonExecutionService, PythonExecutionService>();
builder.Services.AddHostedService<PythonWorkerHostedService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

var app = builder.Build();

app.UseCors("AllowReactDev");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();