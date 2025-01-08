
public class MappingModel
{
    public string XPath { get; set; }
    public string JsonPath { get; set; }
    public bool IsArray { get; set; } = false;
    public List<MappingModel> Mappings { get; set; } = new();
}
