using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddTransient<EnterpriseRag.Api.Services.IDocumentParserService, EnterpriseRag.Api.Services.PdfParserService>();
builder.Services.AddTransient<EnterpriseRag.Api.Services.IChunkingService, EnterpriseRag.Api.Services.TokenSizeChunkingService>();

// Register Groq configuration
builder.Services.Configure<EnterpriseRag.Api.Config.GroqConfig>(
    builder.Configuration.GetSection(EnterpriseRag.Api.Config.GroqConfig.SectionName));

// Register HuggingFace configuration
builder.Services.Configure<EnterpriseRag.Api.Config.HuggingFaceConfig>(
    builder.Configuration.GetSection(EnterpriseRag.Api.Config.HuggingFaceConfig.SectionName));

// Register ILlmService using Groq
builder.Services.AddSingleton<EnterpriseRag.Api.Services.ILlmService, EnterpriseRag.Api.Services.GroqGenerationService>();

// Register IEmbeddingService using HuggingFace
builder.Services.AddHttpClient<EnterpriseRag.Api.Services.IEmbeddingService, EnterpriseRag.Api.Services.HuggingFaceEmbeddingService>();

// Register MongoDB configuration
builder.Services.Configure<EnterpriseRag.Api.Config.MongoDbConfig>(
    builder.Configuration.GetSection(EnterpriseRag.Api.Config.MongoDbConfig.SectionName));

// Register MongoClient as a Singleton to prevent socket exhaustion
builder.Services.AddSingleton<MongoDB.Driver.IMongoClient>(sp =>
{
    var config = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<EnterpriseRag.Api.Config.MongoDbConfig>>().Value;
    return new MongoDB.Driver.MongoClient(config.ConnectionString);
});

// Register MongoDbContext as a Singleton
builder.Services.AddSingleton<EnterpriseRag.Api.Data.MongoDbContext>();

// Configure CORS for a production-ready API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
