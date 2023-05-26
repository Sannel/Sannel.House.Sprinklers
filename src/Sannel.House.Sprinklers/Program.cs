using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using Iot.Device.Board;
using Microsoft.EntityFrameworkCore;
using Sannel.House.Sprinklers.Core;
using Sannel.House.Sprinklers.Core.Hardware;
using Sannel.House.Sprinklers.Core.Schedules;
using Sannel.House.Sprinklers.Infrastructure;
using Sannel.House.Sprinklers.Infrastructure.Hardware;
using Sannel.House.Sprinklers.Infrastructure.Schedules;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSystemd();

// Add services to the container.
builder.Services.AddDbContext<SprinklerDbContext>(i => i.UseSqlite("Data Source=schedule.db"));
builder.Services.AddSingleton<Board>(RaspberryPiBoard.Create());
builder.Services.AddSingleton<ISprinklerHardware, OpenSprinklerHardware>();
builder.Services.AddSingleton<SprinklerService>();
builder.Services.AddTransient<IScheduleRepository,  ScheduleRepository>();
builder.Services.AddTransient<ILoggerRepository, LoggerRepository>();
builder.Services.AddHostedService<SprinklerService>(s => s.GetRequiredService<SprinklerService>());
builder.Services.AddHostedService<ScheduleService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDeveloperExceptionPage();

using (var scope = app.Services.CreateScope())
{
	var context = scope.ServiceProvider.GetRequiredService<SprinklerDbContext>();
	await context.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
