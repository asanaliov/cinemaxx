using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Cinemaxx.Data;
using Cinemaxx.Models;
using Cinemaxx.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Cinemaxx;
using Cinemaxx.Extensions;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IScreeningService, ScreeningService>();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
        options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(SharedResource)));
builder.Services.AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
        options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(SharedResource)));

builder.Services.AddOptions<MvcOptions>().Configure<IStringLocalizerFactory>((options, factory) =>
{
    options.ModelMetadataDetailsProviders.Add(new LocalizationMetadataProvider(factory));

    var localizer = factory.Create(typeof(SharedResource));
    var messages = options.ModelBindingMessageProvider;
    messages.SetValueMustNotBeNullAccessor(value => localizer["The value '{0}' is invalid.", value]);
    messages.SetAttemptedValueIsInvalidAccessor((value, field) => localizer["The value '{0}' is not valid for {1}.", value, field]);
    messages.SetValueIsInvalidAccessor(value => localizer["The value '{0}' is invalid.", value]);
    messages.SetValueMustBeANumberAccessor(field => localizer["The field {0} must be a number.", field]);
    messages.SetMissingBindRequiredValueAccessor(field => localizer["A value for the '{0}' parameter or property was not provided.", field]);
});

// Macedonian month and day names, but keep "." as the decimal separator so prices
// and ratings round-trip the same way in forms, client validation and data attributes.
var macedonian = new CultureInfo("mk");
macedonian.NumberFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();

var supportedCultures = new[]
{
    new CultureInfo("en"),
    macedonian
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
}

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
app.UseRouting();
app.UseRequestLocalization(app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "culture",
    pattern: "culture/{culture}",
    defaults: new { controller = "Home", action = "SetLanguage" });

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();