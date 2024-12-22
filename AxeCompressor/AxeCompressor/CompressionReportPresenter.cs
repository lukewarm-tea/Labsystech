namespace AxeCompressor;

/// <summary>
/// Генерирует и выводит отчёт о эффективности сжатия сериализаторов.
/// </summary>
/// <param name="console">Устройство вывода.</param>
/// <param name="serders">Сериализаторы на проверку. Первый будет эталоном для оценки сжатия.</param>
class CompressionReportPresenter(TextWriter console, IReadOnlyList<ISerder> serders)
{
    public void AnalyzeAndPresent()
    {
        var stats = new List<CompressionDatum>();
        foreach (var (specIx, spec) in BenchmarkSpecsSource.ForDefaultSerder().Index())
        {
            console.WriteLine($"Тест {specIx}");
            var inputNumbers = spec.GetNumbers().ToList();
            var sizeFractions = new List<double>();
            int? baselineLen = null;
            foreach (var (serderIx, serder) in serders.Index())
            {
                var data = serder.Serialize(inputNumbers);
                console.WriteLine($"Сериализатор {serderIx}: {LimitWithEllipsis(data)}");

                if (baselineLen is int bl)
                {
                    sizeFractions.Add(1.0 * data.Length / bl);
                }
                else
                {
                    baselineLen = data.Length;
                }
            }
            stats.Add(new(inputNumbers.Count, sizeFractions));
            console.WriteLine();
        }

        console.WriteLine("Отчёт по сжатию (количество чисел — размеры):");
        foreach (var stat in stats)
        {
            IReadOnlyList<string> cols = [stat.SourceLength.ToString(), .. stat.SizeFractions.Select(sf => Math.Round(sf, 3).ToString())];
            console.WriteLine(" — " + string.Join(" — ", cols.Select(c => c.PadRight(columnWidth))));
        }
        console.WriteLine();

        console.WriteLine($"Использовались: {string.Join(", ", serders.Select(s => s.GetType().Name))}");
        console.WriteLine();
    }

    const int columnWidth = 7;
    const int ellipsisThreshold = 60;

    static string LimitWithEllipsis(string src)
    {
        if (src.Length < ellipsisThreshold) return src;
        return string.Concat(src.AsSpan(0, ellipsisThreshold), "...");
    }

    record struct CompressionDatum(int SourceLength, IReadOnlyList<double> SizeFractions);
}
