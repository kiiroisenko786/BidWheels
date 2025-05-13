using AuctionService.Data;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using AuctionService.Consumers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AuctionService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(opt => 
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
// Add AutoMapper, and look for any classes that derive from the profile class
// and register the mappings in memory for Auto Mapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Set up MassTransit
builder.Services.AddMassTransit(x =>
{
    // Add outbox to masstransit
    x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10);

        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        // Configure the RabbitMQ host so that it be accessed by Dockerized svc
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Authority is identity server
        // Tells the server who the token was issued by (identity service)
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";
    });

// This needs to be add scoped because we want this to be scoped the same as the dbcontext
// we are injecting our dbcontext, and the dbcontext service lifetime is scoped
// so we need the same for the repository that is making use of the dbcontext
// effectively scoped to the http request
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();

builder.Services.AddGrpc();

var app = builder.Build();

// Authentication has to come before authorization ALWAYS
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<GrpcAuctionService>();

try
{
    DbInitializer.InitDb(app);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

app.Run();
