using System;
using System.Collections.Generic;

namespace SerialsApp.GUI
{
    public class SerialsCache
    {
        private readonly Dictionary<long, Tuple<long, long>> _cache = new Dictionary<long, Tuple<long, long>>();
        
        public void Add(long id, long seasonsWithEpisodes, long seasonsWithoutEpisodes)
        {
            _cache.Add(id, new Tuple<long, long>(seasonsWithEpisodes, seasonsWithoutEpisodes));
        }

        public bool Contains(long id) => _cache.ContainsKey(id);

        public Tuple<long, long> Get(long id) => _cache[id];
    }
}
