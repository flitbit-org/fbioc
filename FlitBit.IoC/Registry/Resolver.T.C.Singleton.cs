#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using FlitBit.IoC.Constructors;

namespace FlitBit.IoC.Registry
{
	internal class SingletonResolver<T, C>: Resolver<T, C> where C: class, T
	{
		IContainer _owner;
		Object _synch = new Object();
		C _singleton;

		public SingletonResolver(IContainer owner, ConstructorSet<T, C> constructors)
			: base(constructors)
		{
			if (owner == null) throw new ArgumentNullException("owner");
			_owner = owner;
		}
				
		public override bool TryResolve(IContainer container, LifespanTracking tracking, string name, out T instance, params Param[] parameters)
		{
			var kind = CreationEventKind.Reissued;

			lock (_synch)
			{
				if (_singleton != null)
				{
					instance = _singleton;
				}
				else
				{
					CommandBinding<T> command = null;
					if (Constructors.TryMatchAndBind(parameters, out command))
					{
						var value = _singleton = (C)command.Execute(container, name);
						_owner.Scope.AddAction(() =>
						{
							lock (_synch)
							{
								if (Object.ReferenceEquals(value, _singleton))
								{
									if (IsDisposable) ((IDisposable)_singleton).Dispose();
									_singleton = default(C);
								}
							}
						});
						instance = _singleton;
						kind = CreationEventKind.Created;
					}
					else
					{
						instance = default(T);
						return false;
					}
				}
			}

			// don't notify within the lock!
			container.NotifyObserversOfCreationEvent(typeof(T), instance, name, kind);
			return true;
		}
	}

}
