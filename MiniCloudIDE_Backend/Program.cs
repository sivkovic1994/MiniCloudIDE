var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactDev",
        policy => policy.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("AllowReactDev");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();