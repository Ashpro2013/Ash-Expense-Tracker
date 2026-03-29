using ExpenseIncomeTracker.Infrastructure;
using ExpenseIncomeTracker.Infrastructure.Identity;
using ExpenseIncomeTracker.Infrastructure.Persistence;
using ExpenseIncomeTracker.Web.Components;
using ExpenseIncomeTracker.Web.Persistence;
using ExpenseIncomeTracker.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
builder.Services.AddHttpClient();
builder.Services.AddFluentUIComponents();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddDbContext<WebAppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<DiaryService>();
builder.Services.AddScoped<ActivityService>();
builder.Services.AddScoped<FinanceEntryService>();
builder.Services.AddScoped<DayPlanService>();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/account/login", async (
    HttpRequest httpRequest,
    SignInManager<ApplicationUser> signInManager) =>
{
    if (!httpRequest.HasFormContentType)
    {
        var error = Uri.EscapeDataString("Invalid form submission.");
        return Results.Redirect($"/login?error={error}");
    }

    var form = await httpRequest.ReadFormAsync();
    var email = form["Email"].ToString();
    var password = form["Password"].ToString();
    var rememberRaw = form["RememberMe"].ToString();
    var returnUrl = form["returnUrl"].ToString();

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    {
        var error = Uri.EscapeDataString("Email and password are required.");
        return Results.Redirect($"/login?error={error}");
    }

    var rememberMe = rememberRaw.Equals("true", StringComparison.OrdinalIgnoreCase)
        || rememberRaw.Equals("on", StringComparison.OrdinalIgnoreCase)
        || rememberRaw.Equals("1", StringComparison.OrdinalIgnoreCase);

    var result = await signInManager.PasswordSignInAsync(
        email,
        password,
        rememberMe,
        lockoutOnFailure: false);

    if (result.Succeeded)
    {
        var target = string.IsNullOrWhiteSpace(returnUrl) || returnUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? "/dashboard"
            : returnUrl;
        return Results.Redirect(target);
    }

    var invalid = Uri.EscapeDataString("Invalid email or password.");
    return Results.Redirect($"/login?error={invalid}");
}).DisableAntiforgery();


app.MapPost("/account/login-json", async (
    LoginJsonRequest request,
    SignInManager<ApplicationUser> signInManager) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.Json(new { ok = false, error = "Email and password are required." }, statusCode: 400);
    }

    var result = await signInManager.PasswordSignInAsync(
        request.Email,
        request.Password,
        request.RememberMe,
        lockoutOnFailure: false);

    if (!result.Succeeded)
    {
        return Results.Json(new { ok = false, error = "Invalid email or password." }, statusCode: 401);
    }

    var target = string.IsNullOrWhiteSpace(request.ReturnUrl) || request.ReturnUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
        ? "/dashboard"
        : request.ReturnUrl;

    return Results.Json(new { ok = true, redirect = target });
}).DisableAntiforgery();

app.MapPost("/account/register", async (
    HttpRequest httpRequest,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) =>
{
    if (!httpRequest.HasFormContentType)
    {
        var error = Uri.EscapeDataString("Invalid form submission.");
        return Results.Redirect($"/register?error={error}");
    }

    var form = await httpRequest.ReadFormAsync();
    var email = form["Email"].ToString();
    var password = form["Password"].ToString();
    var confirmPassword = form["ConfirmPassword"].ToString();

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
    {
        var error = Uri.EscapeDataString("All fields are required.");
        return Results.Redirect($"/register?error={error}");
    }

    if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
    {
        var mismatch = Uri.EscapeDataString("Passwords do not match.");
        return Results.Redirect($"/register?error={mismatch}");
    }

    var user = new ApplicationUser
    {
        UserName = email,
        Email = email
    };

    var result = await userManager.CreateAsync(user, password);
    if (!result.Succeeded)
    {
        var error = Uri.EscapeDataString(string.Join(" ", result.Errors.Select(e => e.Description)));
        return Results.Redirect($"/register?error={error}");
    }

    await signInManager.SignInAsync(user, isPersistent: true);
    return Results.Redirect("/dashboard");
}).DisableAntiforgery();


