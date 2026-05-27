using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.Services.Abstractions;

public interface ICrudService<TModel>
{
    Task<PagedResult<TModel>> GetAllAsync(string? search, string? sort, int page, int size, IEnumerable<string>? expand = null, CancellationToken cancellationToken = default);
    Task<TModel?> GetByIdAsync(int id, IEnumerable<string>? expand = null, CancellationToken cancellationToken = default);
    Task<TModel> CreateAsync(TModel model, CancellationToken cancellationToken = default);
    Task<TModel?> UpdateAsync(int id, TModel model, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public interface ISemesterService : ICrudService<SemesterModel>;
public interface ICourseService : ICrudService<CourseModel>;
public interface ISubjectService : ICrudService<SubjectModel>;
public interface IStudentService : ICrudService<StudentModel>;
public interface IEnrollmentService : ICrudService<EnrollmentModel>;