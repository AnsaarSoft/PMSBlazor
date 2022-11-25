using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using PasswordManagement.Data;
using PasswordManagement.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using NLog;
using NLog.Web;
using System;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("application started");

try
{
    var builder = WebApplication.CreateBuilder(args);

    StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

    // Add services to the container.
    builder.Services.AddAuthenticationCore();
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();
    builder.Services.AddBlazoredLocalStorage();
    builder.Services.AddScoped<ProtectedSessionStorage>();
    builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthentication>();
    builder.Services.AddDbContext<AccountContext>(option =>
    {
        option.UseSqlite("Data Source = Data\\accounts.db");
    });
    builder.Services.AddScoped<AccountServices>();

    builder.Services.AddMudServices();
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
    throw (ex);
}
finally
{
    NLog.LogManager.Shutdown();
}
