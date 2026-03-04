using System.Data;
using D.API.Hubs;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("Db")));

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Encoder =
            System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });
    
// builder.Services
//     .AddApplication()
//     .AddInfrastructure();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Services.AddControllers();

builder.Services.AddScoped<DispatchingService>(); 
builder.Services.AddScoped<WorkOrdersService>(); 
builder.Services.AddScoped<WorkCentersService>(); 

builder.Services.AddCors(options => {
    options.AddPolicy("BlazorPolicy", policy => {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); 
    });
});

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseWebSockets();
app.UseCors("BlazorPolicy");
app.MapControllers();

app.MapHub<BroadcastHub>("/dispatchHub");

app.Run();


