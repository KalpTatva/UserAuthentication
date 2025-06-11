namespace UserAuth.Core.Utils;

public class CookieUtils
{
    ///<summary>
    /// Method for setting up cookie
    ///</summary>
    ///<param name="httpContext"></param>
    ///<param name="token"></param>
    ///<param name="isPersistent"></param>
    public static void SetJwtCookie(HttpResponse response, string token, bool isPersistent = false)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = isPersistent ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddDays(1), 
        };
        response.Cookies.Append("auth_token", token, cookieOptions);
    }


    ///<summary>
    /// Method for clearing cookie
    ///</summary>
    ///<param name="httpContext"></param>
    public static void ClearCookies(HttpResponse response)
    {
       response.Cookies.Delete("auth_token");
    }
}
