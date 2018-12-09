using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API
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
			var key = Encoding.ASCII.GetBytes(Configuration.GetSection("AppSettings:Token").Value);
			services.AddDbContext<DataContext>(x => x.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

			IdentityBuilder builder = services.AddIdentityCore<User>(opt =>
			{
				opt.Password.RequireDigit = false;
				opt.Password.RequiredLength = 6;
				opt.Password.RequireUppercase = false;
				opt.Password.RequireNonAlphanumeric = false;
			});

			// to be able to query the user and get the roles as well
			builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);
			//  telling identity to use entity framework as our store, creates table for identity
			builder.AddEntityFrameworkStores<DataContext>();
			//we nedd role validator  to validate roles, sign in and role manager to create and remove roles and be able to sign in as well
			builder.AddRoleValidator<RoleValidator<Role>>();
			builder.AddRoleManager<RoleManager<Role>>();
			builder.AddSignInManager<SignInManager<User>>();

			// configure auth middle ware to use jwt 
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = false,
					ValidateAudience = false,
				};
			});


			//policy based roles
			services.AddAuthorization(options =>
			{
				options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
				options.AddPolicy("ModeratorPhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
				options.AddPolicy("VipOnly", policy => policy.RequireRole("VIP"));
			});

			//  allow global authorization
			services.AddMvc(options =>
			{
				var policy = new AuthorizationPolicyBuilder()
				.RequireAuthenticatedUser()
				.Build();
				options.Filters.Add(new AuthorizeFilter(policy));
			})
			.SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
			.AddJsonOptions(opt =>
			{
				// we need to ignore reference loop handling so as to get the results of relationship in json format
				opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
			});

			services.Configure<ApiBehaviorOptions>(options =>
			{
				options.SuppressInferBindingSourcesForParameters = true;
			});

			// if we wanted global effect, we add it to addmvc options
			services.AddScoped<LogUserActvity>();
			//for our cloudinary
			services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));

			//for seeding data
			services.AddTransient<Seed>();
			//for mapping
			services.AddAutoMapper();

			services.AddCors();

			// dependency injection
			services.AddScoped<IDatingRepository, DatingRepository>();

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, Seed seeder)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseHsts();
				app.UseExceptionHandler(builder =>
				{
					//execute the middleware RUN
					builder.Run(async context =>
					{
						context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

						var error = context.Features.Get<IExceptionHandlerFeature>();
						if (error != null)
						{
							context.Response.AddApplicationError(error.Error.Message);
							await context.Response.WriteAsync(error.Error.Message);
						}
					});
				});
			}

			seeder.SeedUsers();
			app.UseHttpsRedirection();
			app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials());
			//for ng build
			app.UseDefaultFiles();
			app.UseStaticFiles();

			app.UseAuthentication();
			//for ng build
			app.UseMvc(routes =>
			{
				routes.MapSpaFallbackRoute(
					name: "spa-fallback",
					defaults: new { controller = "Fallback", action = "Index" }
					);
			});
		}
	}
}
