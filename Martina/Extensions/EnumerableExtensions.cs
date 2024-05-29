namespace Martina.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// 含有索引的遍历
    /// </summary>
    /// <param name="enumerable">可遍历的接口</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<(T, int)> WithIndex<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.Select((value, index) => (value, index));
    }
}
