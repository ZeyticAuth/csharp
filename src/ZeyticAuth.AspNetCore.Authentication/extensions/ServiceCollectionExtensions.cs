using System;
using Microsoft.Extensions.DependencyInjection;

namespace ZeyticAuth.AspNetCore.Authentication;

/// <summary>
/// The ZeyticAuth extension methods for ServiceCollectionExtensions.
/// </summary>
public static class ServiceCollectionExtensions
{
  /// <summary>
  /// Add ZeyticAuth authentication services to the specified <see cref="IServiceCollection" />.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
  /// <param name="configureOptions">A delegate to configure <see cref="ZeyticAuthOptions"/>.</param>
  /// <returns>A reference to this instance after the operation has completed.</returns>
  public static IServiceCollection AddZeyticAuthAuthentication(this IServiceCollection services, Action<ZeyticAuthOptions> configureOptions)
  {
    services.AddZeyticAuthAuthentication(ZeyticAuthDefaults.AuthenticationScheme, configureOptions);

    return services;
  }

  /// <summary>
  /// Add ZeyticAuth authentication services to the specified <see cref="IServiceCollection" />.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
  /// <param name="authenticationScheme">The authentication scheme.</param>
  /// <param name="configureOptions">A delegate to configure <see cref="ZeyticAuthOptions"/>.</param>
  /// <returns>A reference to this instance after the operation has completed.</returns>
  public static IServiceCollection AddZeyticAuthAuthentication(this IServiceCollection services, string authenticationScheme, Action<ZeyticAuthOptions> configureOptions)
  {
    services.AddZeyticAuthAuthentication(authenticationScheme, ZeyticAuthDefaults.CookieScheme, configureOptions);

    return services;
  }

  /// <summary>
  /// Add ZeyticAuth authentication services to the specified <see cref="IServiceCollection" />.
  /// </summary>
  /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
  /// <param name="authenticationScheme">The authentication scheme.</param>
  /// <param name="cookieScheme">The cookie scheme.</param>
  /// <param name="configureOptions">A delegate to configure <see cref="ZeyticAuthOptions"/>.</param>
  /// <returns>A reference to this instance after the operation has completed.</returns>
  public static IServiceCollection AddZeyticAuthAuthentication(this IServiceCollection services, string authenticationScheme, string cookieScheme, Action<ZeyticAuthOptions> configureOptions)
  {
    services
      .AddAuthentication(options =>
      {
        options.DefaultScheme = cookieScheme;
        options.DefaultChallengeScheme = authenticationScheme;
        options.DefaultSignOutScheme = authenticationScheme;
      })
      .AddZeyticAuth(authenticationScheme, cookieScheme, configureOptions);

    return services;
  }
}
