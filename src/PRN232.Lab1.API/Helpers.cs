using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using PRN232.Lab1.Services.Models;

namespace PRN232.Lab1.API.Infrastructure;

internal static class ApiModelMapper
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

internal static class QueryProjectionHelper
{
    public static PagedResult<object> ProjectPaged<TSource>(PagedResult<TSource> source, string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return new PagedResult<object>
            {
                Items = source.Items.Cast<object>().ToList(),
                Pagination = source.Pagination
            };
        }

        var requestedFields = fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray();
        return new PagedResult<object>
        {
            Items = source.Items.Select(item => (object)ProjectItem(item!, requestedFields)).ToList(),
            Pagination = source.Pagination
        };
    }

    public static object ProjectItem<TSource>(TSource source, string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return source!;
        }

        var requestedFields = fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray();
        return ProjectItem(source!, requestedFields);
    }

    private static Dictionary<string, object?> ProjectItem(object source, IReadOnlyCollection<string> fields)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var type = source.GetType();

        foreach (var field in fields)
        {
            var property = ResolveProperty(type, field);
            if (property is null)
            {
                continue;
            }

            result[field] = property.GetValue(source);
        }

        return result;
    }

    private static PropertyInfo? ResolveProperty(Type type, string field)
    {
        if (field.Equals("id", StringComparison.OrdinalIgnoreCase))
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(property => property.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase));
        }

        return type.GetProperty(field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
    }
}