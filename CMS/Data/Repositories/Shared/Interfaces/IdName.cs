using CMS.Data.Repositories.Interfaces;

namespace CMS.Data.Repositories.Interfaces;

// Small id/name projection for lookups; the service maps it to its own view model.
public sealed record IdName(int Id, string Name);
