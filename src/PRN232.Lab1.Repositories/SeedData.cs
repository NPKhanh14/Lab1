using PRN232.Lab1.Repositories.Entities;

namespace PRN232.Lab1.Repositories.Seeding;

public static class SeedData
{
    private static readonly DateTime SemesterStart = new(2025, 1, 1);
    private static readonly DateTime StudentBirth = new(2000, 1, 1);
    private static readonly DateTime EnrollStart = new(2025, 1, 15);

    public static List<Semester> Semesters { get; } = BuildSemesters();
    public static List<Subject> Subjects { get; } = BuildSubjects();
    public static List<Student> Students { get; } = BuildStudents();
    public static List<Course> Courses { get; } = BuildCourses();
    public static List<Enrollment> Enrollments { get; } = BuildEnrollments();

    public static object[,] SemesterRows() => ToRows(Semesters, x => new object[] { x.SemesterId, x.SemesterName, x.StartDate, x.EndDate });
    public static object[,] SubjectRows() => ToRows(Subjects, x => new object[] { x.SubjectId, x.SubjectCode, x.SubjectName, x.Credit });
    public static object[,] StudentRows() => ToRows(Students, x => new object[] { x.StudentId, x.FullName, x.Email, x.DateOfBirth });
    public static object[,] CourseRows() => ToRows(Courses, x => new object[] { x.CourseId, x.CourseName, x.SemesterId });
    public static object[,] EnrollmentRows() => ToRows(Enrollments, x => new object[] { x.EnrollmentId, x.StudentId, x.CourseId, x.EnrollDate, x.Status });

    private static List<Semester> BuildSemesters()
        => Enumerable.Range(1, 5)
            .Select(index => new Semester
            {
                SemesterId = index,
                SemesterName = $"Semester {index}",
                StartDate = SemesterStart.AddMonths((index - 1) * 4),
                EndDate = SemesterStart.AddMonths((index - 1) * 4).AddMonths(4).AddDays(-1)
            })
            .ToList();

    private static List<Subject> BuildSubjects()
        => Enumerable.Range(1, 10)
            .Select(index => new Subject
            {
                SubjectId = index,
                SubjectCode = $"SUB{index:000}",
                SubjectName = $"Subject {index}",
                Credit = 2 + (index % 3)
            })
            .ToList();

    private static List<Student> BuildStudents()
        => Enumerable.Range(1, 50)
            .Select(index => new Student
            {
                StudentId = index,
                FullName = $"Student {index:000}",
                Email = $"student{index:000}@lms.local",
                DateOfBirth = StudentBirth.AddDays(index * 120)
            })
            .ToList();

    private static List<Course> BuildCourses()
        => Enumerable.Range(1, 20)
            .Select(index => new Course
            {
                CourseId = index,
                CourseName = $"Course {index:000}",
                SemesterId = ((index - 1) % 5) + 1
            })
            .ToList();

    private static List<Enrollment> BuildEnrollments()
    {
        var statuses = new[] { "Active", "Completed", "Dropped" };
        return Enumerable.Range(1, 500)
            .Select(index => new Enrollment
            {
                EnrollmentId = index,
                StudentId = ((index - 1) % 50) + 1,
                CourseId = ((index - 1) % 20) + 1,
                EnrollDate = EnrollStart.AddDays(index - 1),
                Status = statuses[(index - 1) % statuses.Length]
            })
            .ToList();
    }

    private static object[,] ToRows<T>(IReadOnlyList<T> source, Func<T, object[]> selector)
    {
        if (source.Count == 0)
        {
            return new object[0, 0];
        }

        var firstRow = selector(source[0]);
        var rows = new object[source.Count, firstRow.Length];

        for (var rowIndex = 0; rowIndex < source.Count; rowIndex++)
        {
            var row = selector(source[rowIndex]);
            for (var columnIndex = 0; columnIndex < row.Length; columnIndex++)
            {
                rows[rowIndex, columnIndex] = row[columnIndex];
            }
        }

        return rows;
    }
}