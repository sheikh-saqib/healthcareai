using Dapper;
using System.Data;
using System.Text.Json;

namespace HealthCareAI.Infrastructure.Data;

public class JsonDocumentTypeHandler : SqlMapper.TypeHandler<JsonDocument?>
{
    public override void SetValue(IDbDataParameter parameter, JsonDocument? value)
    {
        parameter.Value = value?.RootElement.GetRawText() ?? (object)DBNull.Value;
        parameter.DbType = DbType.String;
    }

    public override JsonDocument? Parse(object value)
    {
        if (value == null || value == DBNull.Value)
            return null;

        var jsonString = value.ToString();
        if (string.IsNullOrWhiteSpace(jsonString))
            return null;

        try
        {
            return JsonDocument.Parse(jsonString);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
