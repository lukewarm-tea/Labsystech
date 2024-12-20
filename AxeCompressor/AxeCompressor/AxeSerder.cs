using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AxeCompressor;

/// <summary>
/// Сериализует и десериализует используя агресивное сжатие за счёт конкатенации на битовом уровне, с учётом домена чисел.
/// </summary>
/// <remarks>
/// Конструктор.
/// </remarks>
/// <param name="alphabet">
///     Алфавит символов, которые можно использовать для сериализации.
///     Реально используемое количество символов будет ограничено ближайшей степенью двойки.
/// </param>
/// <param name="minValue">Минимально возможное значение конвертируемого числа, включительно.</param>
/// <param name="maxValue">Максимально возможное значение конвертируемого числа, включительно.</param>
class AxeSerder(char[] alphabet, int minValue, int maxValue) : ISerder
{
    public string Serialize(IEnumerable<int> numbers)
    {
        IEnumerable<int> IterateSourceBits()
        {
            foreach (var rawNumber in numbers)
            {
                if (rawNumber < minValue || rawNumber > maxValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(numbers));
                }
                var number = rawNumber - minValue; // центровка сэкономит биты, но нужно не забыть сдвинуть обратно при десериализации
                yield return number;
            }
        }
        var outChars = new List<char>();
        foreach (var digitValue in RechunkBits(IterateSourceBits(), _bitsPerNumber, _bitsPerDigit, BitTailHandling.Keep))
        {
            outChars.Add(alphabet[digitValue]);
        }
        return CollectionsMarshal.AsSpan(outChars).ToString();
    }

    public IEnumerable<int> Deserialize(string source)
    {
        IEnumerable<int> IterateSourceBits()
        {
            foreach (var digit in source)
            {
                var digitValue = Array.IndexOf(alphabet, digit);
                if (digitValue == -1)
                {
                    throw new ArgumentException("Invalid character found", nameof(source));
                }
                yield return digitValue;
            }
        }
        foreach (var number in RechunkBits(IterateSourceBits(), _bitsPerDigit, _bitsPerNumber, BitTailHandling.Discard))
        {
            yield return number;
        }
    }

    /// <summary>
    /// Сериализатор с настройками согласно условиями задачи.
    /// </summary>
    public static AxeSerder Default { get => new(PrintableAsciiAlphabet, 0, 300); }

    /// <summary>
    /// Этот алфавит (для числовой базы) включает в себя все печатные символы из кодировки ASCII.
    /// </summary>
    public static readonly char[] PrintableAsciiAlphabet = Enumerable.Range(0, 127).Select(static c => (char)c).Where(static c => !char.IsControl(c)).ToArray();


    /// <summary>
    /// Склеить пооследовательность N-битных чисел и разрезать её на последовательность M-битных чисел.
    /// </summary>
    /// <param name="src">Исходные числа.</param>
    /// <param name="bitsPerSrc">Сколько бит в исходных числах.</param>
    /// <param name="bitsPerDest">Сколько должно быть бит в исходящих числах.</param>
    /// <param name="tailHandling">Сохранять ли хвост, образующийся из-за несоответствия битовой ширины чисел?</param>
    /// <returns>Выходные числа новой битовой ширины.</returns>
    static IEnumerable<int> RechunkBits(IEnumerable<int> src, int bitsPerSrc, int bitsPerDest, BitTailHandling tailHandling)
    {
        var srcMask = LowBitmask(bitsPerSrc);
        var destMask = LowBitmask(bitsPerDest);
        var bitQueue = 0;
        var queueLen = 0;
        foreach (var srcValue in src)
        {
            if (srcValue != (srcValue & srcMask))
            {
                throw new InvalidProgramException("Assertion failed");
                // Ну либо InvalidProgram.
                // Так или иначе, не должно случаться.
            }
            bitQueue <<= bitsPerSrc;
            queueLen += bitsPerSrc;
            bitQueue += srcValue;
            while (queueLen >= bitsPerDest)
            {
                var depth = queueLen - bitsPerDest;
                var destValueDeep = bitQueue & (destMask << depth);
                bitQueue &= ~destValueDeep;
                var destValue = destValueDeep >> depth;
                queueLen -= bitsPerDest;
                yield return destValue;
            }
        }
        // От некоторых чисел остаётся короткий хвост, его нужно натянуть на размер цифры, иначе он пропадёт.
        // Хвост получается из-за несовпадения размера кусков (входящих супротив исходящих).
        if ((tailHandling == BitTailHandling.Keep) & (queueLen != 0))
        {
            var depth = queueLen - bitsPerDest;
            var destValue = bitQueue << -depth;
            yield return destValue;
        }
    }

    enum BitTailHandling
    {
        Keep,
        Discard,
    }

    /// <summary>
    /// Создать битовую маску, где указанное количество нижних бит будут включены.
    /// </summary>
    /// <param name="bitCount">Количество бит которые должны быть включеными.</param>
    /// <returns>Битовая маска в виде 32-битного целого числа.</returns>
    static int LowBitmask(int bitCount)
    {
        uint mask = 0xffffffff;
        mask >>= 32 - bitCount;
        return (int)mask;
    }

    readonly int _bitsPerNumber = (int)Math.Ceiling(Math.Log2(maxValue - minValue + 1));
    readonly int _bitsPerDigit = (int)Math.Floor(Math.Log2(alphabet.Length ));
}
