using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxeCompressor;

static class BenchmarkSpecsSource
{
    public static IReadOnlyList<BenchmarkSpec> ForDefaultSerder()
    {
        var seed = 42;
        return [
            new(1.0f, () => RandomSequence(seed++, 0, 300, 1)),
            new(1.0f, () => RandomSequence(seed++, 0, 300, 2)),
            new(1.0f, () => RandomSequence(seed++, 0, 300, 3)),
            new(1.0f, () => RandomSequence(seed++, 0, 300, 4)),
            new(1.0f, () => RandomSequence(seed++, 0, 300, 5)),
            new(1.0f, () => RandomSequence(seed++, 0, 300, 6)),
            new(1.0f, () => RandomSequence(seed++, 0, 300, 7)),
            new(1.0f, () => RandomSequence(seed++, 0, 300, 8)),
            new(1.0f, () => RandomSequence(seed++, 0, 300, 9)),
            new(1.0f, () => RandomSequence(seed++, 0, 300, 50)),
            new(1.0f, () => RandomSequence(seed++, 0, 300, 100)),
            new(1.0f, () => RandomSequence(seed++, 0, 300, 500)),
            new(1.0f, () => RandomSequence(seed++, 0, 300, 1000)),
            new(1.0f, () => InclusiveRange(0, 9)),
            new(1.0f, () => InclusiveRange(10, 99)),
            new(1.0f, () => InclusiveRange(100, 299)),
            new(1.0f, () => Enumerable.Repeat(InclusiveRange(0, 299), 3).SelectMany(n => n)),
        ];
    }


    /// <summary>
    /// Детерминистичная случайная последовательность, чтобы не загрязнять тесты непредсказуемым поведением.
    /// </summary>
    static IEnumerable<int> RandomSequence(int seed, int minValue, int maxValue, int count)
    {
        var maxValueExclusive = maxValue + 1;
        var rng = new Random(seed);
        return Enumerable.Range(0, count).Select(ix => rng.Next(minValue, maxValueExclusive));
    }

    static IEnumerable<int> InclusiveRange(int minValue, int maxValue)
    {
        for (var ix = minValue; ix <= maxValue; ix++)
        {
            yield return ix;
        }
    }
}

record class BenchmarkSpec(float CompressionRatio, Func<IEnumerable<int>> GetNumbers);
