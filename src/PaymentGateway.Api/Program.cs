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
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Persistance;

string bankSimulatorAddress = Environment.GetEnvironmentVariable("BANK_SIMULATOR_ADDRESS");
bool generateTestData = "true".Equals(Environment.GetEnvironmentVariable("INIT_TEST_DATA"));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(PaymentGateway.Application.AssemblyReference.Assembly));

builder.Services.AddControllers();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddValidatorsFromAssembly(PaymentGateway.Application.AssemblyReference.Assembly);

//Test doubles PaymentRepository
//builder.Services.AddSingleton<IPaymentRepository, PaymentsRepository>();

//Postgres db context
//builder.Services.AddSingleton<PaymentsDbContext>(ctx => new(
//    new DbContextOptionsBuilder<PaymentsDbContext>().UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")).Options));

//Inemmory db context
builder.Services.AddSingleton<PaymentsDbContext>(ctx => new(
    new DbContextOptionsBuilder<PaymentsDbContext>().UseInMemoryDatabase("PaymentsDb").Options));

builder.Services.AddSingleton<IPaymentRepository, PaymentsInMemoryRepository>();
builder.Services.AddSingleton<IBankSimulator>(b => new BankSimulator(bankSimulatorAddress));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ICryptoService, RsaCryptoService>();

var app = builder.Build();
//app.Services.GetService<PaymentsDbContext>().Database.Migrate();
if(generateTestData) DataGenerator.Initialize(app.Services);

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
