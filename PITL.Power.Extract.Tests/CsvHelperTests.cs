namespace PITL.Power.Extract.Tests;
public class CsvHelperTests
{
    private readonly CsvHelper _sut = new();

    [Fact]
    public void Path_Correct()
    {
        var path = _sut.GetExtractCsvPath(@"c:\temp", new DateTime(2026, 1, 12, 20, 7, 4));
        Assert.Equal(@"c:\temp\PowerPosition_20260112_2007.csv", path);
    }

    [Fact]
    public void Csv_Correct()
    {
        double[] volumes = [
            10, 10, 10, 10, 10, 10, 
            50, 50, 50, 50, 50, 50,
            100, 100, 100, 100, 100, 100,
            90, 90, 90, 90, 90, 90,
            120
        ];

        var csv = _sut.CreateExtractCsv(volumes);

        var expected = @"Local Time,Volume
23:00,10
00:00,10
01:00,10
02:00,10
03:00,10
04:00,10
05:00,50
06:00,50
07:00,50
08:00,50
09:00,50
10:00,50
11:00,100
12:00,100
13:00,100
14:00,100
15:00,100
16:00,100
17:00,90
18:00,90
19:00,90
20:00,90
21:00,90
22:00,90
23:00,120
";

        Assert.Equal(expected, csv);
    }
}
