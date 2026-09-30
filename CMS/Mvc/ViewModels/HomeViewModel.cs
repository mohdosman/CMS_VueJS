using System.Text.Json;
using CMS.Features.Menus;

namespace CMS.Mvc.ViewModels;

public sealed class HomeViewModel
{
    public string ApplicationName { get; init; } = "";
    public string UserName { get; init; } = "";
    public string FullName { get; init; } = "";
    public bool IsAdmin { get; init; }
    public bool IsADAccount { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
    public IReadOnlyList<string> Permissions { get; init; } = [];
    public IReadOnlyList<MenuNode> Menu { get; init; } = [];

    // globals, same idea as SafetyNet: url roots (correct under a virtual directory) and the
    // antiforgery request token the SPA sends as X-XSRF-TOKEN on every write.
    public string BaseUrl { get; init; } = "";
    public string WebApiBaseUrl { get; init; } = "/api";
    public string AntiforgeryToken { get; init; } = "";

    // Read by vue-app/src/boot.js from <script id="__cms_boot__">. "<" is escaped so
    // the blob can't close its own script tag. routes = the menu urls this user may open.
    public string ToBootstrapJson() =>
        JsonSerializer.Serialize(
            new
            {
                applicationName = ApplicationName,
                globals = new { baseUrl = BaseUrl, webApiBaseUrl = WebApiBaseUrl, antiforgeryToken = AntiforgeryToken },
                currentUser = new { userName = UserName, fullName = FullName, isAdmin = IsAdmin, roles = Roles, permissions = Permissions },
                // /change-password is the server-rendered Account/Change page, so it is not an SPA route.
                routes = MenuService.Flatten(Menu).Where(n => n.Url != "" && n.Url != "/change-password").Select(n => new { url = n.Url, name = n.Name })
            },
            new JsonSerializerOptions(JsonSerializerDefaults.Web)).Replace("<", "\\u003c");
}
