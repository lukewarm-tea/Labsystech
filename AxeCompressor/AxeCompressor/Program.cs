using AxeCompressor;

const int ellipsisThreshold = 60;

string LimitWithEllipsis(string src)
{
    if (src.Length < ellipsisThreshold) return src;
    return string.Concat(src.AsSpan(0, ellipsisThreshold), "...");
}

string SerializeAndValidate(IReadOnlyList<int> numbers, ISerder serder)
{
    var data = serder.Serialize(numbers);
    var restored = serder.Deserialize(data);
    if (!Enumerable.SequenceEqual(numbers, restored))
    {
        throw new InvalidProgramException();
    }
    return data;
}

var baselineSerder = new SimpleSerder();
var compressingSerder = AxeSerder.Default;
var stats = new List<FractionStat>();

foreach (var spec in BenchmarkSpecsSource.ForDefaultSerder())
{
    var inputNumbers = spec.GetNumbers().ToList();
    var baselineData = SerializeAndValidate(inputNumbers, baselineSerder);
    var compressedData = SerializeAndValidate(inputNumbers, compressingSerder);
    var fraction = Math.Round(1.0 * compressedData.Length / baselineData.Length, 4);
    stats.Add(new(inputNumbers.Count, fraction));
    Console.WriteLine($"Несжато: {LimitWithEllipsis(baselineData)}");
    Console.WriteLine($"Сжато: {LimitWithEllipsis(compressedData)}");
    Console.WriteLine($"Размер: {fraction}");
    Console.WriteLine();
}

Console.WriteLine("Отчёт по сжатию (количество чисел — размер):");
foreach (var stat in stats)
{
    Console.WriteLine($"    {stat.SourceLen,8} — {stat.CompressionFraction}");
}







record struct FractionStat(int SourceLen, double CompressionFraction);
