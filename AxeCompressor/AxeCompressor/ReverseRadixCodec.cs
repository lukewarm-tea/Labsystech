using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AxeCompressor;

/// <summary>
/// Форматирует и читает числа в заданной базе любого размера, используя запись от младших разрядов к старшим.
/// Разряды развёрнуты чтобы избежать лишней работы при конвертации обратно из строки.
/// Этот кодек используется как часть алгоритма сжатия, он не предназначен для интерпретации чисел математически.
/// </summary>
/// <param name="alphabet">Алфавит базы, что определяет так же и её размер.</param>
class ReverseRadixCodec(char[] alphabet)
{
    public char[] Alphabet { get; } = alphabet;

    /// <summary>
    /// Отформатировать число в базу.
    /// </summary>
    /// <param name="number"></param>
    /// <returns>Строковое выражение входящего числа в базе этого кодека, начиная с младших разрядов.</returns>
    public IEnumerable<char> Encode(int number)
    {
        while (number >= Alphabet.Length)
        {
            // FIXME негативные числа дают негативный остаток
            var remainder = number % Alphabet.Length;
            yield return Alphabet[remainder];
            number /= Alphabet.Length;
        }
        yield return Alphabet[number];
    }

    public int Decode(IReadOnlyList<char> source)
    {
        var result = 0;
        var rankMul = 1;
        foreach (var digit in source)
        {
            var value = Array.IndexOf(Alphabet, digit);
            if (value == -1)
            {
                throw new ArgumentException("Invalid character found", nameof(source));
            }
            result += value * rankMul;
            rankMul *= Alphabet.Length;
        }
        return result;
    }

    /// <summary>
    /// Этот алфавит (для числовой базы) включает в себя все печатные символы из кодировки ASCII.
    /// </summary>
    public static readonly char[] PrintableAsciiAlphabet = Enumerable.Range(0, 127).Select(static c => (char)c).Where(static c => !char.IsControl(c)).ToArray();

    /// <summary>
    /// Кодек-по умолчанию, использует алфавит из всех печатных символов ASCII.
    /// </summary>
    public static readonly ReverseRadixCodec Default = new(PrintableAsciiAlphabet);
}
