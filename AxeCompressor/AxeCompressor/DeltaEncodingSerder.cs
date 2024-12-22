namespace AxeCompressor;

/// <summary>
/// Сериализатор, использующий сжатие на основе дельта-кодирования.
/// </summary>
/// <param name="alphabet">Алфавит символов, которые можно использовать для сериализации.</param>
/// <param name="maxValue">Максимально возможное значение конвертируемого числа, включительно.</param>
class DeltaEncodingSerder(char[] alphabet, int maxValue) : ISerder
{
    public IEnumerable<int> Deserialize(string source)
    {
        var currentValue = 0;
        foreach (var digit in source)
        {
            if (_dk.Terminal.FirstOrDefault(ds => ds.Digit == digit) is var termD && termD is { })
            {
                currentValue += termD.DeltaValue;
                yield return currentValue;
            }
            else if (_dk.Accumulative.FirstOrDefault(ds => ds.Digit == digit) is var accD && accD is { })
            {
                currentValue += accD.DeltaValue;
            }
            else
            {
                throw new ArgumentException("Invalid digit");
            }
        }
    }

    public string Serialize(IEnumerable<int> numbers)
    {
        IEnumerable<char> Iterate()
        {
            var lastValue = 0;
            var maxTermDelta = _dk.Terminal.Count - 1; // напоминаю, здесь индекс в массиве равняется размеру дельты
            foreach (var number in numbers.Order())
            {
                var deltaLeft = number - lastValue;
                lastValue = number;

                while (deltaLeft > maxTermDelta)
                {
                    foreach (var accD in _dk.Accumulative)
                    {
                        if (deltaLeft >= accD.DeltaValue)
                        {
                            deltaLeft -= accD.DeltaValue;
                            yield return accD.Digit;
                            break;
                        }
                    }
                }
                var termD = _dk.Terminal[deltaLeft];
                yield return termD.Digit;
            }
        }
        return new string(Iterate().ToArray());
    }

    readonly DeltaKit _dk = GenerateOptimalDeltaKit(alphabet, maxValue);

    /// <summary>
    /// Сериализатор с настройками согласно условиями задачи.
    /// </summary>
    public static DeltaEncodingSerder Default { get => new(RadixAlphabet.PrintableAsciiAlphabet, 300); }

    /// <summary>
    /// Мы хотим сгенерировать набор дельт, который валиден, т.е.
    /// в нём достаточно аккумулятивных дельт чтобы преодолеть максимальный диапазон значений,
    /// но при этом мы хотим чтобы в наборе было минимум аккумулятивных дельт, чтобы не тратить алфавит по чём зря.
    /// </summary>
    /// <returns></returns>
    static DeltaKit GenerateOptimalDeltaKit(IReadOnlyList<char> alphabet, int maxValue)
    {
        foreach (var dk in GenerateCandidateDeltaKits(alphabet))
        {
            var maxTotalDelta = dk.Accumulative.Sum(ds => ds.DeltaValue) + dk.Terminal[^1].DeltaValue;
            if (maxTotalDelta >= maxValue)
            {
                return dk;
            }
        }
        // Не должно происходить с нашими условиями.
        throw new ArgumentException("Task is not possible with given parameters");
    }

    /// <summary>
    /// Сгенерировать потенциальные наборы дельт, перебирая все пропорции между терминальными и аккумулятивными дельтами.
    /// </summary>
    /// <param name="alphabet"></param>
    /// <param name="maxValue"></param>
    /// <returns></returns>
    static IEnumerable<DeltaKit> GenerateCandidateDeltaKits(IReadOnlyList<char> alphabet)
    {
        var minAccs = 1; // Хотя бы 1 цифру хочется иметь в качестве аккумулятора, для работы с большими ддельтами.
        var maxAccs = alphabet.Count - 1; // Если весь алфавит исопльзовать на большие дельты, то чем кодировать точные диапазоны?
        for (var nAccs = minAccs; nAccs < maxAccs; nAccs++)
        {
            var nTerms = alphabet.Count - nAccs;
            var digitsLeft = alphabet.ToList();
            var gap = nTerms;
            var accs = new List<DeltaSpec>();
            var terms = new List<DeltaSpec>();
            for (var ix = 0; ix < nAccs; ix++)
            {
                var digit = digitsLeft[^1];
                digitsLeft.RemoveAt(digitsLeft.Count - 1);
                var deltaValue = gap;
                gap *= 2;
                accs.Add(new(digit, deltaValue));
            }
            accs.Reverse();
            foreach (var (ix, digit) in digitsLeft.Index())
            {
                terms.Add(new(digit, ix));
            }
            yield return new(terms, accs);
        }
    }
}

/// <summary>
/// Связь символа алфавита с дельтой.
/// </summary>
/// <param name="Digit">Цифра из алфавита.</param>
/// <param name="DeltaValue">Дельта, которую этот символ производит.</param>
record class DeltaSpec(char Digit, int DeltaValue);

/// <summary>
/// Наборт дельт, используемых для дельта-кодировки.
/// Мы стараемся эффективно исопльзовать весь алфафит, не прибегая к манипуляции битами.
/// Поэтому в наборе есть 2 вида дельт: для нормальной кодировки, и для больших диапазонов.
/// </summary>
/// <param name="Terminal">
///     Эти дельты используются один раз, т.е. сразу производят следующее число.
///     Отсортированы по возрастанию, индекс дельты равняется её значению.
/// </param>
/// <param name="Accumulative">
///     Эти дельты накопительные, т.е. не производят число сразу, а используются для помощи с преодолением очень больших разрывов.
///     Отсортированы по убыванию.
/// </param>
record class DeltaKit(IReadOnlyList<DeltaSpec> Terminal, IReadOnlyList<DeltaSpec> Accumulative);
