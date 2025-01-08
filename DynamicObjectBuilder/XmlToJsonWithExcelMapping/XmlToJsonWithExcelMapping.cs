
using System.Xml.Linq;
using System.Xml.XPath;
using System.Text.Json.Nodes;

public class XmlToJsonWithExcelMapping
{
    private readonly List<ExcelMapping> _mappings;

    public XmlToJsonWithExcelMapping(List<ExcelMapping> mappings)
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

    private void ProcessMapping(XDocument xDocument, JsonObject rootNode, ExcelMapping mapping)
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
                        itemNode[childMapping.JsonPath] = childValue;
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
                parentNode[mapping.PrimaryJsonElement] = ApplyTransform(value, mapping.Transform);
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
        else if (transform == "Trim")
        {
            return value?.Trim();
        }

        return value;
    }
}
