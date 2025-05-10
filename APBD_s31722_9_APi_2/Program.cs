using APBD_s31722_8_API.Datalayer;
using APBD_s31722_9_APi_2.Exceptions;
using APBD_s31722_9_APi_2.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); 
builder.Services.AddScoped<DbClient>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
var app = builder.Build();

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseRouting();
app.MapControllers();
app.Run();