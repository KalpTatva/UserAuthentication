namespace UserAuth.Service.Utils;

public class EmailBody
{
    public string GetEmailBody(string userName, string resetLink)
    {
        return $@"
            <html>
            <body>
                <h1>Password Reset Request</h1>
                <p>Dear {userName},</p>
                <p>We received a request to reset your password. Please click the link below to reset your password:</p>
                <a href='{resetLink}'>Reset Password</a>
                <p>If you did not request this change, please ignore this email.</p>
                <p>Thank you!</p>
            </body>
            </html>";
    }
}