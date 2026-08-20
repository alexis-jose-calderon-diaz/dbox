using Dbox.Database;
using Microsoft.EntityFrameworkCore;

namespace Dbox.Activities;

public sealed class ActivityRepository
{
    public async Task<Activity> AddAsync(DboxDbContext context, Activity activity, CancellationToken cancellationToken)
    {
        context.Activities.Add(activity);
        await context.SaveChangesAsync(cancellationToken);
        return activity;
    }

    public async Task<IReadOnlyList<Activity>> ListAsync(
        DboxDbContext context,
        string? type,
        string? status,
        CancellationToken cancellationToken)
    {
        var query = context.Activities.AsNoTracking().AsQueryable();
        if (type is not null)
        {
            query = query.Where(activity => activity.Type == type);
        }

        if (status is not null)
        {
            query = query.Where(activity => activity.Status == status);
        }

        return await query
            .OrderByDescending(activity => activity.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Activity?> GetAsync(DboxDbContext context, long id, CancellationToken cancellationToken)
    {
        return context.Activities.AsNoTracking().SingleOrDefaultAsync(activity => activity.Id == id, cancellationToken);
    }

    public async Task<Activity?> UpdateAsync(
        DboxDbContext context,
        long id,
        ActivityUpdateInput input,
        CancellationToken cancellationToken)
    {
        var activity = await context.Activities.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (activity is null)
        {
            return null;
        }

        if (input.TypeProvided)
        {
            activity.Type = input.Type!;
        }

        if (input.TitleProvided)
        {
            activity.Title = input.Title!;
        }

        if (input.DescriptionProvided)
        {
            activity.Description = input.Description;
        }

        if (input.StatusProvided)
        {
            activity.Status = input.Status!;
        }

        await context.SaveChangesAsync(cancellationToken);
        return activity;
    }

    public async Task<bool> DeleteAsync(DboxDbContext context, long id, CancellationToken cancellationToken)
    {
        var activity = await context.Activities.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (activity is null)
        {
            return false;
        }

        context.Activities.Remove(activity);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