app.MapPost("/account/register-json", async (
    RegisterJsonRequest request,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) =>
{
    if (string.IsNullOrWhiteSpace(request.Email)
        || string.IsNullOrWhiteSpace(request.Password)
        || string.IsNullOrWhiteSpace(request.ConfirmPassword))
    {
        return Results.Json(new { ok = false, error = "All fields are required." }, statusCode: 400);
    }

    if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
    {
        return Results.Json(new { ok = false, error = "Passwords do not match." }, statusCode: 400);
    }

    var user = new ApplicationUser
    {
        UserName = request.Email,
        Email = request.Email
    };

    var result = await userManager.CreateAsync(user, request.Password);
    if (!result.Succeeded)
    {
        var error = string.Join(" ", result.Errors.Select(e => e.Description));
        return Results.Json(new { ok = false, error }, statusCode: 400);
    }

    await signInManager.SignInAsync(user, isPersistent: true);
    return Results.Json(new { ok = true, redirect = "/dashboard" });
}).DisableAntiforgery();

app.MapPost("/account/logout", async (
    SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/login");
}).DisableAntiforgery();

using (var scope = app.Services.CreateScope())
{
    var dataDir = Path.Combine(app.Environment.ContentRootPath, "Data");
    Directory.CreateDirectory(dataDir);

    var identityDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    identityDb.Database.EnsureCreated();

    var webDb = scope.ServiceProvider.GetRequiredService<WebAppDbContext>();
    webDb.Database.EnsureCreated();
    webDb.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "FinanceEntries" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_FinanceEntries" PRIMARY KEY,
            "UserId" TEXT NOT NULL,
            "Type" INTEGER NOT NULL,
            "Title" TEXT NOT NULL,
            "Amount" TEXT NOT NULL,
            "Note" TEXT NULL,
            "EntryDate" TEXT NOT NULL,
            "CreatedAt" TEXT NOT NULL,
            "UpdatedAt" TEXT NULL
        );
    """);
    webDb.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "DiaryEntries" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_DiaryEntries" PRIMARY KEY,
            "UserId" TEXT NOT NULL,
            "Title" TEXT NOT NULL,
            "Content" TEXT NOT NULL,
            "EntryDate" TEXT NOT NULL,
            "Tags" TEXT NOT NULL,
            "Mood" INTEGER NOT NULL,
            "CreatedAt" TEXT NOT NULL,
            "UpdatedAt" TEXT NULL
        );
    """);
    webDb.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "ActivityItems" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_ActivityItems" PRIMARY KEY,
            "UserId" TEXT NOT NULL,
            "Title" TEXT NOT NULL,
            "Description" TEXT NULL,
            "Category" TEXT NULL,
            "StartDate" TEXT NULL,
            "DueDate" TEXT NULL,
            "Status" INTEGER NOT NULL,
            "IsImportant" INTEGER NOT NULL,
            "CreatedAt" TEXT NOT NULL,
            "CompletedAt" TEXT NULL
        );
    """);
    webDb.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS "DayPlanItems" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_DayPlanItems" PRIMARY KEY,
            "UserId" TEXT NOT NULL,
            "PlanDate" TEXT NOT NULL,
            "Title" TEXT NOT NULL,
            "StartTime" TEXT NOT NULL,
            "EndTime" TEXT NOT NULL,
            "Notes" TEXT NULL,
            "IsCompleted" INTEGER NOT NULL,
            "CreatedAt" TEXT NOT NULL,
            "UpdatedAt" TEXT NULL
        );
    """);
    webDb.Database.ExecuteSqlRaw("""
        CREATE INDEX IF NOT EXISTS "IX_FinanceEntries_UserId_EntryDate"
        ON "FinanceEntries" ("UserId", "EntryDate");
    """);
    webDb.Database.ExecuteSqlRaw("""
        CREATE INDEX IF NOT EXISTS "IX_DiaryEntries_UserId_EntryDate"
        ON "DiaryEntries" ("UserId", "EntryDate");
    """);
    webDb.Database.ExecuteSqlRaw("""
        CREATE INDEX IF NOT EXISTS "IX_ActivityItems_UserId_Status"
        ON "ActivityItems" ("UserId", "Status");
    """);
    webDb.Database.ExecuteSqlRaw("""
        CREATE INDEX IF NOT EXISTS "IX_DayPlanItems_UserId_PlanDate"
        ON "DayPlanItems" ("UserId", "PlanDate");
    """);
}

app.Run();

internal sealed record LoginJsonRequest(string Email, string Password, bool RememberMe, string? ReturnUrl);

internal sealed record RegisterJsonRequest(string Email, string Password, string ConfirmPassword);
