using System.Text.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        // Sample XML
        var xmlContent = File.ReadAllText("InputXml.xml");

        // Define the in-memory mappings
        var mappings = new List<MappingModel>
                {
                    new MappingModel
                    {
                        PrimaryJsonElement = "FullName",
                        JsonParentElement = "Customer",
                        XPath = "/Root/Customer/Name",
                        IsArray = false,
                        Transform = "Trim:10"
                    },
                    new MappingModel
                    {
                        PrimaryJsonElement = "Id",
                        JsonParentElement = "Dept",
                        XPath = "/Root/Department/Id",
                        TargetDataType = "int"
                    },
                     new MappingModel
                    {
                        PrimaryJsonElement = "Active",
                        JsonParentElement = "Dept",
                        XPath = "/Root/Department/IsActive",
                        TargetDataType = "bool"
                    },
                    new MappingModel
                    {
                        PrimaryJsonElement = "Orders",
                        JsonParentElement = "Customer",
                        XPath = "/Root/Customer/Orders/Order",
                        IsArray = true,
                        Mappings = new List<MappingModel>
                        {
                            new MappingModel
                            {
                                PrimaryJsonElement = "OrderId",
                                JsonParentElement = "Orders",
                                XPath = "Id",
                                IsArray = false
                            },
                            new MappingModel
                            {
                                PrimaryJsonElement = "OrderDate",
                                JsonParentElement = "Orders",
                                XPath = "Date",
                                IsArray = false,
                                Transform = "DateFormat:yyyy-MM-dd"
                            },
                            new MappingModel
                            {
                                PrimaryJsonElement = "AddressLine1",
                                JsonParentElement = "Orders",
                                XPath = "AddressLine1",
                                IsArray = false
                            }
                        }
                    }
                };

        // Instantiate the transformer and generate JSON
        var transformer = new XmlToJsonTransformer(mappings);
        var jsonResult = transformer.TransformXmlToJson(xmlContent);

        // Print the output JSON
        var jsonStr = JsonSerializer.Serialize(jsonResult, new JsonSerializerOptions { WriteIndented = true });

        //var deptVal = JsonSerializer.Serialize(jsonStr);


        File.WriteAllText("DemoModel.json", jsonStr);

        Console.WriteLine("Generated types and validation process completed.");


        var deptModel = JsonSerializer.Deserialize<XmlToJsonWithInMemoryMapping.Json.DemoModel.DemoModel>(jsonStr);

        var name = deptModel.Dept.Active;

        Console.WriteLine(jsonStr);

        Console.ReadLine();

    }
}