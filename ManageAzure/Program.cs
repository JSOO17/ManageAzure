using ManageAzure.Config;
using ManageAzure.DataAccess;
using ManageAzure.DataAccess.Models;
using ManageAzure.Interfaces;
using ManageAzure.Listeners;
using ManageAzure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<ServiceBusOptions>(builder.Configuration.GetSection("AzureServiceBus"));
builder.Services.Configure<AzureBlobOptions>(builder.Configuration.GetSection("AzureBStorage"));
builder.Services.Configure<AzureDevOpsOptions>(builder.Configuration.GetSection("AzureDevops"));
builder.Services.AddHostedService<ServiceBusListener>();
builder.Services.AddTransient<IAzureDevOpsRepository, AzureDevopsRepository>();
builder.Services.AddTransient<IFilesRepository, AzureBlobRepository>();
builder.Services.AddTransient<IApiKeyRepository, ApiKeyRepository>();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<productsContext>(options =>
            options.UseSqlServer(connectionString));
builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(ValidateApiKeyAttribute));
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
