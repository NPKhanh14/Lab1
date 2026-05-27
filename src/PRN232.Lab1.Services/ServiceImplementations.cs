using System.Text.Json;
using System.Text.Json.Serialization;
using PRN232.Lab1.Repositories.Abstractions;
using PRN232.Lab1.Repositories.Entities;
using PRN232.Lab1.Services.Abstractions;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Services;

internal static class ModelMapper
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public static TDestination Map<TDestination>(object source)
    {
        var json = JsonSerializer.Serialize(source, Options);
        return JsonSerializer.Deserialize<TDestination>(json, Options)!;
    }
}

public abstract class CrudService<TEntity, TModel> : ICrudService<TModel>
    where TEntity : class
    where TModel : class
{
    private readonly IRepository<TEntity> _repository;
    private readonly string _keyPropertyName;

    protected CrudService(IRepository<TEntity> repository, string keyPropertyName, IEnumerable<string>? defaultExpand = null)
    {
        _repository = repository;
        _keyPropertyName = keyPropertyName;
        DefaultExpand = defaultExpand?.ToArray() ?? Array.Empty<string>();
    }

    protected string[] DefaultExpand { get; }

    public virtual async Task<PagedResult<TModel>> GetAllAsync(string? search, string? sort, int page, int size, IEnumerable<string>? expand = null, CancellationToken cancellationToken = default)
    {
        var repositoryPage = await _repository.GetPageAsync(search, sort, page, size, expand, cancellationToken);
        return new PagedResult<TModel>
        {
            Items = repositoryPage.Items.Select(MapToModel).ToList(),
            Pagination = CreatePagination(repositoryPage.Page, repositoryPage.PageSize, repositoryPage.TotalItems)
        };
    }

    public virtual async Task<TModel?> GetByIdAsync(int id, IEnumerable<string>? expand = null, CancellationToken cancellationToken = default)
    {
        var includes = expand?.Any() == true ? expand : DefaultExpand;
        var entity = await _repository.GetByIdAsync(id, includes, cancellationToken);
        return entity is null ? null : MapToModel(entity);
    }

    public virtual async Task<TModel> CreateAsync(TModel model, CancellationToken cancellationToken = default)
    {
        var entity = MapToEntity(model);
        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToModel(created);
    }

    public virtual async Task<TModel?> UpdateAsync(int id, TModel model, CancellationToken cancellationToken = default)
    {
        var current = await _repository.GetByIdAsync(id, DefaultExpand, cancellationToken);
        if (current is null)
        {
            return null;
        }

        var entity = MapToEntity(model);
        SetKey(entity, id);
        var updated = await _repository.UpdateAsync(entity, cancellationToken);
        return updated is null ? null : MapToModel(updated);
    }

    public virtual Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        => _repository.DeleteAsync(id, cancellationToken);

    protected virtual TModel MapToModel(TEntity entity) => ModelMapper.Map<TModel>(entity);
    protected virtual TEntity MapToEntity(TModel model) => ModelMapper.Map<TEntity>(model);

    private static PaginationMetadata CreatePagination(int page, int pageSize, int totalItems)
        => new()
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = pageSize == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };

    private void SetKey(TEntity entity, int id)
    {
        var property = typeof(TEntity).GetProperty(_keyPropertyName);
        property?.SetValue(entity, id);
    }
}

public sealed class SemesterService : CrudService<Semester, SemesterModel>, ISemesterService
{
    public SemesterService(IRepository<Semester> repository) : base(repository, nameof(Semester.SemesterId))
    {
    }
}

public sealed class CourseService : CrudService<Course, CourseModel>, ICourseService
{
    public CourseService(IRepository<Course> repository) : base(repository, nameof(Course.CourseId), new[] { nameof(Course.Semester) })
    {
    }
}

public sealed class SubjectService : CrudService<Subject, SubjectModel>, ISubjectService
{
    public SubjectService(IRepository<Subject> repository) : base(repository, nameof(Subject.SubjectId))
    {
    }
}

public sealed class StudentService : CrudService<Student, StudentModel>, IStudentService
{
    public StudentService(IRepository<Student> repository) : base(repository, nameof(Student.StudentId))
    {
    }
}

public sealed class EnrollmentService : CrudService<Enrollment, EnrollmentModel>, IEnrollmentService
{
    public EnrollmentService(IRepository<Enrollment> repository)
        : base(repository, nameof(Enrollment.EnrollmentId), new[] { nameof(Enrollment.Student), nameof(Enrollment.Course), nameof(Enrollment.Course) + "." + nameof(Course.Semester) })
    {
    }
}