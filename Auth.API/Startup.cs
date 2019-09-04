using System;
using System.Text;
using Auth.API.DbContext;
using Auth.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Auth.API
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(_configuration.GetSection("Auth")["ConnStr"]));

            //we have to add this when we use IdentityUser and IdentityDbContext
            services.AddDefaultIdentity<User>()
                .AddEntityFrameworkStores<AuthDbContext>();

            // changing password rules because we are inheriting IdentityUser since we are only developing
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 4;
            });

            services.AddCors();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //jwt Authentication
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = false;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = _configuration.GetValue<string>("AuthConfig:development:JwtIssuer"),
                        ValidAudience = _configuration.GetValue<string>("AuthConfig:development:JwtIssuer"),
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    _configuration.GetValue<string>("AuthConfig:development:secret"))),
                        ClockSkew = TimeSpan.Zero // remove delay of token when expire
                    };
                });

            RegisterServicesInOtherAssemblies(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(x => x.AllowAnyMethod().AllowAnyHeader().WithOrigins("http://localhost:4200"));
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
        }

        //Using Scrutor to automatically register your services with the ASP.NET Core, so no need to add like services.Addscope.........
        private static void RegisterServicesInOtherAssemblies(IServiceCollection services)
        {
            services.Scan(scan => scan
                .FromExecutingAssembly()
                .FromApplicationDependencies(dep =>
                    dep.FullName.StartsWith("Auth.API", StringComparison.CurrentCultureIgnoreCase))
                .AddClasses()
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            );
        }
    }
}