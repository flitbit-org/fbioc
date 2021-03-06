﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using FlitBit.IoC.Constructors;

namespace FlitBit.IoC.Registry
{
	internal class GenericTypeRegistration : TypeRegistration, IGenericTypeRegistration
	{
		readonly Type _target;

		public GenericTypeRegistration(IContainer container, Type generic, Type target)
			: base(container, generic) { _target = target; }

		#region IGenericTypeRegistration Members

		public override Type TargetType
		{
			get { return _target; }
		}

		public override IResolver UntypedResolver
		{
			get { throw new NotImplementedException(); }
		}

		public IResolver<T> ResolverFor<T>()
		{
			var t = typeof(T);
			var ultimateTarget = _target.MakeGenericType(t.GetGenericArguments());

			var behavior = this.ScopeBehavior;
			var ctor = Activator.CreateInstance(typeof(ConstructorSet<,>).MakeGenericType(t, ultimateTarget), new object[] {null});

			if (behavior.HasFlag(ScopeBehavior.Singleton))
			{
				return
					(IResolver<T>) Activator.CreateInstance(typeof(SingletonResolver<,>).MakeGenericType(t, ultimateTarget), ctor);
			}
			if (behavior.HasFlag(ScopeBehavior.InstancePerScope))
			{
				return
					(IResolver<T>)
						Activator.CreateInstance(typeof(InstancePerScopeResolver<,>).MakeGenericType(t, ultimateTarget), ctor);
			}
			else
			{
				return (IResolver<T>) Activator.CreateInstance(typeof(Resolver<,>).MakeGenericType(t, ultimateTarget), ctor);
			}
		}

		#endregion
	}
}