using Iot.Device.Board;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Sannel.House;
using Sannel.House.Sprinklers;
using Sannel.House.Sprinklers.Core;
using Sannel.House.Sprinklers.Core.Hardware;
using Sannel.House.Sprinklers.Core.Schedules;
using Sannel.House.Sprinklers.Infrastructure;
using Sannel.House.Sprinklers.Infrastructure.Hardware;
using Sannel.House.Sprinklers.Infrastructure.Schedules;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile(Path.Combine("app_config", "appsettings.json"), true, true);
builder.Configuration.AddJsonFile(Path.Combine("app_config", $"appsettings.{builder.Environment.EnvironmentName}.json"), true, true);
builder.Host.UseSystemd();

builder.Services.AddApplicationInsightsTelemetry();


builder.Services.AddAuthentication(o =>
{
	o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
	.AddMicrosoftIdentityWebApi(builder.Configuration);

builder.Services.AddAuthorization(o =>
{
	o.AddPolicy(AuthPolicy.ZONE_READERS, p =>
		p.RequireRole(
			Roles.Sprinklers.ZONE_READ,
			Roles.Sprinklers.ZONE_WRITE,
			Roles.ADMIN
		));
	o.AddPolicy(AuthPolicy.ZONE_TRIGGERS, p =>
		p.RequireRole(
			Roles.Sprinklers.ZONE_WRITE,
			Roles.ADMIN
		));
	o.AddPolicy(AuthPolicy.SCHEDULE_READERS, p =>
		p.RequireRole(
			Roles.Sprinklers.SCHEDULE_READ,
			Roles.Sprinklers.SCHEDULE_WRITE,
			Roles.ADMIN
		));
	o.AddPolicy(AuthPolicy.SCHEDULE_SCHEDULERS, p =>
		p.RequireRole(
			Roles.Sprinklers.SCHEDULE_WRITE,
			Roles.ADMIN
		));
});

static bool IsRunningOnRaspberryPi()
{
	const string piPartialProcessorName = "BCM";
	var processorNameFile = "/proc/cpuinfo";

	if (!File.Exists(processorNameFile))
	{
		return false;
	}

	var processorName = File.ReadAllText(processorNameFile);
	return processorName.Contains(piPartialProcessorName);
}

builder.Services.AddApiVersioning(o =>
	{
		o.ReportApiVersions = true;
	})
	.AddMvc()
	.AddApiExplorer(options =>
	{
		// add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
		// note: the specified format code will format the version as "'v'major[.minor][-status]"
		options.GroupNameFormat = "'v'VVV";

		// note: this option is only necessary when versioning by url segment. the SubstitutionFormat
		// can also be used to control the format of the API version in route templates
		options.SubstituteApiVersionInUrl = true;
	});


// Add services to the container.
builder.Services.AddDbContext<SprinklerDbContext>(i => i.UseSqlite("Data Source=schedule.db"));
builder.Services.AddSingleton<Board>(RaspberryPiBoard.Create());
if (IsRunningOnRaspberryPi())
{
	builder.Services.AddSingleton<ISprinklerHardware, OpenSprinklerHardware>();
}
else
{
	builder.Services.AddSingleton<ISprinklerHardware, FakeHardware>();
}
builder.Services.AddTransient<IClaimsTransformation, UserClaimsTransformation>();
builder.Services.AddSingleton<SprinklerService>();
builder.Services.AddTransient<IScheduleRepository, ScheduleRepository>();
builder.Services.AddTransient<ILoggerRepository, LoggerRepository>();
builder.Services.AddHostedService<SprinklerService>(s => s.GetRequiredService<SprinklerService>());
builder.Services.AddHostedService<ScheduleService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
	o.DocumentFilter<DocumentSecurityFilter>();
	o.OperationFilter<SecurityFilter>();
	o.MapType<TimeSpan>(() => new OpenApiSchema { Type = "string", Format = "00:00:00", Reference = null, Nullable = false });
	o.MapType<TimeSpan?>(() => new OpenApiSchema { Type = "string", Format = "00:00:00", Reference = null, Nullable = true });
});


var app = builder.Build();

app.UseDeveloperExceptionPage();

using (var scope = app.Services.CreateScope())
{
	var context = scope.ServiceProvider.GetRequiredService<SprinklerDbContext>();
	await context.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
app.UseSwagger(o =>
{
	o.RouteTemplate = "sprinkler/swagger/{documentName}/swagger.{json|yaml}";
});
app.UseSwaggerUI(o =>
{
	o.RoutePrefix = "sprinkler/swagger";
	o.OAuthAppName("AzureAd");
	o.OAuthClientId(builder.Configuration["AzureAd:ClientId"]);
	o.OAuthScopeSeparator(" ");
});

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();