namespace AxeCompressor;

/// <summary>
/// Простой сериализатор-десериализатор последовательности чисел.
/// Разделяет числа пробелом.
/// </summary>
class SimpleSerder : ISerder
{
    public IEnumerable<int> Deserialize(string source)
    {
        return source.Split(' ').Select(x => int.Parse(x));
    }

    public string Serialize(IEnumerable<int> numbers)
    {
        return string.Join(' ', numbers.Select(n => n.ToString()));
    }
}
