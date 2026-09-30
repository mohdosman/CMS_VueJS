using CrisisManagement.Data.Context;
using CrisisManagement.Shared.Extensions;

namespace CrisisManagement.Data;

// Ported from SafetyNet: the registered IUnitOfWork. It tells the context who is signed in, so the
// audit columns (CreatedBy / UpdatedBy) are stamped with that user. SafetyNet reads the user once in the
// constructor; here it is read at save time (see AppDbContext.CurrentUserIdResolver).
public class HttpUnitOfWork : UnitOfWork
{
    public HttpUnitOfWork(AppDbContext context, IHttpContextAccessor httpAccessor) : base(context)
    {
        context.CurrentUserIdResolver = () =>
            httpAccessor.HttpContext?.User is { Identity.IsAuthenticated: true } user ? user.GetUserId() : 0;
    }
}
