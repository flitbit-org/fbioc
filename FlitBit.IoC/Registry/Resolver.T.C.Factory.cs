#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.IoC.Registry
{
	internal class FactoryResolver<T, TConcrete> : IResolver<T>
		where TConcrete : T
	{
		public FactoryResolver(Func<IContainer, Param[], TConcrete> factory)
		{
			this.Factory = factory;
			IsDisposable = typeof(IDisposable).IsAssignableFrom(typeof(TConcrete));
		}

		protected Func<IContainer, Param[], TConcrete> Factory { get; private set; }
		protected bool IsDisposable { get; private set; }

		#region IResolver<T> Members

		public Type TargetType
		{
			get { return typeof(TConcrete); }
		}

		public virtual bool TryResolve(IContainer container, LifespanTracking tracking, string name, out T instance,
			params Param[] parameters)
		{
			instance = Factory(container, parameters);
			if (IsDisposable && tracking == LifespanTracking.Automatic && !ReferenceEquals(container, instance))
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
}