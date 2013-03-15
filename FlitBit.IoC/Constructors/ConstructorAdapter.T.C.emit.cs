#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using FlitBit.Emit;

namespace FlitBit.IoC.Constructors
{
	/// <summary>
	///   Adapter for constructors defined on concrete type of type T
	/// </summary>
	public partial class ConstructorAdapter<T, TConcrete>
		where TConcrete : T
	{
		/// <summary>
		///   Compiles a constructor adapter for the given constructor.
		/// </summary>
		/// <param name="ordinal">the ordinal position of the constructor among constructors defined on type T</param>
		/// <param name="ci">constructor info</param>
		/// <returns>the compiled constructor adapter type</returns>
		public new static Type GetConstructorAdapterByOrdinal(int ordinal, ConstructorInfo ci)
		{
			Contract.Ensures(Contract.Result<Type>() != null);

			var targetType = typeof(T);
			var concreteType = typeof(TConcrete);
			var typeName = RuntimeAssemblies.PrepareTypeName(targetType, String.Concat(concreteType.Name, "Ctor#", ordinal));

			var module = ConstructorAdapter.Module;
			lock (module)
			{
				var type = module.Builder.GetType(typeName, false, false) ?? BuildConstructorAdapter(module, typeName, ci);
				return type;
			}
		}
	}
}