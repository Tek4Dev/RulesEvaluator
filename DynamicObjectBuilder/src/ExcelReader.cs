using System;
using System.Collections.Generic;
using ClosedXML.Excel;

public class ExcelClassDefinition
{
    public string ParentClassName { get; set; }
    public string ClassName { get; set; }
    public string PropertyName { get; set; }
    public string PropertyType { get; set; }
    public int? MaxLength { get; set; }
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    public bool IsRequired { get; set; } = false;
}

public static class ExcelReader
{
    public static List<ExcelClassDefinition> ReadDefinitions(string filePath)
    {
        var definitions = new List<ExcelClassDefinition>();

        using (var workbook = new XLWorkbook(filePath))
        {
            var worksheet = workbook.Worksheet(1);

            for (int row = 2; row <= worksheet.LastRowUsed().RowNumber(); row++)
            {
                var d = worksheet.Cell(row, 7).GetValue<string>();
                definitions.Add(new ExcelClassDefinition
                {
                    ParentClassName = worksheet.Cell(row, 1).GetValue<string>(),
                    ClassName = worksheet.Cell(row, 2).GetValue<string>(),
                    PropertyName = worksheet.Cell(row, 3).GetValue<string>(),
                    PropertyType = worksheet.Cell(row, 4).GetValue<string>(),
                    MaxLength = worksheet.Cell(row, 5).GetValue<int?>(),
                    IsRequired = (worksheet.Cell(row, 6).GetValue<string>().ToUpper() == "TRUE") ? true : false

                });
            }
        }

        return definitions;
    }
}
