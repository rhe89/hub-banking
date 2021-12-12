using Hub.Shared.Storage.Repository;
using Hub.Shared.Web.BlazorServer;
using Microsoft.Extensions.DependencyInjection;
using Sbanken.Data;
using Sbanken.Data.AutoMapper;
using Sbanken.Providers;
using Sbanken.Services;

var builder = BlazorServerBuilder.CreateWebApplicationBuilder(args);

builder.Services.AddDatabase<SbankenDbContext>(builder.Configuration, "SQL_DB_SBANKEN", "Sbanken.Data");
builder.Services.AddTransient<ITransactionService, TransactionService>();
builder.Services.AddTransient<ITransactionProvider, TransactionProvider>();
            
builder.Services.AddAutoMapper(c =>
{
    c.AddSbankenProfiles();
});

var app = builder.BuildApp();

app.Run();