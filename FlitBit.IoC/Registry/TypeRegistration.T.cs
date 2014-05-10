#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.IoC.Registry
{
	internal abstract class TypeRegistration<T> : TypeRegistration, ITypeRegistration<T>
	{
		protected TypeRegistration(IContainer container)
			: base(container, typeof(T)) { }

		protected abstract IResolver<T> ConstructPerRequestResolver();
		protected abstract IResolver<T> ConstructPerScopeResolver();
		protected abstract IResolver<T> ConstructSingletonResolver();

		protected virtual IResolver<T> ConfigureResolver()
		{
			var behavior = this.ScopeBehavior;

			if (behavior.HasFlag(ScopeBehavior.Singleton))
			{
				return ConstructSingletonResolver();
			}
			if (behavior.HasFlag(ScopeBehavior.InstancePerScope))
			{
				return ConstructPerScopeResolver();
			}
			return ConstructPerRequestResolver();
		}

		#region ITypeRegistration<T> Members

		public override Type TargetType
		{
			get { return typeof(T); }
		}

		public abstract IResolver<T> Resolver { get; }

		#endregion
	}
}