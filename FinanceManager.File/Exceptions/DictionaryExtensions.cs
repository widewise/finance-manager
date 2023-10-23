namespace FinanceManager.File.Exceptions;

internal static class DictionaryExtensions
{
    
    public static void Merge<TKey, TValue>(
        this Dictionary<TKey, TValue> first,
        Dictionary<TKey, TValue> second) where TKey : notnull
    {
        foreach (var secondPair in second.Where(secondPair => !first.ContainsKey(secondPair.Key)))
        {
            first.Add(secondPair.Key, secondPair.Value);
        }
    }
}