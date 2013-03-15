#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using FlitBit.IoC.Constructors;

namespace FlitBit.IoC.Registry
{
	internal class Resolver<T, TConcrete> : IResolver<T>
		where TConcrete : class, T
	{
		public Resolver(ConstructorSet<T, TConcrete> constructors)
		{
			this.Constructors = constructors;
			IsDisposable = typeof(IDisposable).IsAssignableFrom(typeof(TConcrete));
		}

		protected ConstructorSet<T, TConcrete> Constructors { get; private set; }
		protected bool IsDisposable { get; private set; }

		#region IResolver<T> Members

		public Type TargetType
		{
			get { return typeof(TConcrete); }
		}

		public virtual bool TryResolve(IContainer container, LifespanTracking tracking, string name, out T instance,
			params Param[] parameters)
		{
			CommandBinding<T> command;
			if (Constructors.TryMatchAndBind(parameters, out command))
			{
				instance = command.Execute(container, name);
				if (IsDisposable && tracking == LifespanTracking.Automatic && !ReferenceEquals(container, instance))
				{
					container.Scope.Add(instance as IDisposable);
				}
				container.NotifyObserversOfCreationEvent(typeof(T), instance, name, CreationEventKind.Created);
				return true;
			}
			instance = default(T);
			return false;
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
}