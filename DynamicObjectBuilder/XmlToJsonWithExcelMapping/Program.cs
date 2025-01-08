
using System.Text.Json;

// Load mappings from Excel
var excelLoader = new ExcelLoader();
var mappings = excelLoader.LoadMappings("MappingFile.xlsx");

// Parse XML and generate JSON
var xmlContent = @"
<Root>
    <Customer>
        <Name>John Doe</Name>
        <Orders>
            <Order>
                <Id>1</Id>
                <Date>2025-01-01</Date>
                <AddressLine1>Main Street</AddressLine1>
            </Order>
            <Order>
                <Id>2</Id>
                <Date>2025-01-02</Date>
                <AddressLine1>Second Street</AddressLine1>
            </Order>
        </Orders>
    </Customer>
</Root>";

var transformer = new XmlToJsonWithExcelMapping(mappings);
var jsonResult = transformer.TransformXmlToJson(xmlContent);

Console.WriteLine(JsonSerializer.Serialize(jsonResult, new JsonSerializerOptions { WriteIndented = true }));
