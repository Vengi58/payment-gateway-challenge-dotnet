using PaymentGateway.Api;


var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);
startup.ConfgigureLogging(builder.Logging);

var app = builder.Build();

startup.Configure(app, builder.Environment);
