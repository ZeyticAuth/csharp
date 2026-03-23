namespace ZeyticAuth.AspNetCore.Authentication;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Extension methods to configure ZeyticAuth authentication.
/// </summary>
/// <remarks>
/// <para>
/// ZeyticAuth is an identity layer on top of the Open ID Connect protocol. This extension leverages the Open ID Connect
/// authentication handler but adds additional functionality to support ZeyticAuth's requirements.
/// </para>
/// </remarks>
public static class AuthenticationBuilderExtensions
{
  /// <summary>
  /// Adds ZeyticAuth authentication to <see cref="AuthenticationBuilder"/> using the default scheme.
  /// The default scheme is specified by <see cref="ZeyticAuthDefaults.AuthenticationScheme"/>, which is "ZeyticAuth";
  /// The default cookie scheme is specified by <see cref="ZeyticAuthDefaults.CookieScheme"/>, which is "ZeyticAuth.Cookie";
  /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
  /// <param name="configureOptions">A delegate to configure <see cref="ZeyticAuthOptions"/>.</param>
  /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
  public static AuthenticationBuilder AddZeyticAuth(this AuthenticationBuilder builder, Action<ZeyticAuthOptions> configureOptions)
      => builder.AddZeyticAuth(ZeyticAuthDefaults.AuthenticationScheme, configureOptions);


  /// <summary>
  /// Adds ZeyticAuth authentication to <see cref="AuthenticationBuilder"/> using the specified scheme.
  /// The default cookie scheme is specified by <see cref="ZeyticAuthDefaults.CookieScheme"/>, which is "ZeyticAuth.Cookie";
  /// </summary>
  /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
  /// <param name="authenticationScheme">The authentication scheme.</param>
  /// <param name="configureOptions">A delegate to configure <see cref="ZeyticAuthOptions"/>.</param>
  /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
  public static AuthenticationBuilder AddZeyticAuth(this AuthenticationBuilder builder, string authenticationScheme, Action<ZeyticAuthOptions> configureOptions)
      => builder.AddZeyticAuth(authenticationScheme, ZeyticAuthDefaults.CookieScheme, configureOptions);

  /// <summary>
  /// Adds ZeyticAuth authentication to <see cref="AuthenticationBuilder"/> using the specified scheme and cookie scheme.
  /// </summary>
  /// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
  /// <param name="authenticationScheme">The authentication scheme.</param>
  /// <param name="cookieScheme">The cookie scheme.</param>
  /// <param name="configureOptions">A delegate to configure <see cref="ZeyticAuthOptions"/>.</param>
  /// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
  public static AuthenticationBuilder AddZeyticAuth(this AuthenticationBuilder builder, string authenticationScheme, string cookieScheme, Action<ZeyticAuthOptions> configureOptions)
  {
    var zeyticOptions = new ZeyticAuthOptions();
    configureOptions(zeyticOptions);

    builder.Services.Configure(authenticationScheme, configureOptions);
    builder.Services
      .AddOptions<CookieAuthenticationOptions>(cookieScheme)
      .Configure((options) => ConfigureCookieOptions(authenticationScheme, options, zeyticOptions));
    builder.AddCookie(cookieScheme);
    builder.AddOpenIdConnect(authenticationScheme, oidcOptions => ConfigureOpenIdConnectOptions(oidcOptions, zeyticOptions, cookieScheme));

    return builder;
  }

  /// <summary>
  /// Configures the cookie options for ZeyticAuth authentication. This method will mutate the `options` parameter.
  /// </summary>
  private static void ConfigureCookieOptions(string authenticationScheme, CookieAuthenticationOptions options, ZeyticAuthOptions zeyticOptions)
  {
    options.Cookie.Name = $"ZeyticAuth.Cookie.{zeyticOptions.AppId}";
    options.SlidingExpiration = true;
    options.Cookie.Domain = zeyticOptions.CookieDomain;
    options.Events = new CookieAuthenticationEvents
    {
      OnValidatePrincipal = context => new ZeyticAuthCookieContextManager(authenticationScheme, context).Handle()
    };
  }

