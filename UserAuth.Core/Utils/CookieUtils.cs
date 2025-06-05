namespace UserAuth.Core.Utils;

public class CookieUtils
{

    // Method for setting up cookie
    public static void SetJwtCookie(HttpResponse response, string token, bool isPersistent = false)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = isPersistent ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddMinutes(60), 
        };
        response.Cookies.Append("auth_token", token, cookieOptions);
    }


    // Method for clearing cookie
    public static void ClearCookies(HttpResponse response)
    {
       response.Cookies.Delete("auth_token");
    }
}
