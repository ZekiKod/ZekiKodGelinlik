using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.Authentication.ClientServer;
using DevExpress.ExpressApp.WebApi.Services;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.OData;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using ZekiKodGelinlik.Blazor.Server.Controllers;
using ZekiKodGelinlik.Blazor.Server.Services;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;
using ZekiKodGelinlik.WebApi.JWT;

namespace ZekiKodGelinlik.Blazor.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            AppSettingsProvider.ExchangeRateProviderUrl = Configuration["ExchangeRateProvider:Url"];
            AppSettingsProvider.AgentServiceUrl = Configuration["AgentService:Url"];
            services.AddSingleton(typeof(Microsoft.AspNetCore.SignalR.HubConnectionHandler<>), typeof(ProxyHubConnectionHandler<>));
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHttpContextAccessor();
            services.AddHttpClient();
            services.AddSingleton<IFileService, FileService>();
            services.AddScoped<IAuthenticationTokenProvider, JwtTokenProviderService>();
            services.AddScoped<CircuitHandler, CircuitHandlerProxy>();
            services.AddScoped<SpeechCommandController>();

            services.AddXaf(Configuration, builder =>
            {
                builder.UseApplication<ZekiKodGelinlikBlazorApplication>();
                builder.AddXafWebApi(webApiBuilder =>
                {
                    webApiBuilder.AddXpoServices();
                    webApiBuilder.ConfigureOptions(options => { });
                });
                builder.Modules
                    .AddAuditTrailXpo()
                    .AddCloningXpo()
                    .AddConditionalAppearance()
                    .AddDashboards(options => { options.DashboardDataType = typeof(DevExpress.Persistent.BaseImpl.DashboardData); })
                    .AddFileAttachments()
                    .AddOffice()
                    .AddReports(options =>
                    {
                        options.EnableInplaceReports = true;
                        options.ReportDataType = typeof(DevExpress.Persistent.BaseImpl.ReportDataV2);
                        options.ReportStoreMode = DevExpress.ExpressApp.ReportsV2.ReportStoreModes.XML;
                        options.ShowAdditionalNavigation = true;
                    })
                    .AddScheduler()
                    .AddValidation(options => { options.AllowValidationDetailsAccess = false; })
                    .AddViewVariants()
                    .Add<ZekiKodGelinlik.Module.ZekiKodGelinlikModule>()
                    .Add<ZekiKodGelinlikBlazorModule>();

                builder.ObjectSpaceProviders
                    .AddSecuredXpo((serviceProvider, options) =>
                    {
                        string connectionString = Configuration.GetConnectionString("ConnectionString");
                        ArgumentNullException.ThrowIfNull(connectionString);
                        options.ConnectionString = connectionString;
                        options.ThreadSafe = true;
                        options.UseSharedDataStoreProvider = true;
                    })
                    .AddNonPersistent();

                builder.Security
                    .UseAuthenticationStandard(options =>
                    {
                        options.RoleType = typeof(PermissionPolicyRole);
                        options.UserLoginInfoType = typeof(ZekiKodGelinlik.Module.BusinessObjects.ApplicationUserLoginInfo);

                        // Birden fazla kullanıcı tipini destekle
                        options.UserTypes.Add(new UserType(typeof(ZekiKodGelinlik.Module.BusinessObjects.ApplicationUser)));
                        options.UserTypes.Add(new UserType(typeof(PortalUser)));
                    })
                    .AddPasswordAuthentication(options =>
                    {
                        options.IsSupportChangePassword = true;
                    });
            });

            var authentication = services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);
            authentication
                .AddCookie(options => { options.LoginPath = "/LoginPage"; })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Authentication:Jwt:IssuerSigningKey"]))
                    };
                });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .RequireXafAuthentication()
                    .Build();
            });

            services.AddControllers()
                .AddOData((options, serviceProvider) =>
                {
                    options
                        .AddRouteComponents("api/odata", new EdmModelBuilder(serviceProvider).GetEdmModel())
                        .EnableQueryFeatures(100);
                });

            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ZekiKodGelinlik API", Version = "v1" });
                c.AddSecurityDefinition("JWT", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Name = "Bearer", Scheme = "bearer", BearerFormat = "JWT", In = ParameterLocation.Header });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "JWT" } }, new string[0] } });
            });

            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o => { o.JsonSerializerOptions.PropertyNamingPolicy = null; });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SpeechCommandController speechCommandController)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "ZekiKodGelinlik WebApi v1"); });
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseRequestLocalization();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseXaf();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapXafEndpoints();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapControllers();
            });
        }
    }
}
