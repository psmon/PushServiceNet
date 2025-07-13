using Akka.Actor;
using Akka.Hosting;
using PushServiceNet.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Akka.NET
builder.Services.AddAkka("PushServiceSystem", configurationBuilder =>
{
    configurationBuilder
        .ConfigureLoggers(setup =>
        {
            setup.LogLevel = Akka.Event.LogLevel.InfoLevel;
        });
});

// Register services
builder.Services.AddSingleton<AkkaService>(provider =>
{
    var actorSystem = provider.GetRequiredService<ActorSystem>();
    return new AkkaService(actorSystem);
});
builder.Services.AddSingleton<TopicService>();
builder.Services.AddSingleton<SSEService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();