﻿namespace DistributedStorage.Storage
{
    using System.Collections.Generic;
    using System.Linq;

    public static class ContainerExtensions
    {
        public static IEnumerable<KeyValuePair<TKey, TValue>> GetKeysAndValues<TKey, TValue>(this IContainer<TKey, TValue> container)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var key in container.GetKeys())
                if (container.TryGet(key, out var value))
                    yield return new KeyValuePair<TKey, TValue>(key, value);
        }

        public static IEnumerable<TValue> GetValues<TKey, TValue>(this IContainer<TKey, TValue> container) => container.GetKeysAndValues().Select(kvp => kvp.Value);

        /// <summary>
        /// Adapts this <see cref="IDictionary{TKey, TValue}"/> into an <see cref="IFactoryContainer{TKey, TValue}"/>
        /// </summary>
        public static IContainer<TKey, TValue> ToContainer<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) => new Container<TKey, TValue>(new Container<TKey, TValue>.Options
        {
            GetKeys = () => dictionary.Keys,
            TryAdd = (key, value) =>
            {
                if (dictionary.ContainsKey(key))
                    return false;
                dictionary[key] = value;
                return true;
            },
            TryGet = dictionary.TryGetValue,
            TryRemove = dictionary.Remove
        });
    }
}
