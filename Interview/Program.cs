using Interview.Configuration;
using Interview.Services.Interfaces;
using Interview.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IContentstackService, ContentstackService>();
builder.Services.Configure<ContentstackConfiguration>(builder.Configuration.GetSection(ContentstackConfiguration.SectionName));

var app = builder.Build();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
