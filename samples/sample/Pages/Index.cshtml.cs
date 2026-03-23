using ZeyticAuth.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace sample.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        var zeyticOptions = HttpContext.GetZeyticAuthOptions();
        ViewData["Resource"] = zeyticOptions.Resource;
        ViewData["AccessTokenForResource"] = await HttpContext.GetTokenAsync(ZeyticAuthParameters.Tokens.AccessTokenForResource);
    }

    public async Task OnPostSignInAsync()
    {
        var authProperties = new AuthenticationProperties 
        { 
            RedirectUri = "/" 
        };

        /// <see href="https://docs.zeytic.com/docs/references/openid-connect/authentication-parameters/#first-screen"/>
        /// <see cref="ZeyticAuthParameters.Authentication.FirstScreen"/>
        authProperties.SetParameter("first_screen", ZeyticAuthParameters.Authentication.FirstScreen.Register);
        
        // This parameter MUST be used together with `first_screen`
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

        await HttpContext.ChallengeAsync(authProperties);
    }

    public async Task OnPostSignOutAsync()
    {
        await HttpContext.SignOutAsync(new AuthenticationProperties { RedirectUri = "/" });
    }
}
