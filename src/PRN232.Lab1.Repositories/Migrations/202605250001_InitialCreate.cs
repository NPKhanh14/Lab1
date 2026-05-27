using Microsoft.EntityFrameworkCore.Migrations;
using PRN232.Lab1.Repositories.Seeding;

namespace PRN232.Lab1.Repositories.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Semesters",
            columns: table => new
            {
                SemesterId = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                SemesterName = table.Column<string>(maxLength: 200, nullable: false),
                StartDate = table.Column<DateTime>(nullable: false),
                EndDate = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Semesters", x => x.SemesterId);
            });

        migrationBuilder.CreateTable(
            name: "Students",
            columns: table => new
            {
                StudentId = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                FullName = table.Column<string>(maxLength: 200, nullable: false),
                Email = table.Column<string>(maxLength: 256, nullable: false),
                DateOfBirth = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Students", x => x.StudentId);
            });

        migrationBuilder.CreateTable(
            name: "Subjects",
            columns: table => new
            {
                SubjectId = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                SubjectCode = table.Column<string>(maxLength: 50, nullable: false),
                SubjectName = table.Column<string>(maxLength: 200, nullable: false),
                Credit = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Subjects", x => x.SubjectId);
            });

        migrationBuilder.CreateTable(
            name: "Courses",
            columns: table => new
            {
                CourseId = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                CourseName = table.Column<string>(maxLength: 200, nullable: false),
                SemesterId = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Courses", x => x.CourseId);
                table.ForeignKey(
                    name: "FK_Courses_Semesters_SemesterId",
                    column: x => x.SemesterId,
                    principalTable: "Semesters",
                    principalColumn: "SemesterId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Enrollments",
            columns: table => new
            {
                EnrollmentId = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                StudentId = table.Column<int>(nullable: false),
                CourseId = table.Column<int>(nullable: false),
                EnrollDate = table.Column<DateTime>(nullable: false),
                Status = table.Column<string>(maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Enrollments", x => x.EnrollmentId);
                table.ForeignKey(
                    name: "FK_Enrollments_Courses_CourseId",
                    column: x => x.CourseId,
                    principalTable: "Courses",
                    principalColumn: "CourseId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Enrollments_Students_StudentId",
                    column: x => x.StudentId,
                    principalTable: "Students",
                    principalColumn: "StudentId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_Courses_SemesterId", table: "Courses", column: "SemesterId");
        migrationBuilder.CreateIndex(name: "IX_Enrollments_CourseId", table: "Enrollments", column: "CourseId");
        migrationBuilder.CreateIndex(name: "IX_Enrollments_StudentId", table: "Enrollments", column: "StudentId");

        migrationBuilder.InsertData(table: "Semesters", columns: new[] { "SemesterId", "SemesterName", "StartDate", "EndDate" }, values: SeedData.SemesterRows());
        migrationBuilder.InsertData(table: "Subjects", columns: new[] { "SubjectId", "SubjectCode", "SubjectName", "Credit" }, values: SeedData.SubjectRows());
        migrationBuilder.InsertData(table: "Students", columns: new[] { "StudentId", "FullName", "Email", "DateOfBirth" }, values: SeedData.StudentRows());
        migrationBuilder.InsertData(table: "Courses", columns: new[] { "CourseId", "CourseName", "SemesterId" }, values: SeedData.CourseRows());
        migrationBuilder.InsertData(table: "Enrollments", columns: new[] { "EnrollmentId", "StudentId", "CourseId", "EnrollDate", "Status" }, values: SeedData.EnrollmentRows());
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Enrollments");
        migrationBuilder.DropTable(name: "Subjects");
        migrationBuilder.DropTable(name: "Courses");
        migrationBuilder.DropTable(name: "Students");
        migrationBuilder.DropTable(name: "Semesters");
    }
}