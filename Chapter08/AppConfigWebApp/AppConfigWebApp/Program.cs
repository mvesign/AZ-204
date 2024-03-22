using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.FeatureManagement;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

// Get the connectionstring from the setup user secret.
var connectionString = builder.Configuration.GetConnectionString("AppConfig");

// And configure the Azure App Configuration connection here.
builder.Host.ConfigureAppConfiguration((hostingContext, builder) =>
{
    builder.AddAzureAppConfiguration(options =>
    {
        options
            // Connect with the Azure App Configuration.
            .Connect(connectionString)
            // Setup of feature flags.
            .UseFeatureFlags(option =>
            {
                option.Select("demofeature", hostingContext.HostingEnvironment.EnvironmentName);
            })
            // Monitor for any changes in the Azure App Configuration.
            .ConfigureRefresh(refreshOptions =>
            {
                refreshOptions.Register("demofeature");
            })
            // Select any keys based on the host environment.
            .Select(
                KeyFilter.Any, hostingContext.HostingEnvironment.EnvironmentName
            );
    });
});

builder.Services.AddRazorPages();

// Add Azure App Configuration and feature flag services.
builder.Services.AddAzureAppConfiguration();
builder.Services.AddFeatureManagement();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Make sure we use the Azure App Configuration in the pipeline for refreshing.
app.UseAzureAppConfiguration();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
