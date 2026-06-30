using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using PasswordManagement.Data;
using PasswordManagement.Authentication;
using PasswordManagement.Services;
using PasswordManagement.Security;
using Microsoft.AspNetCore.Components.Authorization;
using NLog;
using NLog.Web;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("application started");

try
{
    var builder = WebApplication.CreateBuilder(args);

    //StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

    // Add services to the container.
    builder.Services.AddAuthenticationCore();
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();

    builder.Services.AddDataProtection()
        .SetApplicationName("PasswordManagement");
    builder.Services.AddScoped<ProtectedSessionStorage>();
    builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthentication>();
    builder.Services.AddDbContext<AccountContext>(option =>
    {
        option.UseSqlite(builder.Configuration.GetConnectionString("Accounts")
            ?? builder.Configuration["ConnectionString"]
            ?? throw new InvalidOperationException("The accounts database connection string is not configured."));
    });
    builder.Services.AddScoped<AccountServices>();
    builder.Services.AddSingleton<IUserPasswordService, UserPasswordService>();
    builder.Services.AddSingleton<ICredentialProtector, CredentialProtector>();
    builder.Services.AddScoped<StoredDataSecurityMigrator>();
    builder.Services.AddScoped<AdminBootstrapper>();
    builder.Services.AddSingleton<IPasswordGenerator, PasswordGenerator>();
    builder.Services.AddMudServices(option =>
    {
        option.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
        option.SnackbarConfiguration.PreventDuplicates = true;
        option.SnackbarConfiguration.NewestOnTop = false;
        option.SnackbarConfiguration.ShowCloseIcon = false;
        option.SnackbarConfiguration.VisibleStateDuration = 5000;
        option.SnackbarConfiguration.HideTransitionDuration = 500;
        option.SnackbarConfiguration.ShowTransitionDuration = 500;
        option.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
    });
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();
    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AccountContext>();
        await dbContext.Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<AdminBootstrapper>().EnsureAdminAsync();
        await scope.ServiceProvider.GetRequiredService<StoredDataSecurityMigrator>().UpgradeAsync();
    }

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseRouting();

    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex);
    throw;
}
finally
{
    NLog.LogManager.Shutdown();
}
