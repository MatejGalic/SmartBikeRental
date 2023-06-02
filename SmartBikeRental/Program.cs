using SmartBikeRental.Helpers;
using SmartBikeRental.Hubs;
using SmartBikeRental.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IMqttClientWrapper, MqttClientWrapper>();

// Read AllowedOrigins from appsettings.json
var corsSettings = builder.Configuration.GetSection("CorsSettings").GetSection("AllowedOrigins");
var allowedOrigins = corsSettings.Get<string[]>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowSpecificOrigins");

// Configure the HTTP request pipeline. https://localhost:7226/swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
app.MapHub<BikeRentHub>("hub");



var mqttClient = app.Services.GetRequiredService<IMqttClientWrapper>();
mqttClient.Connect(Guid.NewGuid().ToString());
mqttClient.SubscribeToTopic("all");

app.Lifetime.ApplicationStopping.Register(() =>
{
    mqttClient.Disconnect();
});


app.MapFallbackToFile("index.html");

app.Run();
