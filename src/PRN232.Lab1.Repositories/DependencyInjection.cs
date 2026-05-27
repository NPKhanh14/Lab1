using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PRN232.Lab1.Repositories.Abstractions;
using PRN232.Lab1.Repositories.Entities;
using PRN232.Lab1.Repositories.Persistence;
using PRN232.Lab1.Repositories.Repositories;

namespace PRN232.Lab1.Repositories;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositoryLayer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IRepository<Semester>, SemesterRepository>();
        services.AddScoped<IRepository<Course>, CourseRepository>();
        services.AddScoped<IRepository<Subject>, SubjectRepository>();
        services.AddScoped<IRepository<Student>, StudentRepository>();
        services.AddScoped<IRepository<Enrollment>, EnrollmentRepository>();

        return services;
    }
}