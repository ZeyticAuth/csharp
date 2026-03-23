using ZeyticAuth.AspNetCore.Authentication;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using sample_mvc.Models;
using System.Text.Json;

namespace sample_mvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var zeyticOptions = HttpContext.GetZeyticAuthOptions();
        ViewData["Resource"] = zeyticOptions.Resource;
        ViewData["AccessTokenForResource"] = await HttpContext.GetTokenAsync(ZeyticAuthParameters.Tokens.AccessTokenForResource);
        return View();
    }

    public IActionResult SignIn()
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
        authProperties.SetParameter("direct_sign_in", JsonSerializer.Serialize(directSignIn));

        return Challenge(authProperties);
    }

    // Use the `new` keyword to avoid conflict with the `ControllerBase.SignOut` method
    new public IActionResult SignOut()
    {
        return SignOut(new AuthenticationProperties { RedirectUri = "/" });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
