using APBD_s31722_8_API.Datalayer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); 
builder.Services.AddScoped<DbClient>();

var app = builder.Build();

app.UseRouting();
app.MapControllers();
app.Run();