using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ZeyticAuth.AspNetCore.Authentication;

public static class HttpContextExtensions
{
  /// <summary>
  /// Get the ZeyticAuth options from the <see cref="HttpContext"/>.
  /// </summary>
  /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
  /// <returns>The <see cref="ZeyticAuthOptions"/>.</returns>
  public static ZeyticAuthOptions GetZeyticAuthOptions(this HttpContext httpContext)
  {
    return httpContext.GetZeyticAuthOptions(ZeyticAuthDefaults.AuthenticationScheme);
  }

  /// <summary>
  /// Get the ZeyticAuth options from the <see cref="HttpContext"/>.
  /// </summary>
  /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
  /// <param name="authenticationScheme">The authentication scheme.</param>
  /// <returns>The <see cref="ZeyticAuthOptions"/>.</returns>
  public static ZeyticAuthOptions GetZeyticAuthOptions(this HttpContext httpContext, string authenticationScheme)
  {
    var options = httpContext.RequestServices.GetRequiredService<IOptionsMonitor<ZeyticAuthOptions>>().Get(authenticationScheme);
    if (options is null)
    {
      throw new InvalidOperationException($"No authentication scheme configured for {authenticationScheme}.");
    }

    return options;
  }
}
