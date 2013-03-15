#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Concurrent;

namespace FlitBit.IoC.Registry
{
	internal class FactoryInstancePerScopeResolver<T, TConcrete> : FactoryResolver<T, TConcrete>
		where TConcrete : T
	{
		readonly ConcurrentDictionary<Guid, T> _containerInstances = new ConcurrentDictionary<Guid, T>();

		public FactoryInstancePerScopeResolver(Func<IContainer, Param[], TConcrete> factory)
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
					if (tempIssued && IsDisposable)
					{
						((IDisposable) temp).Dispose();
					}
					break;
				}
				if (!tempIssued)
				{
					temp = Factory(container, parameters);
					tempIssued = true;
				}
				if (_containerInstances.TryAdd(key, temp))
				{

					container.Scope.AddAction(() =>
					{
						T value;
						if (_containerInstances.TryRemove(key, out value))
						{
							if (IsDisposable)
							{
								((IDisposable) value).Dispose();
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
}