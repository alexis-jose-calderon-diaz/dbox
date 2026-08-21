using System.Data;
using Dbox.Cli;
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

    public async Task<ActivityPage> ListPageAsync(
        DboxDbContext context,
        ActivityFilter filter,
        int skip,
        int? take,
        CancellationToken cancellationToken)
    {
        var filteredQuery = ApplyFilter(context, filter);
        var total = await filteredQuery.CountAsync(cancellationToken);
        var query = filteredQuery
            .OrderBy(activity => activity.CreatedAt)
            .ThenBy(activity => activity.Id)
            .Skip(skip);
        if (take is not null)
        {
            query = query.Take(take.Value);
        }

        var items = await query.ToListAsync(cancellationToken);
        return new ActivityPage(total, items);
    }

    public Task<int> CountAsync(DboxDbContext context, ActivityFilter filter, CancellationToken cancellationToken) =>
        ApplyFilter(context, filter).CountAsync(cancellationToken);

    public Task<List<Activity>> ListAllAsync(
        DboxDbContext context,
        CancellationToken cancellationToken) =>
        context.Activities
            .AsNoTracking()
            .OrderBy(activity => activity.CreatedAt)
            .ThenBy(activity => activity.Id)
            .ToListAsync(cancellationToken);

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

        var expectedVersion = input.Version!.Value;
        context.Entry(activity).Property(item => item.Version).OriginalValue = expectedVersion;

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

        activity.UpdatedAt = ActivityTimestamp.UtcNow();
        activity.Version = checked(expectedVersion + 1);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw CliException.Conflict(id);
        }

        return activity;
    }

    public async Task<int> ImportAsync(
        DboxDbContext context,
        IReadOnlyList<PortableActivityRecord> records,
        CancellationToken cancellationToken)
    {
        if (records.Count == 0)
        {
            return 0;
        }

        var duplicate = records
            .GroupBy(record => record.Id)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw CliException.Conflict($"Import contains duplicate activity id {duplicate.Key}.");
        }

        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var ids = records.Select(record => record.Id).ToArray();
        var existingIds = await context.Activities
            .AsNoTracking()
            .Where(activity => ids.Contains(activity.Id))
            .Select(activity => activity.Id)
            .ToListAsync(cancellationToken);
        if (existingIds.Count > 0)
        {
            throw CliException.Conflict($"Import conflicts with activity id {existingIds[0]}.");
        }

        context.Activities.AddRange(records.Select(record => new Activity
        {
            Id = record.Id,
            CreatedAt = record.CreatedAt,
            UpdatedAt = record.UpdatedAt,
            Version = record.Version,
            Type = record.Type,
            Title = record.Title,
            Description = record.Description,
            Status = record.Status,
            Source = record.Source,
            Area = record.Area,
            Result = record.Result,
            Impact = record.Impact,
            Effort = record.Effort,
            Reference = record.Reference,
            Metadata = record.Metadata
        }));

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return records.Count;
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            throw CliException.Conflict();
        }
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

        if (filter.Area is not null)
        {
            query = query.Where(activity => activity.Area == filter.Area);
        }

        if (filter.Source is not null)
        {
            query = query.Where(activity => activity.Source == filter.Source);
        }

        if (filter.Effort is not null)
        {
            query = query.Where(activity => activity.Effort == filter.Effort);
        }

        if (filter.CreatedFrom is not null)
        {
            query = query.Where(activity => activity.CreatedAt >= filter.CreatedFrom.Value);
        }

        if (filter.CreatedTo is not null)
        {
            query = query.Where(activity => activity.CreatedAt <= filter.CreatedTo.Value);
        }

        if (filter.Title is not null)
        {
            var title = filter.Title.ToLowerInvariant();
            query = query.Where(activity => activity.Title.ToLower().Contains(title));
        }

        if (filter.Description is not null)
        {
            var description = filter.Description.ToLowerInvariant();
            query = query.Where(activity => activity.Description.ToLower().Contains(description));
        }

        return query;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        var message = exception.ToString();
        return message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase)
            || message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase);
    }
}

public sealed record ActivityPage(int Total, IReadOnlyList<Activity> Items);
