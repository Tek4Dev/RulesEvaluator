//using Newtonsoft.Json.Schema;
using NJsonSchema;
using NJsonSchema.Generation;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace DynamicObjectBuilder.WithSchema
{
  

    public class MyClass
    {
        [MaxLength(10)]
        [StringLength(10)]
        public string ShortString { get; set; }

        [MaxLength(20)]
        [StringLength(20)]
        public string LongString { get; set; }

        [Required]
        [DateFormat("yyyy-MM-dd HH:mm:ss")]
        [DataType(DataType.DateTime)]
        public DateTime MyDateTime { get; set; }

        [Range(0.0001, double.MaxValue)]
        [DecimalPlaces(4)]
        public decimal PreciseDecimal { get; set; }
    }

    public class SchemaDemo
    {
        public static async Task GenSch()
        {
            var schema = JsonSchema.FromType<MyClass>();
            var jsonSchema = schema.ToJson();
            System.IO.File.WriteAllText("schema.json", jsonSchema);
        }
    }

}
