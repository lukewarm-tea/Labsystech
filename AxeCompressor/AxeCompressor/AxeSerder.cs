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
class AxeSerder : ISerder
{
    // FIXME Убедиться что minValue < maxValue.

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="alphabet">
    ///     Алфавит символов, которые можно использовать для сериализации.
    ///     Реально используемое количество символов будет ограничено ближайшей степенью двойки.
    /// </param>
    /// <param name="minValue">Минимально возможное значение конвертируемого числа, включительно.</param>
    /// <param name="maxValue">Максимально возможное значение конвертируемого числа, включительно.</param>
    public AxeSerder(char[] alphabet, int minValue, int maxValue)
    {
        _alphabet = alphabet;
        _minValue = minValue;
        _maxValue = maxValue;
        _bitsPerNumber = (int)Math.Ceiling(Math.Log2(maxValue - minValue + 1));
        _bitsPerDigit = (int)Math.Floor(Math.Log2(alphabet.Length - 1));
        _numberMask = LowBitmask(_bitsPerNumber);
        _digitMask = LowBitmask(_bitsPerDigit);
    }

    public string Serialize(IEnumerable<int> numbers)
    {
        var bitQueue = 0;
        var queueLen = 0;
        var outChars = new List<char>();
        foreach (var rawNumber in numbers)
        {
            if (rawNumber < _minValue || rawNumber > _maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(numbers));
            }
            var number = rawNumber - _minValue; // центровка сэкономит биты, но нужно не забыть сдвинуть обратно при десериализации
            if (number != (number & _numberMask))
            {
                throw new InvalidProgramException("Assertion failed");
            }
            bitQueue <<= _bitsPerNumber;
            queueLen += _bitsPerNumber;
            bitQueue += number & _numberMask;
            while (queueLen >= _bitsPerDigit)
            {
                var offsetDepth = queueLen - _bitsPerDigit;
                var digitValueDeep = bitQueue & (_digitMask << offsetDepth);
                bitQueue &= ~digitValueDeep;
                var digitValue = digitValueDeep >> offsetDepth;
                queueLen -= _bitsPerDigit;
                outChars.Add(_alphabet[digitValue]);
            }
        }
        // От некоторых чисел остаётся короткий хвост, его нужно натянуть на размер цифры, иначе он пропадёт.
        if (queueLen != 0)
        {
            var offsetDepth = queueLen - _bitsPerDigit;
            var digitValue = bitQueue << -offsetDepth;
            outChars.Add(_alphabet[digitValue]);
        }
        return CollectionsMarshal.AsSpan(outChars).ToString();
    }

    public IEnumerable<int> Deserialize(string source)
    {
        var bitQueue = 0;
        var queueLen = 0;
        foreach (var digit in source)
        {
            var digitValue = Array.IndexOf(_alphabet, digit);
            if (digitValue == -1)
            {
                throw new ArgumentException("Invalid character found", nameof(source));
            }
            bitQueue <<= _bitsPerDigit;
            queueLen += _bitsPerDigit;
            bitQueue += digitValue & _digitMask;
            while (queueLen >= _bitsPerNumber)
            {
            var offsetDepth = queueLen - _bitsPerNumber;
                var numberDeep = (bitQueue & (_numberMask << offsetDepth));
                bitQueue &= ~numberDeep;
                var number = numberDeep >> offsetDepth;
                queueLen -= _bitsPerNumber;
                yield return number;
            }
        }
        // От некоторых цифр здесь могут остаться ещё биты в очереди.
        // Мы их игнорируем, пушто это был паддинг для обрубка последнего исходного числа.
    }

    /// <summary>
    /// Сериализатор с настройками согласно условиями задачи.
    /// </summary>
    public static readonly AxeSerder Default = new(ReverseRadixCodec.PrintableAsciiAlphabet, 0, 300);

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

    readonly char[] _alphabet;
    readonly int _minValue;
    readonly int _maxValue;
    readonly int _bitsPerNumber;
    readonly int _bitsPerDigit;
    readonly int _numberMask;
    readonly int _digitMask;
}
