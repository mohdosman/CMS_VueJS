using System.IO.Compression;
using System.Text.Json;
using System.Xml;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace CrisisManagement.Features.Suicides.Services;

// Reads the death record spreadsheet: the first worksheet, row 1 = column names, one record per row after that. Rows are
// streamed, because a sheet can claim a million formatted rows although only a few hundred hold data.
public static class SuicideWorkbookReader
{
    // The columns the import procedure reads. Provider is optional; the others must be there.
    public static readonly string[] RequiredColumns =
    [
        "DDeathStateCountry", "DNameFirst", "DNameMiddle", "DNameLast", "DSex", "DDODMo", "DDODDay", "DDODYr", "DDOBMo", "DDOBDay", "DDOBYr",
        "DSSN", "DResStateCountry", "DResCounty", "DUSArmedForces", "DDeathManner"
    ];
    private static readonly string[] KnownColumns = [.. RequiredColumns, "Provider"];

    private const int MaxRows = 50_000, MaxCellLength = 255;
    private const long MaxUnpackedBytes = 200L * 1024 * 1024;

    public sealed record Result(string Json, int RecordCount);

    // Problems are returned as messages for the user; null Result when there are any.
    public static (Result? Result, List<string> Problems) Read(byte[] content)
    {
        var problems = new List<string>();
        try
        {
            RejectUnsafePackage(content, problems);
            if (problems.Count > 0) return (null, problems);

            using var stream = new MemoryStream(content);
            using var doc = SpreadsheetDocument.Open(stream, false);
            var workbook = doc.WorkbookPart ?? throw new InvalidDataException("no workbook");
            var firstSheet = workbook.Workbook.Sheets?.Elements<Sheet>().FirstOrDefault() ?? throw new InvalidDataException("no worksheet");
            var sheet = (WorksheetPart)workbook.GetPartById(firstSheet.Id!.Value!);
            var strings = workbook.SharedStringTablePart?.SharedStringTable.Elements<SharedStringItem>().Select(s => s.InnerText).ToArray() ?? [];

            var records = new List<Dictionary<string, string?>>();
            var columns = new Dictionary<int, string>();   // column index -> known column name
            var rowNumber = 0;

            using var reader = OpenXmlReader.Create(sheet);
            while (reader.Read())
            {
                if (reader.ElementType != typeof(Row) || !reader.IsStartElement) continue;
                var row = (Row)reader.LoadCurrentElement()!;
                rowNumber++;
                var cells = row.Elements<Cell>().Select(c => (Index: ColumnIndex(c.CellReference?.Value), Text: CellText(c, strings))).Where(c => c.Index > 0).ToList();

                if (rowNumber == 1)
                {
                    foreach (var (index, text) in cells)
                    {
                        var known = KnownColumns.FirstOrDefault(k => k.Equals(text?.Trim(), StringComparison.OrdinalIgnoreCase));
                        if (known is not null && !columns.ContainsValue(known)) columns[index] = known;
                    }
                    var missing = RequiredColumns.Where(r => !columns.ContainsValue(r)).ToList();
                    if (missing.Count > 0)
                    {
                        problems.Add($"The first row must hold the column names. Missing: {string.Join(", ", missing)}.");
                        return (null, problems);
                    }
                    continue;
                }

                if (cells.All(c => string.IsNullOrWhiteSpace(c.Text))) continue;   // a formatted but empty row
                if (records.Count >= MaxRows) { problems.Add($"The file has more than {MaxRows:N0} records."); return (null, problems); }

                var record = KnownColumns.ToDictionary(k => k, _ => (string?)null);
                foreach (var (index, text) in cells)
                {
                    if (!columns.TryGetValue(index, out var name) || string.IsNullOrWhiteSpace(text)) continue;
                    if (text.Length > MaxCellLength) problems.Add($"Row {rowNumber}: {name} is longer than {MaxCellLength} characters.");
                    record[name] = text.Trim();
                }
                records.Add(record);
            }

            if (rowNumber == 0) { problems.Add("The first worksheet is empty."); return (null, problems); }
            if (records.Count == 0) problems.Add("Zero data rows found in the Excel file.");
            if (problems.Count > 0) return (null, problems.Take(10).ToList());
            return (new Result(JsonSerializer.Serialize(records), records.Count), problems);
        }
        catch (Exception ex) when (ex is OpenXmlPackageException or InvalidDataException or XmlException or IOException or FileFormatException or InvalidOperationException or ArgumentException or KeyNotFoundException)
        {
            return (null, ["Invalid .xlsx Excel file."]);
        }
    }

    // A real .xlsx is a zip package without macros; refuse anything else and packages that unpack to an unreasonable size.
    private static void RejectUnsafePackage(byte[] content, List<string> problems)
    {
        try
        {
            using var zip = new ZipArchive(new MemoryStream(content), ZipArchiveMode.Read);
            if (zip.Entries.Any(e => e.FullName.EndsWith("vbaProject.bin", StringComparison.OrdinalIgnoreCase))) problems.Add("File contains macros. File cannot be processed.");
            else if (zip.Entries.Sum(e => e.Length) > MaxUnpackedBytes) problems.Add("Invalid .xlsx Excel file.");
            else if (!zip.Entries.Any(e => e.FullName.Equals("xl/workbook.xml", StringComparison.OrdinalIgnoreCase))) problems.Add("Invalid .xlsx Excel file.");
        }
        catch (InvalidDataException)
        {
            problems.Add("Invalid .xlsx Excel file.");
        }
    }

    // "AB12" -> 28 (the column number, A = 1); 0 when the reference is missing.
    private static int ColumnIndex(string? reference)
    {
        var n = 0;
        foreach (var c in reference ?? "")
        {
            if (!char.IsAsciiLetter(c)) break;
            n = n * 26 + (char.ToUpperInvariant(c) - 'A' + 1);
        }
        return n;
    }

    private static string? CellText(Cell cell, string[] strings)
    {
        var raw = cell.CellValue?.Text;
        return cell.DataType?.Value switch
        {
            var t when t == CellValues.SharedString => int.TryParse(raw, out var i) && i >= 0 && i < strings.Length ? strings[i] : null,
            var t when t == CellValues.InlineString => cell.InlineString?.InnerText,
            var t when t == CellValues.Boolean => raw == "1" ? "TRUE" : "FALSE",
            _ => raw
        };
    }
}
