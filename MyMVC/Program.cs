using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using MyWeb.Application.DTOs.Common;
using MyWeb.Application.Security;
using MyWeb.Data.Seed;
using MyWeb.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllersWithViews();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Data Protection keys 目前落在專案 App_Data，正式部署多台主機時需改成共享且受保護的 key ring。
// TODO: 依部署環境改用受控儲存體或 Key Vault，並規劃備份與輪替。
var dataProtectionKeyDirectory = Path.Combine(
    builder.Environment.ContentRootPath,
    "App_Data",
    "DataProtectionKeys");
Directory.CreateDirectory(dataProtectionKeyDirectory);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeyDirectory));

var configuredOrigins = builder.Configuration
    .GetSection("Security:Cors:AllowedOrigins")
    .GetChildren()
    .Select(section => section.Value)
    .Where(value => !string.IsNullOrWhiteSpace(value))
    .Select(value => value!)
    .ToArray();

var allowedOrigins = configuredOrigins.Length > 0
    ? configuredOrigins
    : ["http://localhost:5173", "https://localhost:5173", "http://localhost:5174", "https://localhost:5174"];

// 正式環境必須由組態白名單指定來源；localhost fallback 只服務本機 Vue/Razor 開發流程。
// 開發環境允許 SameAsRequest；正式環境使用 Secure Cookie，符合 HttpOnly Secure Cookie 的登入策略。
var cookieSecurePolicy = builder.Environment.IsDevelopment()
    ? CookieSecurePolicy.SameAsRequest
    : CookieSecurePolicy.Always;
var authCookieName = builder.Environment.IsDevelopment()
    ? "MySystemAdminAuth"
    : "__Host-MySystemAdminAuth";
var csrfCookieName = builder.Environment.IsDevelopment()
    ? "MySystemCsrf"
    : "__Host-MySystemCsrf";

builder.Services.AddCors(options =>
{
    options.AddPolicy("TrustedFrontend", policy =>
    {
        // TODO: 正式環境 AllowedOrigins 必須由組態明確指定，不可依賴 localhost fallback。
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddAntiforgery(options =>
{
    // CSRF token 透過 Header 傳送；Cookie 設為 HttpOnly，前端需由安全 API 取得可送出的 token。
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = csrfCookieName;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = cookieSecurePolicy;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = authCookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = cookieSecurePolicy;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.Path = "/";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);

        options.Events.OnRedirectToLogin = context =>
        {
            // API 不導向登入頁，避免前端收到 HTML；頁面請求才導向對應登入頁。
            if (IsApiRequest(context.Request))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }

            var loginPath = context.Request.Path.StartsWithSegments("/admin", StringComparison.OrdinalIgnoreCase)
                ? "/admin/login"
                : "/account/login";

            context.Response.Redirect(loginPath);
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (IsApiRequest(context.Request))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return context.Response.WriteAsync("沒有權限存取此管理頁面。");
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        // 同時接受新舊 Claim Type，讓既有 Cookie 在 Claim 常數化過程中仍可被辨識。
        policy.RequireAssertion(context =>
            context.User.HasClaim("user_type", "Admin")
            || context.User.HasClaim("user_type", "Staff")
            || context.User.HasClaim("UserType", "Admin")
            || context.User.HasClaim("UserType", "Staff"));
    });

    foreach (var permission in AdminPermissionCodes.All)
    {
        // 每個後台 Permission 都註冊成 Policy；Controller Attribute 是入口保護，Service 仍需再檢查。
        options.AddPolicy(permission, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireAssertion(context =>
                IsAdminUser(context.User)
                && HasPermission(context.User, permission));
        });
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.SeedAdminAuthAsync();
    await app.SeedStorefrontCatalogAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("TrustedFrontend");

app.UseAuthentication();

app.UseAuthorization();

// 針對 API 的狀態變更動作做 CSRF 驗證；Controller 保持乾淨，安全管線集中在啟動設定。
app.Use(async (context, next) =>
{
    if (RequiresCsrfValidation(context))
    {
        var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();

        try
        {
            await antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(
                ApiResponse.Failure("CSRF_VALIDATION_FAILED", "CSRF 驗證失敗，請重新取得安全權杖後再送出。"));
            return;
        }
    }

    await next();
});

app.MapStaticAssets();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

static bool RequiresCsrfValidation(HttpContext context)
{
    // 目前只保護 /api 的狀態變更方法；Razor Form 若加入 POST，需搭配 ValidateAntiForgeryToken 或擴充此規則。
    if (!context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    if (context.Request.Path.StartsWithSegments("/api/security/csrf-token", StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    return HttpMethods.IsPost(context.Request.Method)
        || HttpMethods.IsPut(context.Request.Method)
        || HttpMethods.IsPatch(context.Request.Method)
        || HttpMethods.IsDelete(context.Request.Method);
}

static bool IsApiRequest(HttpRequest request)
{
    return request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
}

static bool IsAdminUser(System.Security.Claims.ClaimsPrincipal user)
{
    // 後台身分由伺服器簽發的 Cookie Claim 判斷，不能信任前端傳入的角色或權限欄位。
    return user.HasClaim(AdminClaimTypes.UserType, "Admin")
        || user.HasClaim(AdminClaimTypes.UserType, "Staff")
        || user.HasClaim("UserType", "Admin")
        || user.HasClaim("UserType", "Staff");
}

static bool HasPermission(System.Security.Claims.ClaimsPrincipal user, string permission)
{
    // Permission Claim 以精確比對處理，避免大小寫或部分字串比對造成越權。
    return user.Claims.Any(claim =>
        string.Equals(claim.Type, AdminClaimTypes.Permission, StringComparison.OrdinalIgnoreCase)
        && string.Equals(claim.Value, permission, StringComparison.OrdinalIgnoreCase));
}
