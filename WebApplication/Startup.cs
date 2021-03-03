using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        private string weatherAppVirtualDir = "/forecast";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
	        services.AddControllers()
		        .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
		        .AddNewtonsoftJson(options =>
		        {
			        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			        options.SerializerSettings.Converters.Add(new StringEnumConverter());
			        options.SerializerSettings.Formatting = Formatting.Indented;
		        })
		        .AddControllersAsServices();
            
            services.AddApiVersioning(o =>
            {
	            o.AssumeDefaultVersionWhenUnspecified = true;
	            o.DefaultApiVersion = new ApiVersion(1, 0);
	            o.ReportApiVersions = true;
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
	            options.SuppressModelStateInvalidFilter = true;
            });
            
            services.AddVersionedApiExplorer(options =>
            {
	            // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
	            // note: the specified format code will format the version as "'v'major[.minor][-status]"
	            options.GroupNameFormat = "'v'VVV";

	            // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
	            // can also be used to control the format of the API version in route templates
	            options.SubstituteApiVersionInUrl = true;
            });
            
            services.AddSwaggerGen(c =>
            {
	            c.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "Weather API" });
	            //c.SwaggerDoc("v2", new OpenApiInfo { Version = "v2", Title = "Weather API - v2.0" });
	            c.IgnoreObsoleteActions();

	            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.XmlComments.xml";
	            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
	            c.IncludeXmlComments(xmlPath);
	            c.CustomOperationIds(apiDesc =>
	            {
		            return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : string.Empty;
	            });

	            c.ExampleFilters();
            });
            services.AddSwaggerGenNewtonsoftSupport();

            services.ConfigureSwaggerGen(options =>
            {
	            options.CustomSchemaIds(x => x.FullName);
	            options.IgnoreObsoleteActions();
            });
            
            services.AddMvc(options => options.EnableEndpointRouting = false);

            services.AddSwaggerExamplesFromAssemblyOf<Startup>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IApiVersionDescriptionProvider provider)
		{
			app.UsePathBase(weatherAppVirtualDir);
			
			app.UseSwagger(c =>
			{
				c.SerializeAsV2 = true;
				c.RouteTemplate = "swagger/{documentName}/swagger.json";

				c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
				{
					swaggerDoc.Servers = new List<OpenApiServer>
					{
						new OpenApiServer
						{
							Url = $"https://{httpReq.Host.Value}{weatherAppVirtualDir}"
						}
					};
				});
			});

			app.UseSwaggerUI(c =>
			{
				// build a swagger endpoint for each discovered API version
				foreach (var description in provider.ApiVersionDescriptions)
				{
					c.SwaggerEndpoint($"{weatherAppVirtualDir}/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
				}

				c.RoutePrefix = "swagger/api-docs";
			});
			app.UseMvc();
			app.Map("", ConfigureCommercePath);
		}

		private void ConfigureCommercePath(IApplicationBuilder app)
		{
			app.UseRouting();
			app.UseResponseCaching();
			
				app.UseExceptionHandler("/Error");
				app.UseHsts();
			

			app.UseHttpsRedirection()
				.UseStaticFiles()
				.UseCookiePolicy();

			app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
		}
    }
}