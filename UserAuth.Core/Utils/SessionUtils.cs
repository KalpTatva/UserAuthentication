namespace UserAuth.Core.Utils;

public class SessionUtils
{

    ///<summary>
    /// method for getting timer at 2FA auth timer for 5 min
    ///</summary>
    ///<param name="httpContext"></param>
    public static bool IsTimerAvailable(HttpContext httpContext)
    {
        string? data = httpContext.Session.GetString("CountdownStartTime");
        if(string.IsNullOrEmpty(data))
        {
            return false;
        }
        return true;
    }

    ///<summary>
    /// method for setting timer into session for 2FA auth
    /// </summary>
    ///<param name="httpContext"></param>
    ///<param name="Time"></param>
    public static void SetTimer(HttpContext httpContext, string time)
    {
        httpContext.Session.SetString("CountdownStartTime", time);
    }

    ///<summary>
    /// method for getting timer info from session for 2FA auth
    /// </summary>
    ///<param name="httpContext"></param>
    public static string? GetTimer(HttpContext httpContext)
    {
        return httpContext.Session.GetString("CountdownStartTime");
    }

    ///<summary>
    /// method for remove timer info from session for 2FA auth
    /// </summary>
    ///<param name="httpContext"></param>
    public static void RemoveTimer(HttpContext httpContext)
    {
        httpContext.Session.Remove("CountdownStartTime");
    }

    ///<summary>
    /// method for get the remaining time for user to get login
    /// basically how much time is remain to get login using auth otp
    /// </summary>
    ///<param name="httpContext"></param>
    public static double GetRemainingTimer(HttpContext httpContext)
    {
        string? res = httpContext.Session.GetString("CountdownStartTime");
        DateTime countdownStartTime = res != null ? DateTime.Parse(res) : DateTime.UtcNow;

        double elapsedSeconds = (DateTime.UtcNow - countdownStartTime).TotalSeconds;

        double remainingTime = Math.Max(300 - (int)elapsedSeconds, 0); // 5 minutes in seconds
        
        if (remainingTime <= 0)
        {
            RemoveTimer(httpContext);
        } 

        return remainingTime;
    }
 
    

}
