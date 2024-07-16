using Develoop.Forms.Blazor.Server.API.Security;
using Develoop.Forms.Blazor.Server.Code;
using Develoop.Forms.Blazor.Server.Services;
using Develoop.Forms.Module.BusinessObjects;
using Develoop.Forms.Module.BusinessObjects.Moduli;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.MultiTenancy;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.Authentication.ClientServer;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo.Helpers;
using DevExpress.Xpo.Metadata;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Develoop.Forms.Blazor.Server;

public class Startup
{
    public Startup(IConfiguration configuration) { Configuration = configuration; }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(
            typeof(Microsoft.AspNetCore.SignalR.HubConnectionHandler<>),
            typeof(ProxyHubConnectionHandler<>));

        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddHttpContextAccessor();
        services.AddScoped<CircuitHandler, CircuitHandlerProxy>();
        services.AddScoped<IAuthenticationTokenProvider, JwtTokenProviderService>();

        services.AddControllers()
            .AddJsonOptions(
                options =>
                {
                    XPDictionary dictionary = new ReflectionDictionary();
                    dictionary.GetDataStoreSchema(typeof(Modulo), typeof(RisultatiModulo));
                    options.JsonSerializerOptions.Converters.Add(new ChangesSetJsonConverterFactory(null));
                    options.JsonSerializerOptions.Converters.Add(new XpoModelJsonConverterFactory(dictionary));
                });
        services.AddXaf(
            Configuration,
            builder =>
            {
                builder.UseApplication<FormsBlazorApplication>();
                builder.Modules
                    .AddConditionalAppearance()
                    .AddValidation(
                        options =>
                        {
                            options.AllowValidationDetailsAccess = false;
                        })
                    .Add<Develoop.Forms.Module.FormsModule>()
                    .Add<FormsBlazorModule>();

                builder.AddMultiTenancy()
                    .WithHostDatabaseConnectionString(Configuration.GetConnectionString("ConnectionString"))
#if EASYTEST
                .WithHostDatabaseConnectionString(Configuration.GetConnectionString("EasyTestConnectionString"))   
#endif

                    .WithMultiTenancyModelDifferenceStore(
                        options =>
                        {
#if !RELEASE
                            options.UseTenantSpecificModel = false;
#endif
                        })
                    .WithTenantResolver<TenantByEmailResolver>();

                builder.ObjectSpaceProviders
                    .AddSecuredXpo(
                        (serviceProvider, options) =>
                        {
                            string connectionString = serviceProvider.GetRequiredService<IConnectionStringProvider>()
                                .GetConnectionString();
                            options.ConnectionString = connectionString;
                            options.ThreadSafe = true;
                            options.UseSharedDataStoreProvider = true;
                        })
                    .AddNonPersistent();
                builder.ObjectSpaceProviders.Events.OnObjectSpaceCreated = context =>
                {
                    CompositeObjectSpace compositeObjectSpace = context.ObjectSpace as CompositeObjectSpace;
                    if(compositeObjectSpace != null)
                    {
                        if(!(compositeObjectSpace.Owner is CompositeObjectSpace))
                        {
                            var objectSpaceProviderService = context.ServiceProvider
                                .GetRequiredService<IObjectSpaceProviderService>();
                            var objectSpaceCustomizerService = context.ServiceProvider
                                .GetRequiredService<IObjectSpaceCustomizerService>();
                            compositeObjectSpace.PopulateAdditionalObjectSpaces(
                                objectSpaceProviderService,
                                objectSpaceCustomizerService);
                        }
                    }
                };

                builder.Security
                    .UseIntegratedMode(
                        options =>
                        {
                            options.RoleType = typeof(PermissionPolicyRole);
                            // ApplicationUser descends from PermissionPolicyUser and supports the OAuth authentication. For more information, refer to the following topic: https://docs.devexpress.com/eXpressAppFramework/402197
                            // If your application uses PermissionPolicyUser or a custom user type, set the UserType property as follows:
                            options.UserType = typeof(Develoop.Forms.Module.BusinessObjects.ApplicationUser);
                            // ApplicationUserLoginInfo is only necessary for applications that use the ApplicationUser user type.
                            // If you use PermissionPolicyUser or a custom user type, comment out the following line:
                            options.UserLoginInfoType = typeof(Develoop.Forms.Module.BusinessObjects.ApplicationUserLoginInfo);
                            options.UseXpoPermissionsCaching();
                        })
                    .AddPasswordAuthentication(
                        options =>
                        {
                            options.IsSupportChangePassword = true;
                        });
            });
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(
                options =>
                {
                    options.LoginPath = "/LoginPage";
                })
            .AddJwtBearer(
                options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        //ValidIssuer = Configuration["Authentication:Jwt:Issuer"],
                        //ValidAudience = Configuration["Authentication:Jwt:Audience"],
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(Configuration["Authentication:Jwt:IssuerSigningKey"]))
                    };
                });
        services.AddXafSecurity(
            options =>
            {
                options.RoleType = typeof(PermissionPolicyRole);
                // ApplicationUser descends from PermissionPolicyUser and supports the OAuth authentication. For more information, refer to the following topic: https://docs.devexpress.com/eXpressAppFramework/402197
                // If your application uses PermissionPolicyUser or a custom user type, set the UserType property as follows:
                options.UserType = typeof(ApplicationUser);
                // ApplicationUserLoginInfo is only necessary for applications that use the ApplicationUser user type.
                // If you use PermissionPolicyUser or a custom user type, comment out the following line:
                options.UserLoginInfoType = typeof(ApplicationUserLoginInfo);
                options.Events.OnSecurityStrategyCreated = securityStrategy => ((SecurityStrategy)securityStrategy).RegisterXPOAdapterProviders(
                    );
                options.SupportNavigationPermissionsForTypes = false;
            });
        services.AddSwaggerGen(
            c =>
            {
                c.EnableAnnotations();
                c.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                    {
                        Title = "Develoop.OSDB.Pos API",
                        Version = "v1",
                        Description =
                            @"Use AddXafWebApi(options) in the Develoop.OSDB.Pos.WebApi\Startup.cs file to make Business Objects available in the Web API."
                    });
                c.AddSecurityDefinition(
                    "JWT",
                    new OpenApiSecurityScheme()
                    {
                        Type = SecuritySchemeType.Http,
                        Name = "Bearer",
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header
                    });
                c.AddSecurityRequirement(
                    new OpenApiSecurityRequirement()
                    {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Reference =
                                new OpenApiReference()
                                        {
                                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                            Id = "JWT"
                                        }
                        },
                        new string[0]
                    },
                    });
            });
        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(
            o =>
            {
                //The code below specifies that the naming of properties in an object serialized to JSON must always exactly match
                //the property names within the corresponding CLR type so that the property names are displayed correctly in the Swagger UI.
                //XPO is case-sensitive and requires this setting so that the example request data displayed by Swagger is always valid.
                //Comment this code out to revert to the default behavior.
                //See the following article for more information: https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsonserializeroptions.propertynamingpolicy
                o.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
        services.AddAuthorization(
            options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                  .RequireAuthenticatedUser()
                    .RequireXafAuthentication()
                    .Build();
            });
        services.AddCors(
            options =>
            {
                options.AddPolicy(
                    "NewPolicy",
                    builder =>
            builder.AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader());
            });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if(env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(
                c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Develoop.Forms WebApi v1");
                });
        } else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. To change this for production scenarios, see: https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseRequestLocalization();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseXaf();
        app.UseCors("NewPolicy");
        app.UseEndpoints(
            endpoints =>
            {
                endpoints.MapXafEndpoints();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapControllers();
            });
    }
}
