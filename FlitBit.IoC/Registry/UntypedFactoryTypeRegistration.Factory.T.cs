#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Threading;

namespace FlitBit.IoC.Registry
{
	internal class UntypedFactoryTypeRegistration<T> : TypeRegistration<T>
	{
		readonly Func<IContainer, Param[], object> _factory;
		readonly Lazy<IResolver<T>> _resolver;

		internal UntypedFactoryTypeRegistration(IContainer container, Func<IContainer, Param[], object> factory)
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

		protected override IResolver<T> ConstructPerRequestResolver() { return new FactoryResolver<T>(_factory); }
		protected override IResolver<T> ConstructPerScopeResolver() { return new FactoryInstancePerScopeResolver<T>(_factory); }
		protected override IResolver<T> ConstructSingletonResolver() { return new FactorySingletonResolver<T>(this.Container, _factory); }
	}
}