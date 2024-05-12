using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ServerORT.DbContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Runtime.ConstrainedExecution;
using ServerORT.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Добавление JWT аутентификации
builder.Services.AddAuthentication(options =>
{
	options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	//options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; 
	// Устанавливаем схему по умолчанию как Cookie
	//options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = "GitHub"; // Добавлено для возможности выбора между JWT и GitHub
})

	.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme) // Добавляем поддержку Cookie

.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
options.TokenValidationParameters = new TokenValidationParameters
{
	ValidateIssuerSigningKey = true,
	IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("zenitsu is the best character in the world")),
	ValidateIssuer = false,
	ValidateAudience = false
};
})
// Добавление аутентификации через GitHub.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme) // Добавляем поддержку Cookie

.AddOAuth("GitHub", options =>
{
	options.ClientId = "c8fb445b6686367d7aec";
	options.ClientSecret = "d8919c506adcd9a782e9b2b0f0faf88fd8963304";
	options.CallbackPath = new PathString("/api/Auth/getUserId");

	options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
	options.TokenEndpoint = "https://github.com/login/oauth/access_token";
	options.UserInformationEndpoint = "https://api.github.com/user";
	options.Scope.Add("read:user");
	options.SaveTokens = true;
	options.Events = new OAuthEvents
	{
		OnRemoteFailure = context =>
		{
			context.HandleResponse();
			context.Response.Redirect("/api/Auth/getUserId"); // Перенаправление на страницу ошибок
			return Task.CompletedTask;
		}
	};
	options.Events = new OAuthEvents
	{
		OnCreatingTicket = async context =>
		{
			Console.WriteLine($"Access Token: {context.AccessToken}");
			var accessToken = context.AccessToken;
			var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();
			var user = JsonDocument.Parse(json);

			// Обработка полученных данных
			var userId = user.RootElement.GetProperty("id").GetInt32();
			var userName = user.RootElement.GetProperty("login").GetString();
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
				new Claim(ClaimTypes.Name, userName)
			};
			AuthController.gitId = userId.ToString();
			Console.WriteLine(userId);
			var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			var authProperties = new AuthenticationProperties { IsPersistent = true };
			Console.WriteLine("AA");
			await context.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
					
		}
	};
});
/*
.AddOAuth("GitHub", options =>
{
	options.ClientId = "c8fb445b6686367d7aec";
	options.ClientSecret = "d8919c506adcd9a782e9b2b0f0faf88fd8963304";
	options.CallbackPath = new PathString("/api/Auth/getUserId");
	options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
	options.TokenEndpoint = "https://github.com/login/oauth/access_token";
	options.UserInformationEndpoint = "https://api.github.com/user";
	options.Scope.Add("read:user");
	options.SaveTokens = true;

	options.Events = new OAuthEvents
	{
		OnRedirectToAuthorizationEndpoint = context =>
		{
			// Логирование URL перенаправления (включая параметр state)
			Console.WriteLine($"Redirecting to Authorization Endpoint: {context.RedirectUri}");
			return Task.CompletedTask;
		},
		OnCreatingTicket = async context =>
		{
			// Здесь можно добавить дополнительное логирование, если требуется
			Console.WriteLine("Creating ticket after successful authentication.");

			var accessToken = context.AccessToken;
			var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
			response.EnsureSuccessStatusCode();

			var json = await response.Content.ReadAsStringAsync();
			var user = JsonDocument.Parse(json);

			var userId = user.RootElement.GetProperty("id").GetString();
			var userName = user.RootElement.GetProperty("login").GetString();
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, userId),
				new Claim(ClaimTypes.Name, userName)
			};

			var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			var authProperties = new AuthenticationProperties { IsPersistent = true };
			await context.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
		}
	};
});*/


// Добавление контекста базы данных
builder.Services.AddDbContext<MyDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("MyConnection")
	));

// Добавление контроллеров
builder.Services.AddControllers();

// Добавление Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Настройка HTTP пайплайна
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication(); // Обратите внимание, что вызов добавлен для обеспечения аутентификации
app.UseAuthorization();

app.MapControllers();

app.Run();
