#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Threading;
using FlitBit.IoC.Constructors;

namespace FlitBit.IoC.Registry
{
	internal class TypeRegistration<T, TConcrete> : TypeRegistration<T>
		where TConcrete : class, T
	{
		readonly ConstructorSet<T, TConcrete> _constructors;
		readonly Lazy<IResolver<T>> _resolver;

		public TypeRegistration(IContainer container)
			: this(container, null) { }

		public TypeRegistration(IContainer container, Param[] parameters)
			: base(container)
		{
			_constructors = new ConstructorSet<T, TConcrete>(parameters);
			_resolver = new Lazy<IResolver<T>>(ConfigureResolver, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		public override IResolver<T> Resolver
		{
			get { return _resolver.Value; }
		}

		public override IResolver UntypedResolver
		{
			get { return _resolver.Value; }
		}

		protected override IResolver<T> ConstructPerRequestResolver() { return new Resolver<T, TConcrete>(_constructors); }

		protected override IResolver<T> ConstructPerScopeResolver() { return new InstancePerScopeResolver<T, TConcrete>(_constructors); }

		protected override IResolver<T> ConstructSingletonResolver() { return new SingletonResolver<T, TConcrete>(Container, _constructors); }
	}
}