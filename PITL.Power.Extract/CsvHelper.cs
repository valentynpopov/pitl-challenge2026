using System.Text;

namespace PITL.Power.Extract;

public interface ICsvHelper
{
    string GetExtractCsvPath(string outputDirectory, DateTime date);
    string CreateExtractCsv(double[] volumes);
}

public class CsvHelper: ICsvHelper
{
    public string GetExtractCsvPath(string outputDirectory, DateTime date) =>
        Path.Combine(outputDirectory, $"PowerPosition_{date:yyyyMMdd_HHmm}.csv");

    public string CreateExtractCsv(double[] volumes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Local Time,Volume");

        var time = new TimeOnly(23, 0);
        for (int i = 0; i < volumes.Length; i++)
        {
            sb.AppendLine($"{time:HH:mm},{volumes[i]}");
            time = time.AddHours(1);
        }
        return sb.ToString();
    }
}
