using MyWeb.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddProblemDetails();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddTrustedFrontendCors(builder.Configuration, builder.Environment);
builder.Services.AddApplicationCookieAuthentication();
builder.Services.AddApplicationAuthorization();
builder.Services.AddApplicationAntiforgery();
builder.Services.AddApplicationRateLimiting();
builder.Services.AddApplicationHealthChecks();

var app = builder.Build();

app.UseApiExceptionHandler(app.Environment);
app.UseApplicationForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSecurityHeaders();
app.UseRouting();
app.UseCors(CorsExtensions.DefaultCorsPolicyName);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseApplicationAntiforgeryValidation();

app.MapStaticAssets();
app.MapControllers();
app.MapApplicationHealthChecks();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
