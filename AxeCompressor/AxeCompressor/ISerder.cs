
namespace AxeCompressor;

/// <summary>
/// Сериализатор-десериализатор последовательности чисел в форму строки и обратно.
/// </summary>
internal interface ISerder
{
    /// <summary>
    /// Десериализует последовательность чисел из строки.
    /// Строка должна быть произведена способом, совместимым с этим конкретным экземпляром сериализатора, иначе результат может быть некорректным.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    IEnumerable<int> Deserialize(string source);

    /// <summary>
    /// Сериализовать последовательность чисел в строку, согласно алгоритму и параметрам этого конкретного экземпляра сериализатора.
    /// </summary>
    /// <param name="numbers"></param>
    /// <returns></returns>
    string Serialize(IEnumerable<int> numbers);
}
