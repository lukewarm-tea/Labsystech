namespace AxeCompressor.Tests;

public class TestBase
{
    public static void AssertEqualSorted(IEnumerable<int> a, IEnumerable<int> b)
    {
        var aa = a.Order().ToList();
        var bb = b.Order().ToList();
        Assert.Equal(aa, bb);
    }
}

public class RadixAlphabetTest : TestBase
{
    [Fact]
    public void PrintableAsciiAlphabetSizeAdheresToAsciiStandard()
    {
        const int numPrintableCharactersPerAsciiSpec = 95;
        Assert.Equal(numPrintableCharactersPerAsciiSpec, RadixAlphabet.PrintableAsciiAlphabet.Length);
    }
}

public class AxeSerderTest : TestBase
{
    [Fact]
    public void CanReserializeBenchmarkSpecs()
    {
        var serder = BitRechunkingSerder.Default;

        foreach (var spec in BenchmarkSpecsSource.ForDefaultSerder())
        {
            IReadOnlyList<int> givenInput = spec.GetNumbers().ToList();
            var serializedValue = serder.Serialize(givenInput);
            var reserializedInput = serder.Deserialize(serializedValue);

            AssertEqualSorted(givenInput, reserializedInput);
        }
    }
}

public class DeltaSerderTest : TestBase
{
    [Fact]
    public void CanReserializeBenchmarkSpecs()
    {
        var serder = DeltaEncodingSerder.Default;

        foreach (var spec in BenchmarkSpecsSource.ForDefaultSerder())
        {
            IReadOnlyList<int> givenInput = spec.GetNumbers().ToList();
            var serializedValue = serder.Serialize(givenInput);
            var reserializedInput = serder.Deserialize(serializedValue);

            AssertEqualSorted(givenInput, reserializedInput);
        }
    }
}
