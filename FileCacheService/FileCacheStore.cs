using System;
using System.Collections.Generic;

namespace FileCacheService
{
    internal static class FileCacheStore
    {
        private static FileCache _cache;

        private static readonly object _sync;

        static FileCacheStore()
        {
            _cache = new FileCache();
            _sync = new object();
        }

        internal static IEnumerable<string> Get(string key)
        {
            if (!_cache.IsActual)
            {
                lock (_sync)
                {
                    if (!_cache.IsActual)
                    {
                        _cache = new FileCache();
                    }
                }
            }
            if (!_cache.Files.ContainsKey(key))
            {
                throw new ApplicationException($"An object with key {key} does not exists");
            }
            return _cache.Files[key];
        }
    }
}