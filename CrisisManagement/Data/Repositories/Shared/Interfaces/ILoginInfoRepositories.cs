using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories.Interfaces;

namespace CrisisManagement.Data.Repositories.Interfaces;

// What the sign-in page shows before anyone is signed in.
public interface INotificationRepository : IRepository<AppNotification>
{
    // Newest first.
    Task<List<AppNotification>> GetLatestAsync(int take);
}

public interface ISupportRepository : IRepository<AppSupport>
{
    // The designated support people with their contact details, in id order.
    Task<List<AppSupport>> GetWithContactsAsync(int take);
}
