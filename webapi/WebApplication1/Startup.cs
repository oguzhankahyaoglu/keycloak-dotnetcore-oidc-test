using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            Environment = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.SwaggerDoc("v1", new Info {Title = "My API", Version = "v1"});
                c.DescribeAllEnumsAsStrings();
                // JWT-token authentication by password
                c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OAuth2Scheme
                {
                    Flow = Configuration["Jwt:Flow"],
                    TokenUrl = Configuration["Jwt:TokenUrl"],
                    AuthorizationUrl = Configuration["Jwt:AuthUrl"]
                });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    {JwtBearerDefaults.AuthenticationScheme, new string[0]}
                });
                c.OperationFilter<AuthorizeCheckOperationFilter>();
            });
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.Authority = Configuration["Jwt:Authority"];
                o.Audience = Configuration["Jwt:Audience"];
                o.RequireHttpsMetadata = false;
                o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                };
                o.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.NoResult();
                        c.Response.ContentType = "text/plain";

                        if (Environment.IsDevelopment())
                        {
                            return c.Response.WriteAsync(c.Exception.ToString());
                        }

                        return c.Response.WriteAsync("An error occured processing your authentication.");
                    }
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                // Swashbuckle.AspNetCore 4.0.1
                c.OAuthClientId(Configuration["Jwt:ClientId"]);
                c.OAuthClientSecret(Configuration["Jwt:ClientSecret"]);
                c.OAuthRealm(Configuration["Jwt:Realm"]);
            });


            app.UseAuthentication();
            app.UseMvc();
        }
    }

    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var hasAuthorize =
                context.ApiDescription.ActionAttributes()
                    .OfType<AuthorizeAttribute>()
                    .Any();

            hasAuthorize = hasAuthorize ||
                           context.ApiDescription.ControllerAttributes()
                               .OfType<AuthorizeAttribute>()
                               .Any();

            if (hasAuthorize)
            {
                operation.Responses.Add("401", new Response {Description = "Unauthorized"});
                operation.Responses.Add("403", new Response {Description = "Forbidden"});

                operation.Security = new List<IDictionary<string, IEnumerable<string>>>
                {
                    new Dictionary<string, IEnumerable<string>>
                        {{JwtBearerDefaults.AuthenticationScheme, new string[0] { }}}
                };
            }
        }
    }
}