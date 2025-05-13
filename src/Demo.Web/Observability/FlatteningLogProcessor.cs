using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenTelemetry;
using OpenTelemetry.Logs;

namespace Demo.Web.Observability;

public class FlatteningLogProcessor : BaseProcessor<LogRecord>
{
    public override void OnEnd(LogRecord logRecord)
    {
        if (logRecord.Attributes is null) 
            return;
        
        logRecord.Attributes = logRecord.Attributes.SelectMany(kv =>
            Flatten(kv.Key.TrimStart('@'), kv.Value)
                .Select(x => new KeyValuePair<string, object?>(x.Item1, x.Item2))).ToList();
    }

    private static IEnumerable<(string, string)> Flatten(string prefix, object? value)
    {
        if (value is null)
            return [];
        
        var type = value.GetType();
        if (type.IsPrimitive || value is string || value is DateTime || value is Guid || value is decimal)
        {
            var v = value.ToString();
            return v is null ? [] : [(prefix, v)];
        }

        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .SelectMany(prop =>
            {
                var subValue = prop.GetValue(value);
                return subValue is null ? [] : Flatten($"{prefix}.{prop.Name}", subValue);
            });
    }
}