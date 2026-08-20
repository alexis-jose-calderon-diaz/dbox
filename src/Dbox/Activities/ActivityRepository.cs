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
        ActivityFilter filter,
        int skip,
        int? take,
        CancellationToken cancellationToken)
    {
        var query = ApplyFilter(context, filter)
            .OrderBy(activity => activity.CreatedAt)
            .ThenBy(activity => activity.Id)
            .Skip(skip);
        if (take is not null)
        {
            query = query.Take(take.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(DboxDbContext context, ActivityFilter filter, CancellationToken cancellationToken) =>
        ApplyFilter(context, filter).CountAsync(cancellationToken);

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
            activity.Description = input.Description!;
        }

        if (input.StatusProvided)
        {
            activity.Status = input.Status!;
        }

        if (input.SourceProvided)
        {
            activity.Source = input.Source!;
        }

        if (input.AreaProvided)
        {
            activity.Area = input.Area!;
        }

        if (input.ResultProvided)
        {
            activity.Result = input.Result!;
        }

        if (input.ImpactProvided)
        {
            activity.Impact = input.Impact!;
        }

        if (input.EffortProvided)
        {
            activity.Effort = input.Effort!;
        }

        if (input.ReferenceProvided)
        {
            activity.Reference = input.Reference;
        }

        if (input.MetadataProvided)
        {
            activity.Metadata = input.Metadata;
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

    private static IQueryable<Activity> ApplyFilter(DboxDbContext context, ActivityFilter filter)
    {
        var query = context.Activities.AsNoTracking().AsQueryable();
        if (filter.Type is not null)
        {
            query = query.Where(activity => activity.Type == filter.Type);
        }

        if (filter.Status is not null)
        {
            query = query.Where(activity => activity.Status == filter.Status);
        }

        return query;
    }
}
