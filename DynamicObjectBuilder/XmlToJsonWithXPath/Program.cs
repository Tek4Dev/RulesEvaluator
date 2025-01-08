
using System.Text.Json;

var xml = @"
<Root>
    <Customer>
        <Name>John Doe</Name>
        <Orders>
            <Order>
                <Id>1</Id>
                <Date>2025-01-01</Date>
            </Order>
            <Order>
                <Id>2</Id>
                <Date>2025-01-02</Date>
            </Order>
        </Orders>
    </Customer>
</Root>";

var mappings = new List<MappingModel>
{
    new MappingModel
    {
        XPath = "/Root/Customer/Name",
        JsonPath = "Customer.FullName"
    },
    new MappingModel
    {
        XPath = "/Root/Customer/Orders/Order",
        JsonPath = "Customer.Orders",
        IsArray = true,
        Mappings = new List<MappingModel>
        {
            new MappingModel { XPath = "Id", JsonPath = "OrderId" },
            new MappingModel { XPath = "Date", JsonPath = "OrderDate" }
        }
    }
};

var transformer = new XmlToJsonTransformer(mappings);
var jsonResult = transformer.TransformXmlToJson(xml);

Console.WriteLine(JsonSerializer.Serialize(jsonResult, new JsonSerializerOptions { WriteIndented = true }));
