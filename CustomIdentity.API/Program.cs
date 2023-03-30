using CustomIdentity.Core.Middlewares.Jwt;
using CustomIdentity.Core.Repositories.User;
using CustomIdentity.Core.Services.Authentication;
using CustomIdentity.Core.Services.EmailService;
using CustomIdentity.Core.TokenGenerator;
using CustomIdentity.Core.Utility;
using CustomIdentity.Domain;
using CustomIdentity.Domain.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using Microsoft.Extensions.Configuration;


var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient<IUtilityClass, UtilityClass>();
builder.Services.AddTransient<IEmailService, SmtpEmailService>();
builder.Services.AddTransient<IEmailMessage, ActivationEmailMessage>();
builder.Services.AddTransient<ITokenGenerator, TokenGenerator>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<JwtMiddleware>();

// Add Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
	options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
	{
		Description = "Standard Auth header using the Bearer scheme (\"Bearer {token}\")",
		In = ParameterLocation.Header,
		Name = "Authorization",
		Type = SecuritySchemeType.ApiKey
	});
	options.OperationFilter<SecurityRequirementsOperationFilter>();
});

// Add authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("Jwt:Key").Value)),
			ValidateIssuer = false,
			ValidateAudience = false
		};
	});

// Add database context
builder.Services.AddDbContext<CustomIdentityDb>(options =>
	options.UseNpgsql("User ID=postgres;Password=\"haslo1234\";Server=localhost;Port=5432;Database=CustomIdentityDb;"));
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add authorization
builder.Services.AddAuthorization();

// Build the app
var app = builder.Build();

// Use Swagger and SwaggerUI in development environment
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Use HTTPS redirection
app.UseHttpsRedirection();
// Use authentication
app.UseAuthentication();

// Use routing
app.UseRouting();

// Use authorization
app.UseAuthorization();

// Use JwtMiddleware
app.UseMiddleware<JwtMiddleware>();


// Map controllers
app.MapControllers();

// Run the app
app.Run();
