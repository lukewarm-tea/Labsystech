using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AxeCompressor;

/// <summary>
/// Сериализует и десериализует последовательность положительных чисел, используя указанный кодек.
/// Использует знание о максимальном диапазоне используемых в домене чисел чтобы уменьшить размер мериализованного представления.
/// </summary>
/// <param name="codec">Кодек для сериализации-десериализации каждого числа.</param>
/// <param name="minValue">Минимально возможное значение конвертируемого числа, включительно.</param>
/// <param name="maxValue">Максимально возможное значение конвертируемого числа, включительно.</param>
class RangeAwareRadixSerder(ReverseRadixCodec codec, int minValue, int maxValue) : ISerder
{
    // FIXME Убедиться что minValue < maxValue.

    public string Serialize(IEnumerable<int> numbers)
    {
        List<char> results = [];
        foreach (var number in numbers)
        {
            if (number < minValue || number > maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(numbers), "Number out of range");
            }
            // Центровка может уменьшить количество цифр.
            var centered = number - minValue;
            var serd = codec.Encode(centered).ToList();
            if (serd.Count > _charsPerNumber)
            {
                // FIXME Тестов для такого случая наверно будет достаточно... когда они появятся.
                throw new InvalidProgramException("Assertion failed");
            }
            while (serd.Count < _charsPerNumber)
            {
                serd.Add(codec.Alphabet[0]);
            }
            results.AddRange(serd);
        }
        return CollectionsMarshal.AsSpan(results).ToString();
    }

    public IEnumerable<int> Deserialize(string source)
    {
        foreach (var piece in source.Chunk(_charsPerNumber))
        {
            yield return codec.Decode(piece) + minValue;
        }
    }

    /// <summary>
    /// Сериализатор с настройками согласно условиями задачи.
    /// </summary>
    public static readonly RangeAwareRadixSerder Default = new(ReverseRadixCodec.Default, 0, 300);

    /// <summary>
    /// Максимальное количество цифр которое может встретиться в сериализованном числе.
    /// </summary>
    readonly int _charsPerNumber = (int)Math.Ceiling(Math.Log(maxValue - minValue + 1, codec.Alphabet.Length));
}
