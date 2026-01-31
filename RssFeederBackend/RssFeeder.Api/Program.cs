using RssFeeder.Application.Interfaces;
using RssFeeder.Application.Services;
using RssFeeder.Domain.Interfaces;
using RssFeeder.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);
var baseFilePath = builder.Configuration.GetValue<string>("BaseFilePath");


builder.Services.AddSwaggerGen();
//builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("Redis:Configuration") ?? "localhost:6379";
});

builder.Services.AddScoped<IResponseCaching, ResponseCaching>();
builder.Services.AddScoped<IFeedService, FeedService>();
builder.Services.AddScoped<RssSyndicationService>();

builder.Services.AddSingleton<IFeedRepository, FeedRepository>(serviceProvider =>
new FeedRepository(baseFilePath));
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RssFeeder API v1");
        c.RoutePrefix = "swagger";
    });

}


app.UseHttpsRedirection();
app.MapControllers();

app.Run();

