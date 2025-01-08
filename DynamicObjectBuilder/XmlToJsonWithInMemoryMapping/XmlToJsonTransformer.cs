using System.Xml.Linq;
using System.Xml.XPath;
using System.Text.Json.Nodes;

public class XmlToJsonTransformer
{
    private readonly List<MappingModel> _mappings;

    public XmlToJsonTransformer(List<MappingModel> mappings)
    {
        _mappings = mappings;
    }

    public JsonObject TransformXmlToJson(string xmlContent)
    {
        var xDocument = XDocument.Parse(xmlContent);
        var rootNode = new JsonObject();

        foreach (var mapping in _mappings)
        {
            ProcessMapping(xDocument, rootNode, mapping);
        }

        return rootNode;
    }

    private void ProcessMapping(XDocument xDocument, JsonObject rootNode, MappingModel mapping)
    {
        var parentNode = GetOrCreateParentNode(rootNode, mapping.JsonParentElement);

        if (mapping.IsArray)
        {
            var elements = xDocument.XPathSelectElements(mapping.XPath);
            var jsonArray = parentNode[mapping.PrimaryJsonElement] as JsonArray ?? new JsonArray();

            foreach (var element in elements)
            {
                var itemNode = new JsonObject();
                foreach (var childMapping in mapping.Mappings)
                {
                    var childValue = element.XPathSelectElement(childMapping.XPath)?.Value;
                    if (!string.IsNullOrEmpty(childValue))
                    {
                        var transformedValue = ApplyTransform(childValue, childMapping.Transform);
                        itemNode[childMapping.PrimaryJsonElement] = JsonValue.Create(ConvertToTargetType(transformedValue, childMapping.TargetDataType));
                    }
                }
                jsonArray.Add(itemNode);
            }

            parentNode[mapping.PrimaryJsonElement] = jsonArray;
        }
        else
        {
            var value = xDocument.XPathSelectElement(mapping.XPath)?.Value;
            if (!string.IsNullOrEmpty(value))
            {
                var transformedValue = ApplyTransform(value, mapping.Transform);
                parentNode[mapping.PrimaryJsonElement] = JsonValue.Create(ConvertToTargetType(transformedValue, mapping.TargetDataType));
            }
        }
    }

    private JsonObject GetOrCreateParentNode(JsonObject rootNode, string jsonParentPath)
    {
        var keys = jsonParentPath.Split("->");
        var current = rootNode;

        foreach (var key in keys)
        {
            if (!current.ContainsKey(key))
                current[key] = new JsonObject();

            current = (JsonObject)current[key];
        }

        return current;
    }

    private string ApplyTransform(string value, string transform)
    {
        if (string.IsNullOrEmpty(transform)) return value;

        if (transform.StartsWith("DateFormat:"))
        {
            var format = transform.Substring("DateFormat:".Length);
            if (DateTime.TryParse(value, out var date))
                return date.ToString(format);
        }
        else if (transform.StartsWith("Trim"))
        {
            var format = transform.Split("Trim:");
            if (format.Length > 1)
            {
                return value.Substring(0, int.Parse(format[1]));
            }

            return value.Trim();
        }

        return value; // Default: no transformation applied
    }

    private object ConvertToTargetType(string value, string targetDataType)
    {
        // Override with target data type if specified
        if (!string.IsNullOrEmpty(targetDataType))
        {
            return targetDataType.ToLower() switch
            {
                "int" => int.TryParse(value, out var intValue) ? intValue : value,
                "bool" => bool.TryParse(value, out var boolValue) ? boolValue : value,
                "decimal" => decimal.TryParse(value, out var decimalValue) ? decimalValue : value,
                "string" => value,
                _ => value // Default to string if unknown type
            };
        }

        // Auto-detect type if no override
        if (int.TryParse(value, out var inferredInt))
            return inferredInt;

        if (bool.TryParse(value, out var inferredBool))
            return inferredBool;

        if (decimal.TryParse(value, out var inferredDecimal))
            return inferredDecimal;

        return value; // Default to string
    }
}
