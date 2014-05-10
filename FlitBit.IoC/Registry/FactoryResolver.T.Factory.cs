#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace FlitBit.IoC.Registry
{
	internal class FactoryResolver<T> : IResolver<T>
	{
		public FactoryResolver(Func<IContainer, Param[], object> factory)
		{
			this.Factory = factory;
		}

		protected Func<IContainer, Param[], object> Factory { get; private set; }

		#region IResolver<T> Members

		public Type TargetType
		{
			get { return typeof(T); }
		}

		public virtual bool TryResolve(IContainer container, LifespanTracking tracking, string name, out T instance,
			params Param[] parameters)
		{
			instance = (T)Factory(container, parameters);
			var disposable = instance as IDisposable;
			if (disposable != null && tracking == LifespanTracking.Automatic && !ReferenceEquals(container, instance))
			{
				container.Scope.Add(instance as IDisposable);
			}
			container.NotifyObserversOfCreationEvent(typeof(T), instance, name, CreationEventKind.Created);
			return true;
		}

		public bool TryUntypedResolve(IContainer container, LifespanTracking tracking, string name, out object instance,
			params Param[] parameters)
		{
			T temp;
			var result = TryResolve(container, tracking, name, out temp, parameters);
			instance = temp;
			return result;
		}

		#endregion
	}

	internal class FactoryInstancePerScopeResolver<T> : FactoryResolver<T>
	{
		readonly ConcurrentDictionary<Guid, T> _containerInstances = new ConcurrentDictionary<Guid, T>();

		public FactoryInstancePerScopeResolver(Func<IContainer, Param[], object> factory)
			: base(factory) { }

		public override bool TryResolve(IContainer container, LifespanTracking tracking, string name, out T instance,
			params Param[] parameters)
		{
			var kind = CreationEventKind.Reissued;
			var key = container.Key;
			var tempIssued = false;
			var temp = default(T);
			while (true)
			{
				if (_containerInstances.TryGetValue(key, out instance))
				{
					if (tempIssued && temp is IDisposable)
					{
						((IDisposable)temp).Dispose();
					}
					break;
				}
				if (!tempIssued)
				{
					temp = (T)Factory(container, parameters);
					tempIssued = true;
				}
				if (_containerInstances.TryAdd(key, temp))
				{
					container.Scope.AddAction(() =>
					{
						T value;
						if (_containerInstances.TryRemove(key, out value))
						{
							if (value is IDisposable)
							{
								((IDisposable)value).Dispose();
							}
						}
					});
					kind = CreationEventKind.Created;
					instance = temp;
					break;
				}
			}
			container.NotifyObserversOfCreationEvent(typeof(T), instance, name, kind);
			return true;
		}
	}

	internal class FactorySingletonResolver<T> : FactoryResolver<T>
	{
		readonly IContainer _owner;
		readonly Object _synch = new Object();
		T _singleton;

		public FactorySingletonResolver(IContainer owner, Func<IContainer, Param[], object> factory)
			: base(factory)
		{
			Contract.Requires<ArgumentNullException>(owner != null);

			_owner = owner;
		}

		public override bool TryResolve(IContainer container, LifespanTracking tracking, string name, out T instance,
			params Param[] parameters)
		{
			var kind = CreationEventKind.Reissued;

			lock (_synch)
			{
				if (!EqualityComparer<T>.Default.Equals(_singleton, default(T)))
				{
					instance = _singleton;
				}
				else
				{
					var value = _singleton = (T)Factory(container, Param.EmptyParams);
					_owner.Scope.AddAction(() =>
					{
						lock (_synch)
						{
							if (ReferenceEquals(value, _singleton))
							{
								if (value is IDisposable)
								{
									((IDisposable)_singleton).Dispose();
								}
								_singleton = default(T);
							}
						}
					});
					instance = value;
					kind = CreationEventKind.Created;
				}
			}
			// don't notify within the lock!
			container.NotifyObserversOfCreationEvent(typeof(T), instance, name, kind);
			return true;
		}
	}
}