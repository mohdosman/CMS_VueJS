namespace CrisisManagement.Shared.Common;

// Collects field errors (camelCase keys, several messages per field) and throws them as one ValidationFailedException,
// which the API answers as a 400 the SPA shows next to the inputs.
public sealed class ErrorBag
{
    private readonly Dictionary<string, List<string>> _errors = new();

    public bool Any => _errors.Count > 0;
    public bool Has(string key) => _errors.ContainsKey(key);

    public void Add(string key, string message)
    {
        if (!_errors.TryGetValue(key, out var list)) _errors[key] = list = [];
        list.Add(message);
    }

    // Trimmed value; adds "<label> is required." when blank and "<label> cannot exceed N characters." when too long.
    public string Required(string key, string label, string? value, int max)
    {
        var v = value?.Trim() ?? "";
        if (v.Length == 0) Add(key, $"{label} is required.");
        else if (v.Length > max) Add(key, $"{label} cannot exceed {max} characters.");
        return v;
    }

    // Trimmed value, "" when blank; only the length is checked.
    public string Optional(string key, string label, string? value, int max)
    {
        var v = value?.Trim() ?? "";
        if (v.Length > max) Add(key, $"{label} cannot exceed {max} characters.");
        return v;
    }

    public void ThrowIfAny()
    {
        if (Any) throw new ValidationFailedException(_errors.ToDictionary(e => e.Key, e => e.Value.ToArray()));
    }
}
