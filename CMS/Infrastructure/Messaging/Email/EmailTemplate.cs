namespace CMS.Infrastructure.Messaging.Email;

// Loads an HTML email body from the EmailTemplates folder next to the app and fills {{Placeholder}} tokens.
public static class EmailTemplate
{
    private static readonly string Folder = Path.Combine(AppContext.BaseDirectory, "EmailTemplates");

    public static async Task<string> RenderAsync(string name, IReadOnlyDictionary<string, string> values, CancellationToken ct = default)
    {
        // ponytail: read per send, no cache: these go out rarely and copy edits should not need a restart.
        var body = await File.ReadAllTextAsync(Path.Combine(Folder, $"{name}.html"), ct);

        // ponytail: no HTML escaping, every value passed today is app-generated. Escape here if user input ever lands in a template.
        foreach (var (key, value) in values)
            body = body.Replace($"{{{{{key}}}}}", value);

        return body;
    }
}
