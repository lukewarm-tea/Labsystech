using AxeCompressor;

var baselineSerder = new SimpleSerder();
var compressingSerder = AxeSerder.Default;


foreach (var spec in BenchmarkSpecsSource.ForDefaultSerder)
{
    var inputNumbers = spec.GetNumbers().ToList();
    var baselineSrc = baselineSerder.Serialize(inputNumbers);
    var compressedSrc = compressingSerder.Serialize(inputNumbers);
    Console.WriteLine($"Uncompressed: {baselineSrc}");
    Console.WriteLine($"Compressed: {compressedSrc}");
    Console.WriteLine($"Ratio: {1.0 * baselineSrc.Length / compressedSrc.Length}");
    Console.WriteLine($"Fraction: {1.0 * compressedSrc.Length / baselineSrc.Length}");
    Console.WriteLine();
}
