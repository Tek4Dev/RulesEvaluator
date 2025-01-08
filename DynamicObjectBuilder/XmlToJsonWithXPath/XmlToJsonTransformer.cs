
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
        if (mapping.IsArray)
        {
            var elements = xDocument.XPathSelectElements(mapping.XPath);
            var jsonArray = new JsonArray();

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

            rootNode[mapping.JsonPath] = jsonArray;
        }
        else
        {
            var value = xDocument.XPathSelectElement(mapping.XPath)?.Value;
            if (!string.IsNullOrEmpty(value))
            {
                rootNode[mapping.JsonPath] = value;
            }
        }
    }
}
