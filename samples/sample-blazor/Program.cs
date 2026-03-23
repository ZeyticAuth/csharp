using sample_blazor.Components;
using ZeyticAuth.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddZeyticAuthAuthentication(options =>
{
    options.Endpoint = builder.Configuration["ZeyticAuth:Endpoint"]!;
    options.AppId = builder.Configuration["ZeyticAuth:AppId"]!;
    options.AppSecret = builder.Configuration["ZeyticAuth:AppSecret"];
    options.Scopes = [
        ZeyticAuthParameters.Scopes.Email,
        ZeyticAuthParameters.Scopes.Phone,
        ZeyticAuthParameters.Scopes.CustomData,
        ZeyticAuthParameters.Scopes.Identities
    ];
    options.Resource = builder.Configuration["ZeyticAuth:Resource"];
    options.GetClaimsFromUserInfoEndpoint = true;
});
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/SignIn", async context =>
{
    if (!(context.User?.Identity?.IsAuthenticated ?? false))
    {
        var authProperties = new AuthenticationProperties 
        { 
            RedirectUri = "/" 
        };

        /// <see href="https://docs.zeytic.com/docs/references/openid-connect/authentication-parameters/#first-screen"/>
        /// <see cref="ZeyticAuthParameters.Authentication.FirstScreen"/>
        authProperties.SetParameter("first_screen", ZeyticAuthParameters.Authentication.FirstScreen.Register);
        
        // This parameter MUST be used together with `first_screen`.
        authProperties.SetParameter("identifiers", string.Join(",", new[] 
        {
            ZeyticAuthParameters.Authentication.Identifiers.Username,
        }));

        var directSignIn = new ZeyticAuthParameters.Authentication.DirectSignIn
        {
            Target = "github",
            Method = ZeyticAuthParameters.Authentication.DirectSignIn.Methods.Social
        };
        
        /// <see href="https://docs.zeytic.com/docs/references/openid-connect/authentication-parameters/#direct-sign-in"/>
        /// <see cref="ZeyticAuthParameters.Authentication.DirectSignIn"/>
        authProperties.SetParameter("direct_sign_in", System.Text.Json.JsonSerializer.Serialize(directSignIn));

        await context.ChallengeAsync(authProperties);
    } 
    else 
    {
        context.Response.Redirect("/");
    }
});

app.MapGet("/SignOut", async context =>
{
    if (context.User?.Identity?.IsAuthenticated ?? false)
    {
        await context.SignOutAsync(new AuthenticationProperties { RedirectUri = "/" });
    } else {
        context.Response.Redirect("/");
    }
});

app.Run();
