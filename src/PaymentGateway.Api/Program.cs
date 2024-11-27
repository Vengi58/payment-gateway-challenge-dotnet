using FluentValidation;

using MediatR;

using PaymentGateway.Api.Middleware;
using PaymentGateway.Application.Repository;
using PaymentGateway.Services.BankSimulator;
using PaymentGateway.Services.Encryption;
using PaymentGateway.Application.Behaviors;
using PaymentGateway.Application.Services.BankSimulator;
using PaymentGateway.Application.Encryption;
using PaymentGateway.Persistance.Repository;
using PaymentGateway.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(PaymentGateway.Application.AssemblyReference.Assembly));

builder.Services.AddControllers();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddValidatorsFromAssembly(PaymentGateway.Application.AssemblyReference.Assembly);
//builder.Services.AddSingleton<IPaymentRepository, PaymentsRepository>();
//builder.Services.AddDbContext<PaymentsDbContext>(options =>
//        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<PaymentsDbContext>(ctx => new(
    new DbContextOptionsBuilder<PaymentsDbContext>().UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")).Options));
//builder.Services.AddSingleton<PaymentsDbContext>(ctx => new(new DbContextOptionsBuilder<PaymentsDbContext>().UseInMemoryDatabase("PaymentsDb").Options));
builder.Services.AddSingleton<IPaymentRepository, PaymentsInMemoryRepository>();
builder.Services.AddSingleton<IBankSimulator>(b => new BankSimulator(Environment.GetEnvironmentVariable("BANK_SIMULATOR_ADDRESS")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ICryptoService, RsaCryptoService>();

var app = builder.Build();
//app.Services.GetService<PaymentsDbContext>().Database.Migrate();
if("true".Equals(Environment.GetEnvironmentVariable("INIT_TEST_DATA"))) DataGenerator.Initialize(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
