using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repositories.Abstractions;
using PRN232.Lab1.Repositories.Entities;
using PRN232.Lab1.Repositories.Persistence;

namespace PRN232.Lab1.Repositories.Repositories;

public abstract class RepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly string _keyPropertyName;

    protected RepositoryBase(AppDbContext context, string keyPropertyName)
    {
        Context = context;
        Set = context.Set<TEntity>();
        _keyPropertyName = keyPropertyName;
    }

    protected AppDbContext Context { get; }
    protected DbSet<TEntity> Set { get; }
    protected virtual string[] DefaultIncludes => Array.Empty<string>();

    public virtual async Task<RepositoryPage<TEntity>> GetPageAsync(string? search, string? sort, int page, int size, IEnumerable<string>? expand = null, CancellationToken cancellationToken = default)
    {
        var currentPage = page <= 0 ? 1 : page;
        var pageSize = size <= 0 ? 10 : size;

        IQueryable<TEntity> query = Set.AsNoTracking();
        query = ApplyIncludes(query, expand);
        query = ApplySearch(query, search);
        query = ApplySort(query, sort);

        var totalItems = await query.CountAsync(cancellationToken);
        var items = await query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new RepositoryPage<TEntity>(items, totalItems, currentPage, pageSize);
    }

    public virtual async Task<TEntity?> GetByIdAsync(int id, IEnumerable<string>? expand = null, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = Set.AsNoTracking();
        query = ApplyIncludes(query, expand);
        return await query.FirstOrDefaultAsync(entity => EF.Property<int>(entity, _keyPropertyName) == id, cancellationToken);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await Set.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task<TEntity?> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Set.Update(entity);
        await Context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await Set.FirstOrDefaultAsync(item => EF.Property<int>(item, _keyPropertyName) == id, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        Set.Remove(entity);
        await Context.SaveChangesAsync(cancellationToken);
        return true;
    }

    protected virtual IQueryable<TEntity> ApplySearch(IQueryable<TEntity> query, string? search) => query;

    protected IQueryable<TEntity> ApplyContainsSearch(IQueryable<TEntity> query, string? search, params string[] propertyPaths)
    {
        if (string.IsNullOrWhiteSpace(search) || propertyPaths.Length == 0)
        {
            return query;
        }

        search = search.Trim().ToLowerInvariant();
        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        Expression? predicate = null;

        foreach (var propertyPath in propertyPaths)
        {
            var member = BuildMemberAccess(parameter, propertyPath);
            if (member is null)
            {
                continue;
            }

            Expression stringValue = member.Type == typeof(string)
                ? member
                : Expression.Call(member, typeof(object).GetMethod(nameof(ToString), Type.EmptyTypes)!);

            Expression notNull = member.Type == typeof(string)
                ? Expression.NotEqual(member, Expression.Constant(null, typeof(string)))
                : Expression.Constant(true);

            var toLower = Expression.Call(stringValue, typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!);
            var contains = Expression.Call(toLower, nameof(string.Contains), Type.EmptyTypes, Expression.Constant(search));
            var safeContains = Expression.AndAlso(notNull, contains);

            predicate = predicate is null ? safeContains : Expression.OrElse(predicate, safeContains);
        }

        if (predicate is null)
        {
            return query;
        }

        return query.Where(Expression.Lambda<Func<TEntity, bool>>(predicate, parameter));
    }

    protected virtual IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query, IEnumerable<string>? expand)
    {
        var includes = (expand?.Any() == true ? expand : DefaultIncludes).Distinct(StringComparer.OrdinalIgnoreCase);
        foreach (var include in includes)
        {
            if (!string.IsNullOrWhiteSpace(include))
            {
                query = query.Include(include);
            }
        }

        return query;
    }

    protected virtual IQueryable<TEntity> ApplySort(IQueryable<TEntity> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query;
        }

        IOrderedQueryable<TEntity>? orderedQuery = null;
        var parameter = Expression.Parameter(typeof(TEntity), "entity");

        foreach (var token in sort.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var descending = token.StartsWith('-');
            var field = descending ? token[1..] : token;
            var member = BuildMemberAccess(parameter, field);
            if (member is null)
            {
                continue;
            }

            var lambda = Expression.Lambda(member, parameter);
            var methodName = orderedQuery is null
                ? descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy)
                : descending ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy);

            var method = typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(methodInfo => methodInfo.Name == methodName && methodInfo.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TEntity), member.Type);

            query = (IQueryable<TEntity>)method.Invoke(null, new object[] { orderedQuery ?? query, lambda })!;
            orderedQuery = (IOrderedQueryable<TEntity>)query;
        }

        return orderedQuery ?? query;
    }

    private static Expression? BuildMemberAccess(Expression instance, string path)
    {
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var property = instance.Type.GetProperty(segment, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (property is null)
            {
                return null;
            }

            instance = Expression.Property(instance, property);
        }

        return instance;
    }
}

public sealed class SemesterRepository : RepositoryBase<Semester>
{
    public SemesterRepository(AppDbContext context) : base(context, nameof(Semester.SemesterId))
    {
    }

    protected override IQueryable<Semester> ApplySearch(IQueryable<Semester> query, string? search)
        => ApplyContainsSearch(query, search, nameof(Semester.SemesterName));
}

public sealed class CourseRepository : RepositoryBase<Course>
{
    public CourseRepository(AppDbContext context) : base(context, nameof(Course.CourseId))
    {
    }

    protected override string[] DefaultIncludes => new[] { nameof(Course.Semester) };

    protected override IQueryable<Course> ApplySearch(IQueryable<Course> query, string? search)
        => ApplyContainsSearch(query, search, nameof(Course.CourseName), nameof(Course.Semester) + "." + nameof(Semester.SemesterName));
}

public sealed class SubjectRepository : RepositoryBase<Subject>
{
    public SubjectRepository(AppDbContext context) : base(context, nameof(Subject.SubjectId))
    {
    }

    protected override IQueryable<Subject> ApplySearch(IQueryable<Subject> query, string? search)
        => ApplyContainsSearch(query, search, nameof(Subject.SubjectCode), nameof(Subject.SubjectName));
}

public sealed class StudentRepository : RepositoryBase<Student>
{
    public StudentRepository(AppDbContext context) : base(context, nameof(Student.StudentId))
    {
    }

    protected override IQueryable<Student> ApplySearch(IQueryable<Student> query, string? search)
        => ApplyContainsSearch(query, search, nameof(Student.FullName), nameof(Student.Email));
}

public sealed class EnrollmentRepository : RepositoryBase<Enrollment>
{
    public EnrollmentRepository(AppDbContext context) : base(context, nameof(Enrollment.EnrollmentId))
    {
    }

    protected override string[] DefaultIncludes => new[] { nameof(Enrollment.Student), nameof(Enrollment.Course), nameof(Enrollment.Course) + "." + nameof(Course.Semester) };

    protected override IQueryable<Enrollment> ApplySearch(IQueryable<Enrollment> query, string? search)
        => ApplyContainsSearch(
            query,
            search,
            nameof(Enrollment.Status),
            nameof(Enrollment.Student) + "." + nameof(Student.FullName),
            nameof(Enrollment.Student) + "." + nameof(Student.Email),
            nameof(Enrollment.Course) + "." + nameof(Course.CourseName),
            nameof(Enrollment.Course) + "." + nameof(Course.Semester) + "." + nameof(Semester.SemesterName));
}