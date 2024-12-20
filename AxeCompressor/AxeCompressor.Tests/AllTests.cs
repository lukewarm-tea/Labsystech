namespace AxeCompressor.Tests;

public abstract class TestBase
{
    public static string ToString(IEnumerable<char> chars)
    {
        return new string(chars.ToArray());
    }
}





public class SimpleSerderTest : TestBase
{
    public void CanSerializeBasicExample()
    {
        IReadOnlyList<int> givenInput = [1, 25, 17, 299];
        string expectedOutput = "1 25 17 299";

        var serder = new SimpleSerder();
        var effectiveOutput = serder.Serialize(givenInput);
        var restoredInput = serder.Deserialize(effectiveOutput);

        Assert.Equal(expectedOutput, effectiveOutput);
        Assert.Equal(givenInput, restoredInput);
    }
}





public class ReverseRadixCodecTest : TestBase
{
    [Fact]
    public void PrintableAsciiAlphabetSizeAdheresToAsciiStandard()
    {
        const int numPrintableCharactersPerAsciiSpec = 95;
        Assert.Equal(numPrintableCharactersPerAsciiSpec, ReverseRadixCodec.PrintableAsciiAlphabet.Length);
    }

    [Fact]
    public void CanCorrectlyConvertBasicExamples()
    {
        var codec = ReverseRadixCodec.Default;
        IReadOnlyList<int> inputs = [0, 1, 2, 9, 10, 11, 94, 95, 96, 97, 111, 4321, 65536];
        IReadOnlyList<string> expectedOutputs = [" ", "!", "\"", ")", "*", "+", "~", " !", "!!", "\"!", "0!", "NM", "q8'"];

        IReadOnlyList<string> producedOutputs = inputs.Select(n => ToString(codec.Encode(n))).ToArray();

        Assert.Equal(expectedOutputs, producedOutputs);
    }
}





public class RangeAwareRadixSerderTest : TestBase
{
    [Fact]
    public void CanSerializeBasicExample()
    {
        IReadOnlyList<int> givenInput = [1, 25, 17, 299];
        string expectedOutput = "! 9 1 .#";

        var serder = RangeAwareRadixSerder.Default;
        var effectiveOutput = serder.Serialize(givenInput);
        var restoredInput = serder.Deserialize(effectiveOutput);

        Assert.Equal(expectedOutput, serder.Serialize(givenInput));
        Assert.Equal(givenInput, restoredInput);
    }
}





public class AxeSerderTest : TestBase
{
    [Fact]
    public void CanSerializeBasicExample()
    {
        IReadOnlyList<int> givenInput = [1, 25, 17, 299];
        string expectedOutput = " (9\",K";

        var serder = AxeSerder.Default;
        var effectiveOutput = serder.Serialize(givenInput);
        var restoredInput = serder.Deserialize(effectiveOutput);

        Assert.Equal(expectedOutput, serder.Serialize(givenInput));
        Assert.Equal(givenInput, restoredInput);
    }

    [Fact]
    public void CanReserializeBenchmarkSpecs()
    {
        var serder = AxeSerder.Default;

        foreach (var spec in BenchmarkSpecsSource.ForDefaultSerder)
        {
            IReadOnlyList<int> givenInput = spec.GetNumbers().ToList();
            var serializedValue = serder.Serialize(givenInput);
            var deserializedValue = serder.Deserialize(serializedValue);

            Assert.Equal(givenInput, deserializedValue);
        }
    }
}
