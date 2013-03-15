#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using FlitBit.IoC.Constructors;

namespace FlitBit.IoC.Registry
{
	internal class SingletonResolver<T, TConcrete> : Resolver<T, TConcrete>
		where TConcrete : class, T
	{
		readonly IContainer _owner;
		readonly Object _synch = new Object();
		TConcrete _singleton;

		public SingletonResolver(IContainer owner, ConstructorSet<T, TConcrete> constructors)
			: base(constructors)
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
				if (_singleton != null)
				{
					instance = _singleton;
				}
				else
				{
					CommandBinding<T> command;
					if (Constructors.TryMatchAndBind(parameters, out command))
					{
						var value = _singleton = (TConcrete) command.Execute(container, name);
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