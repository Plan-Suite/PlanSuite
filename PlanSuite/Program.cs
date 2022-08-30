using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using PlanSuite.Data;
using PlanSuite.Models.Persistent;
using PlanSuite.Services;
using PlanSuite.Utility;

var builder = WebApplication.CreateBuilder(args);

WebsiteVersion.Init(builder.Environment.EnvironmentName);

// Add services to the container.
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<OrganisationService>();

var configuration = System.Environment.GetEnvironmentVariables();
builder.Services.AddTransient<IEmailSender, EmailService>();

if (builder.Environment.IsProduction())
{
    builder.Services.AddAuthentication().AddFacebook(facebookOptions =>
    {
        facebookOptions.AppId = configuration["FacebookId"].ToString();
        facebookOptions.AppSecret = configuration["FacebookSecret"].ToString();
        facebookOptions.AccessDeniedPath = "/Home/AuthError";
    });
}

// Add database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    //options.UseSqlServer(connectionString));
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>((options) => 
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

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

using (var scope = app.Services.CreateScope())
{
    await ApplicationDbInitialise.Initialize(scope.ServiceProvider);
}

new LocalisationService();
await EmailService.InitEmailService(configuration["EmailUser"].ToString(), configuration["EmailPass"].ToString(), configuration["EmailConfigSet"].ToString(), configuration["EmailHost"].ToString(), int.Parse(configuration["EmailPort"].ToString()), configuration["EmailConfigEmail"].ToString(), configuration["EmailConfigName"].ToString());
PaymentService.InitPaymentService(configuration["StripeApi"].ToString(), configuration["StripeSecret"].ToString(), configuration["StripePlus"].ToString(), configuration["StripePro"].ToString());

app.Run();
