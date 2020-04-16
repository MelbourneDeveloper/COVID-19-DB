﻿using System;
using System.Collections.Generic;

namespace Covid19DB.Services
{
    public class Cache<T> : ICache<T>
    {
        private readonly Dictionary<string, T> _dictionary = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

        public T Get(string key)
        {
            _dictionary.TryGetValue(key, out var item);
            return item;
        }

        public void Add(string key, T value)
        {
            _dictionary.Add(key, value);
        }
    }
}
