using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using PaymentGateway.Api.Middleware;
using PaymentGateway.Application.Behaviors;
using PaymentGateway.Application.Encryption;
using PaymentGateway.Application.Repository;
using PaymentGateway.Application.Services.BankSimulator;
using PaymentGateway.Persistance;
using PaymentGateway.Persistance.Repository;
using PaymentGateway.Services.BankSimulator;
using PaymentGateway.Services.Encryption;

namespace PaymentGateway.Api
{
    public class Startup
    {

        string bankSimulatorAddress = Environment.GetEnvironmentVariable("BANK_SIMULATOR_ADDRESS");
        bool generateTestData = "true".Equals(Environment.GetEnvironmentVariable("INIT_TEST_DATA"));
        
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services) {

            // Add services to the container.
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(PaymentGateway.Application.AssemblyReference.Assembly));

            services.AddControllers();
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddValidatorsFromAssembly(Application.AssemblyReference.Assembly);


            //Test doubles PaymentRepository
            //services.AddSingleton<IPaymentRepository, PaymentsRepository>();

            //Postgres db context
            //services.AddSingleton<PaymentsDbContext>(ctx => new(
            //    new DbContextOptionsBuilder<PaymentsDbContext>().UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")).Options));

            //Inemmory db context
            services.AddSingleton<PaymentsDbContext>(ctx => new(
                new DbContextOptionsBuilder<PaymentsDbContext>().UseInMemoryDatabase("PaymentsDb").Options));

            services.AddSingleton<IPaymentRepository, PaymentsEfRepository>();
            services.AddSingleton<IBankSimulator>(b => new BankSimulator(bankSimulatorAddress));

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddSingleton<ICryptoService, RsaCryptoService>();

            //app.Services.GetService<PaymentsDbContext>().Database.Migrate();
        }

        public void Configure(WebApplication app, IWebHostEnvironment env) {

            if (generateTestData) DataGenerator.Initialize(app.Services);

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
        }

        public void ConfgigureLogging(ILoggingBuilder loggingBuilder)
        {

            loggingBuilder.ClearProviders();
            loggingBuilder.AddConsole();
        }
    }
}
