using System.Text.Encodings.Web;
using System.Text.Json.Serialization.Metadata;
using GitVersion.Extensions;
using GitVersion.Helpers;

namespace GitVersion.OutputVariables;

[JsonSerializable(typeof(VersionVariablesJsonModel))]
public partial class VersionVariablesJsonModelSerializerContext : JsonSerializerContext { }

public class VersionVariableSerializer(IFileSystem fileSystem) : IVersionVariableSerializer
{
    public GitVersionVariables FromJson(string json)
    {
        var serializeOptions = JsonSerializerOptions();
        var variablePairs = JsonSerializer.Deserialize<Dictionary<string, string>>(json, serializeOptions);
        return FromDictionary(variablePairs);
    }

    public string ToJson(GitVersionVariables gitVersionVariables)
    {
        var variablesType = typeof(VersionVariablesJsonModel);
        var variables = new VersionVariablesJsonModel();

        foreach (var (key, value) in gitVersionVariables.OrderBy(x => x.Key))
        {
            var propertyInfo = variablesType.GetProperty(key);
            propertyInfo?.SetValue(variables, ChangeType(value, propertyInfo.PropertyType));
        }

        var serializeOptions = JsonSerializerOptions();

        return JsonSerializer.Serialize(variables, serializeOptions);
    }

    public GitVersionVariables FromFile(string filePath)
    {
        try
        {
            var retryAction = new RetryAction<IOException, GitVersionVariables>();
            return retryAction.Execute(() => FromFileInternal(filePath));
        }
        catch (AggregateException ex)
        {
            var lastException = ex.InnerExceptions.LastOrDefault() ?? ex.InnerException;
            if (lastException != null)
            {
                throw lastException;
            }

            throw;
        }
    }

    public void ToFile(GitVersionVariables gitVersionVariables, string filePath)
    {
        try
        {
            var retryAction = new RetryAction<IOException>();
            retryAction.Execute(() => ToFileInternal(gitVersionVariables, filePath));
        }
        catch (AggregateException ex)
        {
            var lastException = ex.InnerExceptions.LastOrDefault() ?? ex.InnerException;
            if (lastException != null)
            {
                throw lastException;
            }

            throw;
        }
    }

    private static GitVersionVariables FromDictionary(IEnumerable<KeyValuePair<string, string>>? properties)
    {
        var type = typeof(GitVersionVariables);
        var constructors = type.GetConstructors();

        var ctor = constructors.Single();
        var ctorArgs = ctor.GetParameters()
            .Select(p => properties?.Single(v => string.Equals(v.Key, p.Name, StringComparison.InvariantCultureIgnoreCase)).Value)
            .Cast<object>()
            .ToArray();
        var instance = Activator.CreateInstance(type, ctorArgs).NotNull();
        return (GitVersionVariables)instance;
    }

    private GitVersionVariables FromFileInternal(string filePath)
    {
        var json = fileSystem.ReadAllText(filePath);
        return FromJson(json);
    }

    private void ToFileInternal(GitVersionVariables gitVersionVariables, string filePath)
    {
        var json = ToJson(gitVersionVariables);
        fileSystem.WriteAllText(filePath, json);
    }

    private static JsonSerializerOptions JsonSerializerOptions() => new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new VersionVariablesJsonStringConverter() },

        TypeInfoResolver = JsonSerializer.IsReflectionEnabledByDefault
                ? new DefaultJsonTypeInfoResolver()
                : VersionVariablesJsonModelSerializerContext.Default
    };

    private static object? ChangeType(object? value, Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            if (value == null || value.ToString()?.Length == 0)
            {
                return null;
            }

            type = Nullable.GetUnderlyingType(type)!;
        }

        return Convert.ChangeType(value, type);
    }
}
