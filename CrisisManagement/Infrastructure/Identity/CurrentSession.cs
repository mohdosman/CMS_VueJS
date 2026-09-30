namespace CrisisManagement.Infrastructure.Identity;

// The browser session the signed-in user is working in, as an id kept in a session cookie. Services store it so the
// Enter Service screen can list what was entered in this session. It is cleared at sign-out.
public sealed class CurrentSession(IHttpContextAccessor http)
{
    public const string CookieName = "cms.entry";
    private string? _id;

    // Always a 32-digit hex id: it goes into a search procedure, so nothing else is ever accepted from the cookie.
    public string Id
    {
        get
        {
            if (_id is not null) return _id;
            var ctx = http.HttpContext!;
            if (ctx.Request.Cookies.TryGetValue(CookieName, out var v) && Guid.TryParseExact(v, "N", out _)) return _id = v;
            _id = Guid.NewGuid().ToString("N");
            ctx.Response.Cookies.Append(CookieName, _id, new CookieOptions { HttpOnly = true, Secure = ctx.Request.IsHttps, SameSite = SameSiteMode.Strict, IsEssential = true });
            return _id;
        }
    }
}
