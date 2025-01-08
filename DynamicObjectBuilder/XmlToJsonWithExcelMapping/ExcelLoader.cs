
using OfficeOpenXml;

public class ExcelLoader
{
    public List<ExcelMapping> LoadMappings(string filePath)
    {
        var mappings = new List<ExcelMapping>();

        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++) // Start from row 2 (skip headers)
            {
                mappings.Add(new ExcelMapping
                {
                    PrimaryJsonElement = worksheet.Cells[row, 1].Text,
                    JsonParentElement = worksheet.Cells[row, 2].Text,
                    XPath = worksheet.Cells[row, 3].Text,
                    IsArray = bool.TryParse(worksheet.Cells[row, 4].Text, out var isArray) && isArray,
                    Transform = worksheet.Cells[row, 5].Text
                });
            }
        }

        return mappings;
    }
}
