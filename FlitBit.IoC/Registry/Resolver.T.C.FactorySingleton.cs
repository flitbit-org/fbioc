#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace FlitBit.IoC.Registry
{
	internal class FactorySingletonResolver<T, TConcrete> : FactoryResolver<T, TConcrete>
		where TConcrete : T
	{
		readonly IContainer _owner;
		readonly Object _synch = new Object();
		TConcrete _singleton;

		public FactorySingletonResolver(IContainer owner, Func<IContainer, Param[], TConcrete> factory)
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
				if (!EqualityComparer<TConcrete>.Default.Equals(_singleton, default(TConcrete)))
				{
					instance = _singleton;
				}
				else
				{
					var value = _singleton = Factory(container, Param.EmptyParams);
					_owner.Scope.AddAction(() =>
					{
						lock (_synch)
						{
							if (ReferenceEquals(value, _singleton))
							{
								if (IsDisposable)
								{
									((IDisposable) _singleton).Dispose();
								}
								_singleton = default(TConcrete);
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