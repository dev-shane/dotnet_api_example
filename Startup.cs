using dotnet_api_example.DatabaseContext;
using dotnet_api_example.Middleware;
using dotnet_api_example.Swagger;
using dotnet_api_example.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_api_example.Models;
using Microsoft.AspNetCore.HttpOverrides;

namespace dotnet_api_example
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppConfig>(this.Configuration.GetSection("AppConfig"));
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                // Only loopback proxies are allowed by default.
                // Clear that restriction because forwarders are enabled by explicit 
                // configuration.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
            services.AddControllers().AddNewtonsoftJson();
            services.AddHttpContextAccessor();
            services.AddSwaggerGenNewtonsoftSupport().AddSwaggerGen(c =>
            {
                SwaggerDocument.Info.ForEach(i =>
                {
                    c.SwaggerDoc(i.Version, i);
                });
                c.OperationFilter<AuthOperationFilter>();
                c.AddSecurityDefinition(APIAuthenticationScheme.AuthenticationSchemeName, new OpenApiSecurityScheme
                {
                    Description = "Authorization: Bearer <token>",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Name = "Authorization",
                    Scheme = "bearer"
                });
            });
            services.AddDbContext<UserDbContext>(options =>
            {
                options.UseInMemoryDatabase("UserDatabase");
            });
            services.AddAuthentication(options =>
            {
                options.AddScheme<APIAuthenticationHandler>(APIAuthenticationScheme.AuthenticationSchemeName, APIAuthenticationScheme.AuthenticationDisplayName);
                //options.DefaultScheme = APIAuthenticationScheme.AuthenticationSchemeName;
            });
            services.AddSingleton<ITokenServices, TokenServices>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "dotnet_api_example v1"));
            }

            app.UseHttpsRedirection();
            app.UseForwardedHeaders();
            app.UseRouting();
            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
