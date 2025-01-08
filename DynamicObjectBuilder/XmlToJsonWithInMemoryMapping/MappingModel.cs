
public class MappingModel
{
    public string PrimaryJsonElement { get; set; } // Name of the JSON property
    public string JsonParentElement { get; set; } // Hierarchical parent element
    public string XPath { get; set; }            // XPath for the XML element
    public bool IsArray { get; set; }            // Is it part of a list?
    public string Transform { get; set; }        // Transformation rule

    public string TargetDataType { get; set; } = "string";   // Optional target data type override

    public List<MappingModel> Mappings { get; set; } = new(); // Child mappings for nested structures
}
