namespace AxeCompressor;

/// <summary>
/// Алфавиты символов для использования в качестве числовой базы.
/// </summary>
static class RadixAlphabet
{
    /// <summary>
    /// Этот алфавит (для числовой базы) включает в себя все печатные символы из кодировки ASCII.
    /// </summary>
    public static readonly char[] PrintableAsciiAlphabet = Enumerable.Range(0, 127).Select(static c => (char)c).Where(static c => !char.IsControl(c)).ToArray();
}
