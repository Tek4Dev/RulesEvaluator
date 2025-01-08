using DynamicObjectBuilder.Json.DemoModel;
using DynamicObjectBuilder.WithSchema;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        //1.... for gen schema from class

        //await GenSchema();


        //2 . convert the input json to output json with few scrub transformers
        
       // ToJsonAndScrub();

        //return;

        //3..Dynamic Object Builder Example
        BuildDynamicObject();

        Console.ReadLine();
    }

    private static void ToJsonAndScrub()
    {
        //var myObject = new MyClass
        //{
        //    ShortString = "Short text",
        //    LongString = "This is a very long string that will be truncated",
        //    MyDateTime = DateTime.Now,
        //    PreciseDecimal = 12345.6789m
        //};

        var inputDto = new InputDto
        {
            MyDateTime = "2024-12-31 15:23:45", // Matches the format exactly
            ShortString = "This is a very long string",
            PreciseDecimal = "123.456789m",
            SomeNumber = "42"
        };

        //string jsonString = JsonSerializer.Serialize(myObject, options);
        //var serializedDto = CustomSerializer.Serialize(dto);
        var deserializedDto = CustomDeserializer.Deserialize<InputDto, OutputDto>(inputDto);



        Console.WriteLine($"MyDateTime: {deserializedDto.MyDateTime} (Type: {deserializedDto.MyDateTime.GetType()})");
        Console.WriteLine($"ShortString: {deserializedDto.ShortString} (Type: {deserializedDto.ShortString.GetType()})");
        Console.WriteLine($"PreciseDecimal: {deserializedDto.PreciseDecimal} (Type: {deserializedDto.PreciseDecimal.GetType()})");
        Console.WriteLine($"SomeNumber: {deserializedDto.SomeNumber} (Type: {deserializedDto.SomeNumber.GetType()})");

        //Console.WriteLine(dtoserializedDto);
        Console.ReadLine();
    }

    private async static Task GenSchema()
    {
        await SchemaDemo.GenSch();
    }

    private static void BuildDynamicObject()
    {
        Console.WriteLine("Dynamic Object Builder Example");

        // 1. Read the Excel file and load property definitions
        var definitions = ExcelReader.ReadDefinitions("model.xlsx");

        // 2. Build the class hierarchy from definitions
        var hierarchy = HierarchyBuilder.BuildHierarchy(definitions);

        // 3. Generate types dynamically
        var generatedTypes = DynamicTypeBuilder.BuildTypes(hierarchy);

        var validators = DynamicValidatorBuilder.GenerateValidators(generatedTypes.Types, hierarchy);

        // Create an instance of Department
        var department = generatedTypes.Instances.First().Value;

        //dynamic department = departmentInstance;
        //department.Name = "Asdad";
        //department.Location = "";

        department.GetType().GetProperty("Name")?.SetValue(department, ""); // Invalid: Empty string
        department.GetType().GetProperty("Location")?.SetValue(department, "Valid Location");

        //department.Employee[0].Age = 12;
        // Validate Department instance
        var res = DynamicObjectValidator.ValidateObject(department, validators);
        Console.WriteLine(res.Errors.FirstOrDefault()?.ErrorMessage);

        var deptVal = JsonSerializer.Serialize(department);

        File.WriteAllText("DemoModel.json", deptVal);

        Console.WriteLine("Generated types and validation process completed.");


        DemoModel? deptModel = JsonSerializer.Deserialize<DemoModel>(deptVal);
        deptModel!.Name = "test";
        deptModel!.Employee[0]!.Age = 12;

        Console.WriteLine("The new model is: " + JsonSerializer.Serialize(deptModel));
    }
}


public class InputDto
{
    public string MyDateTime { get; set; } // Received as string
    public string ShortString { get; set; } // Received as string
    public string PreciseDecimal { get; set; } // Received as string
    public string SomeNumber { get; set; } // Received as string
}

public class OutputDto
{
    [DateFormat("yyyy-MM-dd HH:mm:ss")]
    public DateTime MyDateTime { get; set; }

    [StringLength(10)]
    public string ShortString { get; set; }

    [DecimalPlaces(4)]
    public decimal PreciseDecimal { get; set; }

    public int SomeNumber { get; set; }
}