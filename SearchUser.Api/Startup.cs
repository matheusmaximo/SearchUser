﻿using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SearchUser.Api.Manager;
using SearchUser.Api.Persistence;
using SearchUser.Entities.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace SearchUser.Api
{
    /// <summary>
    /// Startup class
    /// </summary>
    public class Startup
    {
        public IConfiguration Configuration { get; }

        #region Startup default methods
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            // DbContext comes from Configure{Environment}Services method
            services.AddMvc();

            // AutoMapper settings
            services.AddAutoMapper(typeof(Startup));

            // Swagger settings
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info { Title = "Search User API", Version = "1.0", Description = "Simple Search User API created to demonstrate skills as part of a Genesis Automation back-end challenge", Contact = new Contact { Name = "Matheus Maximo de Araujo", Email = "matheusmaximo@gmail.com", Url = "https://github.com/matheusmaximo" } });
            });

            // Identity settings
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<SearchUserDbContext>()
                .AddSignInManager<ApplicationSignInManager>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Make sure email user is unique
                options.User.RequireUniqueEmail = true;
            });

            // Custom method to add security settings
            services.AddApplicationSecurity(Configuration);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, SearchUserDbContext dbContext)
        {
            // Use Authentication
            app.UseAuthentication();

            // Swagger settings
            app.UseSwagger(c => c.RouteTemplate = "documentation/{documentName}/swagger.json");
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = "documentation";
                options.SwaggerEndpoint("/documentation/v1/swagger.json", "Search User API v1");
            });

            // DotNetCore settings
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "api/{action}/{id?}");
            });

            // Ensure tables are created
            dbContext.Database.EnsureCreated();
        }
        #endregion

        #region Production Environment methods
        public void ConfigureProductionServices(IServiceCollection services)
        {
            services.AddDbContext<SearchUserDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            this.ConfigureServices(services);
        }
        #endregion

        #region Development Environment methods
        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            services.AddDbContext<SearchUserDbContext>(options => options.UseInMemoryDatabase("SearchUser"));
            this.ConfigureServices(services);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void ConfigureDevelopment(IApplicationBuilder app, SearchUserDbContext dbContext)
        {
            app.UseDeveloperExceptionPage();
            this.Configure(app, dbContext);
        }
        #endregion

        #region Constructor
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        #endregion
    }
}
