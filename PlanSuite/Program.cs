using Facebook;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using PlanSuite.Data;
using PlanSuite.Models.Persistent;
using PlanSuite.Services;
using PlanSuite.Utility;
using System.Security.Claims;
using NLog;
using NLog.Web;
using PlanSuite.Interfaces;
using Microsoft.AspNetCore.Builder;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    WebsiteVersion.Init(builder.Environment.EnvironmentName);

    // Add services to the container.
    builder.Services.AddScoped<ProjectService>();
    builder.Services.AddScoped<AdminService>();
    builder.Services.AddScoped<OrganisationService>();
    builder.Services.AddScoped<AuditService>();
    builder.Services.AddScoped<TaskService>();
    builder.Services.AddScoped<IPathService, PathService>();

    var configuration = System.Environment.GetEnvironmentVariables();
    builder.Services.AddTransient<IEmailSender, EmailService>();

    //if (builder.Environment.IsProduction())
    //{
    builder.Services.AddAuthentication().AddFacebook(facebookOptions =>
    {
        facebookOptions.AppId = configuration["FacebookId"].ToString();
        facebookOptions.AppSecret = configuration["FacebookSecret"].ToString();
        facebookOptions.AccessDeniedPath = "/Home/AuthError";

        facebookOptions.Scope.Add("public_profile");
        facebookOptions.Scope.Add("email");

        facebookOptions.Events = new OAuthEvents
        {
            OnCreatingTicket = context => {
                // Use the Facebook Graph Api to get the user's email address
                // and add it to the email claim

                var client = new FacebookClient(context.AccessToken);
                dynamic info = client.Get("me", new { fields = "name,id,email" });

                context.Identity.AddClaim(new Claim(ClaimTypes.Email, info.email));
                return Task.FromResult(0);
            }
        };
    });
    //}

    // Add database connection
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        //options.UseSqlServer(connectionString));
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.AddDefaultIdentity<ApplicationUser>((options) =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
        options.Lockout.MaxFailedAccessAttempts = 3;

        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
    builder.Services.AddControllersWithViews();

    builder.Logging.ClearProviders();
#if DEBUG
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
#else
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
#endif
    builder.Host.UseNLog();

    builder.Services.AddTransient<LoggingHelper>();
    builder.Services.AddTransient<ICaptchaService, RecaptchaService>();

    var app = builder.Build();

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapRazorPages();
    app.UseStatusCodePagesWithReExecute("/error/{0}");

    using (var scope = app.Services.CreateScope())
    {
        await ApplicationDbInitialise.Initialize(scope.ServiceProvider);
    }

    new LocalisationService();
    await EmailService.InitEmailService(configuration["EmailUser"].ToString(), configuration["EmailPass"].ToString(), configuration["EmailConfigSet"].ToString(), configuration["EmailHost"].ToString(), int.Parse(configuration["EmailPort"].ToString()), configuration["EmailConfigEmail"].ToString(), configuration["EmailConfigName"].ToString());
    PaymentService.InitPaymentService(configuration["StripeApi"].ToString(), configuration["StripeSecret"].ToString(), configuration["StripePlus"].ToString(), configuration["StripePro"].ToString());

    app.Run();
}
catch(Exception exception)
{
    logger.Error(exception, "Website stopped due to exception during main");
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}
