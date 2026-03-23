using ZeyticAuth.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// This configuration only applies to the local HTTP development environment.
// Production deployments should use a more secure configuration.
static void CheckSameSite(HttpContext httpContext, CookieOptions options)
{
    if (options.SameSite == SameSiteMode.None && options.Secure == false)
    {
        options.SameSite = SameSiteMode.Unspecified;
    }
}

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    options.OnAppendCookie = cookieContext => CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
    options.OnDeleteCookie = cookieContext => CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
});

builder.Services.AddZeyticAuthAuthentication(options =>
{
    options.Endpoint = builder.Configuration["ZeyticAuth:Endpoint"]!;
    options.AppId = builder.Configuration["ZeyticAuth:AppId"]!;
    options.AppSecret = builder.Configuration["ZeyticAuth:AppSecret"];
    options.Scopes = new string[] {
        ZeyticAuthParameters.Scopes.Email,
        ZeyticAuthParameters.Scopes.Phone,
        ZeyticAuthParameters.Scopes.CustomData,
        ZeyticAuthParameters.Scopes.Identities
    };
    options.Resource = builder.Configuration["ZeyticAuth:Resource"];
    options.GetClaimsFromUserInfoEndpoint = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseCookiePolicy();

app.UseAuthentication();
// app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Use(async (context, next) =>
{
    Console.Write("Request: ");
    Console.Write(context.Request.Method + " ");
    Console.Write(context.Request.Path);
    Console.WriteLine();

    await next.Invoke();
});

app.Run();
