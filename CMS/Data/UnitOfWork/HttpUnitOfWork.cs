using CMS.Data.Context;
using CMS.Shared.Extensions;

namespace CMS.Data;

// Ported from SafetyNet: the registered IUnitOfWork. It tells the context who is signed in, so the
// audit columns (CreatedBy / UpdatedBy) are stamped with that user.
public class HttpUnitOfWork : UnitOfWork
{
    public HttpUnitOfWork(AppDbContext context, IHttpContextAccessor httpAccessor) : base(context)
    {
        var user = httpAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
            context.CurrentUserId = user.GetUserId();
    }
}
