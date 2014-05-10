#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Threading;
using FlitBit.Core;

namespace FlitBit.IoC.Containers
{
	internal partial class Container : Disposable, IContainer
	{
		readonly Lazy<ConcurrentDictionary<object, object>> _localCaches =
			new Lazy<ConcurrentDictionary<object, object>>(LazyThreadSafetyMode.PublicationOnly);

		#region IContainer Members

		public TCache EnsureCache<TKey, TCache>(TKey key, Func<TCache> factory)
		{
			if (_parent != null && _options.HasFlag(CreationContextOptions.InheritCache))
			{
				return _parent.EnsureCache(key, factory);
			}
			var caches = this._localCaches.Value;
			return (TCache) caches.GetOrAdd(key, ignored => factory());
		}

		public TCache EnsureCache<TKey, TCache>(TKey key)
			where TCache : new()
		{
			if (_parent != null && _options.HasFlag(CreationContextOptions.InheritCache))
			{
				return _parent.EnsureCache<TKey, TCache>(key);
			}
			var caches = this._localCaches.Value;
			return (TCache) caches.GetOrAdd(key, ignored => new TCache());
		}

		public bool TryGetCache<TKey, TCache>(TKey key, out TCache cache)
			where TCache : new()
		{
			if (_options.HasFlag(CreationContextOptions.EnableCaching))
			{
				var caches = _localCaches.Value;
				object instance;
				var result = caches.TryGetValue(key, out instance);
				cache = (TCache) instance;
				return result;
			}
			cache = default(TCache);
			return false;
		}		 
		
		#endregion

	}
}