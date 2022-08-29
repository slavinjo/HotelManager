using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using AspNetCore.Authentication.ApiKey;
using Hotel.Server.Data;
using Npgsql.Logging;
using Hotel.Server.Helpers;
using Hotel.Server.Integrations;
using Hotel.Server.Notifications;
using Hotel.Server.Hotels;
using Hotel.Server.Users;
using Microsoft.AspNetCore.Http;


namespace Hotel.Server;

public class Startup
{
    private IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        StaticConfiguration.Initialize(Configuration);
        var dbConnectionString = StaticConfiguration.ConnectionStringsHotelDB;

        services.AddCors();
        services.AddControllers();

        // configure jwt authentication
        var key = Encoding.ASCII.GetBytes(StaticConfiguration.AppSettingsSecret);
        services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            })
            .AddApiKeyInQueryParams<ApiKeyProvider>(x =>
            {
                x.Realm = "ApiKey";
                x.KeyName = "apiKey";
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AuthPolicy", policy =>
            {
                policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                policy.AuthenticationSchemes.Add(ApiKeyDefaults.AuthenticationScheme);
                policy.RequireAuthenticatedUser();
            });

            options.DefaultPolicy = options.GetPolicy("AuthPolicy");
        });

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Test Hotel",
                Description = "Test Hotel Server API",
                TermsOfService = new Uri("https://www.google.com"),
            });

            c.AddSecurityDefinition("Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http
                });

            c.AddSecurityDefinition("apiKey",
                new OpenApiSecurityScheme
                {
                    Name = "apiKey",
                    Scheme = "apiKey",
                    In = ParameterLocation.Query,
                    Type = SecuritySchemeType.ApiKey
                });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "Bearer"}
                    },
                    new List<string>()
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id = "apiKey"}
                    },
                    new List<string>()
                }
            });

            // uncomment to generate Swagger descriptions from summary comments
            // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            // c.IncludeXmlComments(xmlPath);
        });

        // configure DI for application services
        services.AddDbContext<HotelContext>(options => options.UseNpgsql(dbConnectionString));
        services.AddHttpContextAccessor();
        services.AddAutoMapper(typeof(Mappings.Mappings));
        services.AddSingleton<DateTimeProvider>();
        services.AddScoped<DbUserTrackingService>();

        Dapper.SqlMapper.AddTypeHandler<Dictionary<string, object>>(new JsonHandler<Dictionary<string, object>>());
        Dapper.SqlMapper.AddTypeHandler<List<int>>(new JsonHandler<List<int>>());
        Dapper.SqlMapper.AddTypeHandler<List<Guid>>(new JsonHandler<List<Guid>>());
        Dapper.SqlMapper.AddTypeHandler<List<string>>(new JsonHandler<List<string>>());
        Dapper.SqlMapper.AddTypeHandler(new DateTimeHandler());
        services.AddScoped<UserService>();
        services.AddScoped<IMailgunService, MailgunService>();
        services.AddScoped<NotificationService>();
        services.AddScoped<HotelService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, HotelContext hotelContext)
    {
        NpgsqlLogManager.IsParameterLoggingEnabled = true;

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseExceptionHandler("/error");

        app.UseRouting();

        // global cors policy
        app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        app.UseAuthentication();
        app.UseAuthorization();

        app.UsePermissionLevel();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", context => context.Response.WriteAsync("Test Hotel API"));
            endpoints.MapGet("/health", context => context.Response.WriteAsync("ok"));
            endpoints.MapGet("/version",
                context => context.Response.WriteAsync(System.IO.File.Exists("version.txt")
                    ? System.IO.File.ReadAllText("version.txt")
                    : "non-production"));
        });

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Server"); });

        if (hotelContext.Database.GetPendingMigrations().Any())
        {
            Console.WriteLine("Applying DB migrations");
            hotelContext.Database.Migrate();
        }
    }
}
