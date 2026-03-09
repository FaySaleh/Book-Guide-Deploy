public static class EmailTemplates
{
    public static string ResetPasswordHtml(string appName, string logoUrl, string userName, string resetUrl)
    {
        var safeUrl = System.Net.WebUtility.HtmlEncode(resetUrl);
        var safeName = System.Net.WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(userName) ? "there" : userName);

        var html = @"<!doctype html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Reset your password</title>
</head>
<body style=""margin:0; padding:0; background:#f5f7fb; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Arial, sans-serif; color:#0f172a;"">
  <div style=""display:none; max-height:0; overflow:hidden; opacity:0; color:transparent;"">
    Reset your password for {{APP_NAME}}.
  </div>

  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""background:#f5f7fb; padding:24px 12px;"">
    <tr><td align=""center"">
      <table role=""presentation"" width=""600"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""width:600px; max-width:100%;"">
        <tr>
          <td style=""padding:10px 8px 18px;"">
            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
              <tr>
                <td align=""left"" style=""vertical-align:middle;"">
                  <img src=""{{LOGO_URL}}"" alt=""{{APP_NAME}}"" width=""44"" height=""44"" style=""display:block; width:44px; height:44px; border-radius:10px;"" />
                </td>
                <td align=""left"" style=""padding-left:12px; vertical-align:middle;"">
                  <div style=""font-size:16px; font-weight:800; letter-spacing:0.2px;"">{{APP_NAME}}</div>
                </td>
              </tr>
            </table>
          </td>
        </tr>

        <tr>
          <td style=""background:#ffffff; border:1px solid #e6eaf2; border-radius:16px; overflow:hidden; box-shadow:0 10px 24px rgba(15,23,42,0.08);"">
            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
              <tr>
                <td style=""padding:26px 22px 10px;"">
                  <h1 style=""margin:0; font-size:22px; line-height:1.25; letter-spacing:-0.2px;"">Reset your password</h1>
                  <p style=""margin:12px 0 0; font-size:14px; line-height:1.6; color:#334155;"">
                    Hi {{USER_NAME}},<br />
                    We received a request to reset your password.
                  </p>
                </td>
              </tr>

              <tr>
                <td style=""padding:18px 22px 10px;"">
                  <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"">
                    <tr>
                      <td align=""center"" style=""border-radius:12px; background:#B0E0E6;"">
                        <a href=""{{RESET_URL}}"" style=""display:inline-block; padding:14px 18px; font-size:14px; font-weight:800; color:#ffffff; text-decoration:none; border-radius:12px;"">
                          Reset password
                        </a>
                      </td>
                    </tr>
                  </table>

                  <p style=""margin:14px 0 0; font-size:12.5px; line-height:1.6; color:#64748b;"">
                    If you didn’t request this, you can safely ignore this email.<br />
                    This link will expire soon for your security.
                  </p>
                </td>
              </tr>

              <tr><td style=""padding:10px 22px;""><div style=""height:1px; background:#eef2f7;""></div></td></tr>


            </table>
          </td>
        </tr>



      </table>
    </td></tr>
  </table>
</body>
</html>";

        return html
            .Replace("{{APP_NAME}}", appName)
            .Replace("{{LOGO_URL}}", logoUrl)
            .Replace("{{USER_NAME}}", safeName)
            .Replace("{{RESET_URL}}", safeUrl);
    }

    public static string ResetPasswordText(string appName, string userName, string resetUrl)
        => $"Hi {userName},\n\nWe received a request to reset your {appName} password.\n\nReset link:\n{resetUrl}\n\nIf you didn’t request this, ignore this email.\n\n- {appName} Team";
}
