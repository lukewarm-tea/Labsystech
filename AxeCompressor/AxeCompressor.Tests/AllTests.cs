namespace AxeCompressor.Tests;

public class AxeSerderTest
{
    [Fact]
    public void PrintableAsciiAlphabetSizeAdheresToAsciiStandard()
    {
        const int numPrintableCharactersPerAsciiSpec = 95;
        Assert.Equal(numPrintableCharactersPerAsciiSpec, AxeSerder.PrintableAsciiAlphabet.Length);
    }

    [Fact]
    public void CanSerializeBasicExample()
    {
        IReadOnlyList<int> givenInput = [1, 25, 17, 299];
        string expectedOutput = " (9\",K";

        var serder = AxeSerder.Default;
        var effectiveOutput = serder.Serialize(givenInput);
        var restoredInput = serder.Deserialize(effectiveOutput);

        Assert.Equal(expectedOutput, effectiveOutput);
        Assert.Equal(givenInput, restoredInput);
    }

    [Fact]
    public void CanReserializeBenchmarkSpecs()
    {
        var serder = AxeSerder.Default;

        foreach (var spec in BenchmarkSpecsSource.ForDefaultSerder())
        {
            IReadOnlyList<int> givenInput = spec.GetNumbers().ToList();
            var serializedValue = serder.Serialize(givenInput);
            var deserializedValue = serder.Deserialize(serializedValue);

            Assert.Equal(givenInput, deserializedValue);
        }
    }
}