  /// <summary>
  /// Configures the OpenID Connect options for ZeyticAuth authentication. This method will mutate the `options` parameter.
  /// </summary>
  /// <param name="options">The OpenID Connect options to configure.</param>
  /// <param name="zeyticOptions">The ZeyticAuth options to use for configuration.</param>
  private static void ConfigureOpenIdConnectOptions(OpenIdConnectOptions options, ZeyticAuthOptions zeyticOptions, string cookieScheme)
  {
    options.Authority = zeyticOptions.Endpoint + "oidc";
    options.ClientId = zeyticOptions.AppId;
    options.ClientSecret = zeyticOptions.AppSecret;
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;
    options.UsePkce = true;
    options.Prompt = zeyticOptions.Prompt;
    options.CallbackPath = new PathString(zeyticOptions.CallbackPath);
    options.SignedOutCallbackPath = new PathString(zeyticOptions.SignedOutCallbackPath);
    options.GetClaimsFromUserInfoEndpoint = zeyticOptions.GetClaimsFromUserInfoEndpoint;
    options.MapInboundClaims = false;
    options.ClaimActions.MapAllExcept("nbf", "nonce", "c_hash", "at_hash");
    options.Events = new OpenIdConnectEvents
    {
      OnRedirectToIdentityProvider = context =>
      {
        if (context.Properties.Parameters.TryGetValue("first_screen", out var firstScreen))
        {
          context.ProtocolMessage.Parameters.Add("first_screen", firstScreen?.ToString());
        }

        if (context.Properties.Parameters.TryGetValue("identifiers", out var identifiers))
        {
          context.ProtocolMessage.Parameters.Add("identifiers", identifiers?.ToString());
        }

        if (context.Properties.Parameters.TryGetValue("direct_sign_in", out var directSignIn))
        {
          var directSignInOption = System.Text.Json.JsonSerializer.Deserialize<ZeyticAuthParameters.Authentication.DirectSignIn>(
            directSignIn?.ToString() ?? "{}"
          );
          if (directSignInOption != null && !string.IsNullOrEmpty(directSignInOption.Method) && !string.IsNullOrEmpty(directSignInOption.Target))
          {
            context.ProtocolMessage.Parameters.Add("direct_sign_in", $"{directSignInOption.Method}:{directSignInOption.Target}");
          }
        }

        if (context.Properties.Parameters.TryGetValue("extra_params", out var extraParams))
        {
          var parameters = System.Text.Json.JsonSerializer.Deserialize<ZeyticAuthParameters.Authentication.ExtraParams>(
            extraParams?.ToString() ?? "{}"
          );
          if (parameters != null)
          {
            foreach (var param in parameters)
            {
              context.ProtocolMessage.Parameters.Add(param.Key, param.Value);
            }
          }
        }

        return Task.CompletedTask;
      },
      OnRedirectToIdentityProviderForSignOut = async context =>
      {
        // Clean up the cookie when signing out.
        await context.HttpContext.SignOutAsync(cookieScheme);
        // Rebuild parameters since we use <c>client_id</c> for sign-out, no need to use <c>id_token_hint</c>.
        context.ProtocolMessage.Parameters.Remove(OpenIdConnectParameterNames.IdTokenHint);
        context.ProtocolMessage.Parameters.Add(OpenIdConnectParameterNames.ClientId, zeyticOptions.AppId);
      },
    };
    options.TokenValidationParameters = new TokenValidationParameters
    {
      NameClaimType = "name",
      RoleClaimType = "roles",
      ValidateAudience = true,
      ValidAudience = zeyticOptions.AppId,
      ValidateIssuer = true,
      ValidIssuer = zeyticOptions.Endpoint + "oidc",
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,
    };

    // Handle scopes
    var scopes = new HashSet<string>(zeyticOptions.Scopes)
      {
          "openid",
          "offline_access",
          "profile"
      };

    options.Scope.Clear();
    foreach (var scope in scopes)
    {
      options.Scope.Add(scope);
    }

    // Handle resource
    if (!string.IsNullOrEmpty(zeyticOptions.Resource))
    {
      options.Resource = zeyticOptions.Resource;
    }
  }
}
