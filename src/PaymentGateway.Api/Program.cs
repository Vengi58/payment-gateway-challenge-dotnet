using System.Reflection;

using FluentValidation;

using MediatR;

using PaymentGateway.Api.Middleware;
using PaymentGateway.Api.Services.BankSimulator;
using PaymentGateway.Persistance.Repository;
using PaymentGateway.Services.BankSimulator;
using PaymentGateway.Services.Encryption;
using PaymentGateway.Application.Behaviors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(PaymentGateway.Application.AssemblyReference.Assembly));


//builder.Services.AddMediatR(PaymentGateway.Application.AssemblyReference.Assembly);



//builder.Services.AddMediatR(Assembly.GetExecutingAssembly());
builder.Services.AddControllers();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddValidatorsFromAssembly(PaymentGateway.Application.AssemblyReference.Assembly);
builder.Services.AddSingleton<IPaymentRepository, PaymentsRepository>();
builder.Services.AddSingleton<IBankSimulator, BankSimulator>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddSingleton<PaymentsRepository>();

builder.Services.AddSingleton<ICryptoService, RsaCryptoService>();

var app = builder.Build();

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
