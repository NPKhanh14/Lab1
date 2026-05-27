using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.API.Infrastructure;
using PRN232.Lab1.API.Models;
using PRN232.Lab1.Services.Abstractions;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.API.Controllers;

[ApiController]
[Route("api/students")]
public sealed class StudentsController : ControllerBase
{
    private readonly IStudentService _service;

    public StudentsController(IStudentService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetAll([FromQuery] string? search, [FromQuery] string? sort, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? fields = null, [FromQuery] string? expand = null, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(search, sort, page, size, ParseCsv(expand), cancellationToken);
        return Ok(ApiResponseFactory.Success(QueryProjectionHelper.ProjectPaged(result, fields)));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> GetById(int id, [FromQuery] string? expand = null, CancellationToken cancellationToken = default)
    {
        var model = await _service.GetByIdAsync(id, ParseCsv(expand), cancellationToken);
        if (model is null)
        {
            return NotFound(ApiResponseFactory.Failure<StudentResponse>($"Student {id} was not found."));
        }

        return Ok(ApiResponseFactory.Success(ApiModelMapper.Map<StudentResponse>(model)));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> Create([FromBody] StudentRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.Failure<StudentResponse>("Validation failed.", GetModelStateErrors()));
        }

        var created = await _service.CreateAsync(ApiModelMapper.Map<StudentModel>(request), cancellationToken);
        var response = ApiModelMapper.Map<StudentResponse>(created);
        return CreatedAtAction(nameof(GetById), new { id = response.StudentId }, ApiResponseFactory.Success(response, "Student created successfully"));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StudentResponse>>> Update(int id, [FromBody] StudentRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.Failure<StudentResponse>("Validation failed.", GetModelStateErrors()));
        }

        var model = ApiModelMapper.Map<StudentModel>(request);
        model.StudentId = id;
        var updated = await _service.UpdateAsync(id, model, cancellationToken);
        if (updated is null)
        {
            return NotFound(ApiResponseFactory.Failure<StudentResponse>($"Student {id} was not found."));
        }

        return Ok(ApiResponseFactory.Success(ApiModelMapper.Map<StudentResponse>(updated), "Student updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken = default)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(ApiResponseFactory.Failure<object>($"Student {id} was not found."));
        }

        return Ok(ApiResponseFactory.Success<object>(null, "Student deleted successfully"));
    }

    private static IEnumerable<string> ParseCsv(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private IEnumerable<string> GetModelStateErrors()
        => ModelState.Values.SelectMany(state => state.Errors).Select(error => error.ErrorMessage).Where(message => !string.IsNullOrWhiteSpace(message));
}

[ApiController]
[Route("api/semesters")]
public sealed class SemestersController : ControllerBase
{
    private readonly ISemesterService _service;

    public SemestersController(ISemesterService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetAll([FromQuery] string? search, [FromQuery] string? sort, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? fields = null, [FromQuery] string? expand = null, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(search, sort, page, size, ParseCsv(expand), cancellationToken);
        return Ok(ApiResponseFactory.Success(QueryProjectionHelper.ProjectPaged(result, fields)));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var model = await _service.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (model is null)
        {
            return NotFound(ApiResponseFactory.Failure<SemesterResponse>($"Semester {id} was not found."));
        }

        return Ok(ApiResponseFactory.Success(ApiModelMapper.Map<SemesterResponse>(model)));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> Create([FromBody] SemesterRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.Failure<SemesterResponse>("Validation failed.", GetModelStateErrors()));
        }

        var created = await _service.CreateAsync(ApiModelMapper.Map<SemesterModel>(request), cancellationToken);
        var response = ApiModelMapper.Map<SemesterResponse>(created);
        return CreatedAtAction(nameof(GetById), new { id = response.SemesterId }, ApiResponseFactory.Success(response, "Semester created successfully"));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SemesterResponse>>> Update(int id, [FromBody] SemesterRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.Failure<SemesterResponse>("Validation failed.", GetModelStateErrors()));
        }

        var model = ApiModelMapper.Map<SemesterModel>(request);
        model.SemesterId = id;
        var updated = await _service.UpdateAsync(id, model, cancellationToken);
        if (updated is null)
        {
            return NotFound(ApiResponseFactory.Failure<SemesterResponse>($"Semester {id} was not found."));
        }

        return Ok(ApiResponseFactory.Success(ApiModelMapper.Map<SemesterResponse>(updated), "Semester updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken = default)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(ApiResponseFactory.Failure<object>($"Semester {id} was not found."));
        }

        return Ok(ApiResponseFactory.Success<object>(null, "Semester deleted successfully"));
    }

    private static IEnumerable<string> ParseCsv(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private IEnumerable<string> GetModelStateErrors()
        => ModelState.Values.SelectMany(state => state.Errors).Select(error => error.ErrorMessage).Where(message => !string.IsNullOrWhiteSpace(message));
}

[ApiController]
[Route("api/courses")]
public sealed class CoursesController : ControllerBase
{
    private readonly ICourseService _service;

    public CoursesController(ICourseService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetAll([FromQuery] string? search, [FromQuery] string? sort, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? fields = null, [FromQuery] string? expand = null, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(search, sort, page, size, ParseCsv(expand), cancellationToken);
        return Ok(ApiResponseFactory.Success(QueryProjectionHelper.ProjectPaged(result, fields)));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> GetById(int id, [FromQuery] string? expand = null, CancellationToken cancellationToken = default)
    {
        var model = await _service.GetByIdAsync(id, ParseCsv(expand), cancellationToken);
        if (model is null)
        {
            return NotFound(ApiResponseFactory.Failure<CourseResponse>($"Course {id} was not found."));
        }

        return Ok(ApiResponseFactory.Success(ApiModelMapper.Map<CourseResponse>(model)));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> Create([FromBody] CourseRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.Failure<CourseResponse>("Validation failed.", GetModelStateErrors()));
        }

        var created = await _service.CreateAsync(ApiModelMapper.Map<CourseModel>(request), cancellationToken);
        var response = ApiModelMapper.Map<CourseResponse>(created);
        return CreatedAtAction(nameof(GetById), new { id = response.CourseId }, ApiResponseFactory.Success(response, "Course created successfully"));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CourseResponse>>> Update(int id, [FromBody] CourseRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.Failure<CourseResponse>("Validation failed.", GetModelStateErrors()));
        }

        var model = ApiModelMapper.Map<CourseModel>(request);
        model.CourseId = id;
        var updated = await _service.UpdateAsync(id, model, cancellationToken);
        if (updated is null)
        {
            return NotFound(ApiResponseFactory.Failure<CourseResponse>($"Course {id} was not found."));
        }

        return Ok(ApiResponseFactory.Success(ApiModelMapper.Map<CourseResponse>(updated), "Course updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken = default)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(ApiResponseFactory.Failure<object>($"Course {id} was not found."));
        }

        return Ok(ApiResponseFactory.Success<object>(null, "Course deleted successfully"));
    }

    private static IEnumerable<string> ParseCsv(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private IEnumerable<string> GetModelStateErrors()
        => ModelState.Values.SelectMany(state => state.Errors).Select(error => error.ErrorMessage).Where(message => !string.IsNullOrWhiteSpace(message));
}

[ApiController]
[Route("api/subjects")]
public sealed class SubjectsController : ControllerBase
{
    private readonly ISubjectService _service;

    public SubjectsController(ISubjectService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetAll([FromQuery] string? search, [FromQuery] string? sort, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? fields = null, [FromQuery] string? expand = null, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(search, sort, page, size, ParseCsv(expand), cancellationToken);
        return Ok(ApiResponseFactory.Success(QueryProjectionHelper.ProjectPaged(result, fields)));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SubjectResponse>>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var model = await _service.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (model is null)
        {
            return NotFound(ApiResponseFactory.Failure<SubjectResponse>($"Subject {id} was not found."));
        }

        return Ok(ApiResponseFactory.Success(ApiModelMapper.Map<SubjectResponse>(model)));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<SubjectResponse>>> Create([FromBody] SubjectRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.Failure<SubjectResponse>("Validation failed.", GetModelStateErrors()));
        }

        var created = await _service.CreateAsync(ApiModelMapper.Map<SubjectModel>(request), cancellationToken);
        var response = ApiModelMapper.Map<SubjectResponse>(created);
        return CreatedAtAction(nameof(GetById), new { id = response.SubjectId }, ApiResponseFactory.Success(response, "Subject created successfully"));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SubjectResponse>>> Update(int id, [FromBody] SubjectRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.Failure<SubjectResponse>("Validation failed.", GetModelStateErrors()));
        }

        var model = ApiModelMapper.Map<SubjectModel>(request);
        model.SubjectId = id;
        var updated = await _service.UpdateAsync(id, model, cancellationToken);
        if (updated is null)
        {
            return NotFound(ApiResponseFactory.Failure<SubjectResponse>($"Subject {id} was not found."));
        }

        return Ok(ApiResponseFactory.Success(ApiModelMapper.Map<SubjectResponse>(updated), "Subject updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken = default)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(ApiResponseFactory.Failure<object>($"Subject {id} was not found."));
        }

        return Ok(ApiResponseFactory.Success<object>(null, "Subject deleted successfully"));
    }

    private static IEnumerable<string> ParseCsv(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private IEnumerable<string> GetModelStateErrors()
        => ModelState.Values.SelectMany(state => state.Errors).Select(error => error.ErrorMessage).Where(message => !string.IsNullOrWhiteSpace(message));
}

[ApiController]
[Route("api/enrollments")]
public sealed class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _service;

    public EnrollmentsController(IEnrollmentService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> GetAll([FromQuery] string? search, [FromQuery] string? sort, [FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? fields = null, [FromQuery] string? expand = null, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(search, sort, page, size, ParseCsv(expand), cancellationToken);
        return Ok(ApiResponseFactory.Success(QueryProjectionHelper.ProjectPaged(result, fields)));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> GetById(int id, [FromQuery] string? expand = null, CancellationToken cancellationToken = default)
    {
        var model = await _service.GetByIdAsync(id, ParseCsv(expand), cancellationToken);
        if (model is null)
        {
            return NotFound(ApiResponseFactory.Failure<EnrollmentResponse>($"Enrollment {id} was not found."));
        }

        return Ok(ApiResponseFactory.Success(ApiModelMapper.Map<EnrollmentResponse>(model)));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> Create([FromBody] EnrollmentRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.Failure<EnrollmentResponse>("Validation failed.", GetModelStateErrors()));
        }

        var created = await _service.CreateAsync(ApiModelMapper.Map<EnrollmentModel>(request), cancellationToken);
        var response = ApiModelMapper.Map<EnrollmentResponse>(created);
        return CreatedAtAction(nameof(GetById), new { id = response.EnrollmentId }, ApiResponseFactory.Success(response, "Enrollment created successfully"));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<EnrollmentResponse>>> Update(int id, [FromBody] EnrollmentRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponseFactory.Failure<EnrollmentResponse>("Validation failed.", GetModelStateErrors()));
        }

        var model = ApiModelMapper.Map<EnrollmentModel>(request);
        model.EnrollmentId = id;
        var updated = await _service.UpdateAsync(id, model, cancellationToken);
        if (updated is null)
        {
            return NotFound(ApiResponseFactory.Failure<EnrollmentResponse>($"Enrollment {id} was not found."));
        }

        return Ok(ApiResponseFactory.Success(ApiModelMapper.Map<EnrollmentResponse>(updated), "Enrollment updated successfully"));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken = default)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(ApiResponseFactory.Failure<object>($"Enrollment {id} was not found."));
        }

        return Ok(ApiResponseFactory.Success<object>(null, "Enrollment deleted successfully"));
    }

    private static IEnumerable<string> ParseCsv(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private IEnumerable<string> GetModelStateErrors()
        => ModelState.Values.SelectMany(state => state.Errors).Select(error => error.ErrorMessage).Where(message => !string.IsNullOrWhiteSpace(message));
}