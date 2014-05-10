#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Threading;

namespace FlitBit.IoC.Registry
{
	internal class FactoryTypeRegistration<T, TConcrete> : TypeRegistration<T>
		where TConcrete : T
	{
		readonly Func<IContainer, Param[], TConcrete> _factory;
		readonly Lazy<IResolver<T>> _resolver;

		internal FactoryTypeRegistration(IContainer container, Func<IContainer, Param[], TConcrete> factory)
			: base(container)
		{
			Contract.Requires<ArgumentNullException>(factory != null);

			_factory = factory;
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

		protected override IResolver<T> ConstructPerRequestResolver() { return new FactoryResolver<T, TConcrete>(_factory); }
		protected override IResolver<T> ConstructPerScopeResolver() { return new FactoryInstancePerScopeResolver<T, TConcrete>(_factory); }
		protected override IResolver<T> ConstructSingletonResolver() { return new FactorySingletonResolver<T, TConcrete>(this.Container, _factory); }
	}
}