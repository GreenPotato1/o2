using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Linq;
using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using O2.Black.Toolkit.Core;
using O2.Certificate.API.Helper;
using O2.Certificate.Data;
using O2.Certificate.Data.Models.O2C;
using O2.Certificate.Data.Models.O2Ev;
using O2.Certificate.Repositories;
using O2.Certificate.Repositories.Interfaces;


namespace O2.Certificate.API
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }

        public virtual void ConfigureEntityFramework(IServiceCollection services)
        {
            services.AddDbContext<O2BusinessDataContext>(x =>
                x.UseSqlServer(Configuration.GetConnectionString("DBConnectO2Business")));
            
            Debug.WriteLine(Configuration.GetConnectionString("DBConnectO2Business"));
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0).AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddCors();
            services.AddAutoMapper();
            // In short, that is a breaking change in Asp.Net Core 3.0.
            // services.AddControllers(
            //     //options => options.SuppressAsyncSuffixInActionNames = false
            //     );

            services.AddControllers()
                .AddNewtonsoftJson();
            //Todo: Will create fix for clear list
            CloudStorage.Instance.Clear();
            CloudStorage.Instance.AccountCloudStorages.Add(
                new AccountCloudStorage()
                {
                    AccountName = Configuration.GetSection("CloudStorage:Certificates:AccountName").Value,
                    Container = Configuration.GetSection("CloudStorage:Certificates:Container").Value,
                    AccountKey = Configuration.GetSection("CloudStorage:Certificates:AccountKey").Value,
                    TypeTable = TypeTable.Certificates
                });
            CloudStorage.Instance.AccountCloudStorages.Add(
                new AccountCloudStorage()
                {
                    AccountName = Configuration.GetSection("CloudStorage:Users:AccountName").Value,
                    Container = Configuration.GetSection("CloudStorage:Users:Container").Value,
                    AccountKey = Configuration.GetSection("CloudStorage:Users:AccountKey").Value,
                    TypeTable = TypeTable.Users
                });
            CloudStorage.Instance.AccountCloudStorages.Add(
                new AccountCloudStorage()
                {
                    AccountName = Configuration.GetSection("CloudStorage:Events:AccountName").Value,
                    Container = Configuration.GetSection("CloudStorage:Events:Container").Value,
                    AccountKey = Configuration.GetSection("CloudStorage:Events:AccountKey").Value,
                    TypeTable = TypeTable.Events
                });

            ConfigureEntityFramework(services);

            services.AddScoped<IEventBaseRepository<O2EvEvent>, EventBaseRepository<O2EvEvent>>();
            services.AddScoped<ICertificateBaseRepository<O2CCertificate>, CertificateBaseRepository<O2CCertificate>>();

            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            });

            IServiceCollection serviceCollection = services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1.1", new OpenApiInfo {Title = "Versioned Api v1.1", Version = "v1.1"});
                c.SwaggerDoc("v1.0",
                    new OpenApiInfo
                    {
                        Title = "Versioned Api v1.0",
                        Version = "v1.0"
                    }
                );
                c.DescribeAllParametersInCamelCase();

                // Apply the filters
                c.OperationFilter<RemoveVersionFromParameter>();
                c.DocumentFilter<ReplaceVersionWithExactValueInPath>();

                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var actionApiVersionModel = apiDesc.ActionDescriptor?.GetApiVersion();

                    // would mean this action is unversioned and should be included everywhere
                    if (actionApiVersionModel == null)
                    {
                        return true;
                    }

                    if (actionApiVersionModel.DeclaredApiVersions.Any())
                    {
                        return actionApiVersionModel.DeclaredApiVersions.Any(v => $"v{v.ToString()}" == docName);
                    }

                    return actionApiVersionModel.ImplementedApiVersions.Any(v => $"v{v.ToString()}" == docName);
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
//             using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
//             {
//                 HelperDBContext.Context = serviceScope.ServiceProvider.GetRequiredService<O2BusinessDataContext>();
//                 // var context = serviceScope.ServiceProvider.GetRequiredService<O2BusinessDataContext>();
// #if DEBUG
//                 // context.Database.EnsureDeleted();
//                 // context.Database.EnsureCreated();
// #else
//                 // HelperDBContext.Context.Database.Migrate();
// #endif
//             }

            //preload default images for storage
            const string notImage = "not_image.jpg";
            const string path = "Files/" + notImage;

            LoadDefaultUrl(path, notImage, TypeTable.Events);
            LoadDefaultUrl(path, notImage, TypeTable.Certificates);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(builder =>
                {
                    builder.Run(async context =>
                    {
                        context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

                        var error = context.Features.Get<IExceptionHandlerFeature>();

                        if (error != null)
                        {
                            context.Response.AddApplicationError(error.Error.Message);
                            await context.Response.WriteAsync(error.Error.Message);
                        }
                    });
                });
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1.1/swagger.json", "O2 Business API V1.1");
                c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "O2 Business API V1.0");
            });

            // app.UseHttpsRedirection();
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        private static void LoadDefaultUrl(string path, string notImage, TypeTable typeTable)
        {
            if (!File.Exists(path))
            {
                throw new Exception("File not found - " + path);
            }

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            switch (typeTable)
            {
                case TypeTable.Certificates:
                    HelperDefaulter.UrlCertificates = AzureBlobHelper.UploadFileToStorage(stream,
                        fileName: "default" + Path.GetExtension(notImage).ToLower(),
                        typeTable).GetAwaiter().GetResult();
                    break;
                case TypeTable.Users:
                    HelperDefaulter.UrlUsers = AzureBlobHelper.UploadFileToStorage(stream,
                        fileName: "default" + Path.GetExtension(notImage).ToLower(),
                        typeTable).GetAwaiter().GetResult();
                    break;
                case TypeTable.Events:
                    HelperDefaulter.UrlEvents = AzureBlobHelper.UploadFileToStorage(stream,
                        fileName: "default" + Path.GetExtension(notImage).ToLower(),
                        typeTable).GetAwaiter().GetResult();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeTable), typeTable, null);
            }
        }
    }

    public class HelperDBContext
    {
        public static O2BusinessDataContext Context { get; set; }
    }
}