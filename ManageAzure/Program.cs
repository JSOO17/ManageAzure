using ManageAzure.Config;
using ManageAzure.Interfaces;
using ManageAzure.Listeners;
using ManageAzure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<ServiceBusOptions>(builder.Configuration.GetSection("AzureServiceBus"));
builder.Services.Configure<AzureBlobOptions>(builder.Configuration.GetSection("AzureBlobStorage"));
builder.Services.Configure<AzureDevOpsOptions>(builder.Configuration.GetSection("AzureDevops"));
builder.Services.AddHostedService<ServiceBusListener>();
builder.Services.AddTransient<IAzureDevOpsRepository, AzureDevopsRepository>();
builder.Services.AddTransient<IFilesRepository, AzureBlobRepository>();


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
