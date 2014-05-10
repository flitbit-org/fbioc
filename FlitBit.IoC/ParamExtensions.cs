#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using FlitBit.Core;
using System.Diagnostics.Contracts;

namespace FlitBit.IoC
{
	/// <summary>
	///   extensions for the Param class
	/// </summary>
	public static class ParamExtensions
	{
		/// <summary>
		///   Gets a value from the first parameter assignable to type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="pp">array of parameters</param>
		/// <param name="c">a container</param>
		/// <returns>the parameter's value</returns>
		public static T AssignableTo<T>(this Param[] pp, IContainer c)
		{
			var t = typeof(T);
			var v = First(pp, p => t.IsAssignableFrom(p.TypeofValue));
			return (T) v.GetValue(c);
		}

		/// <summary>
		///   Gets a value from the parameter of type T and at the index given.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="pp">array of paramters</param>
		/// <param name="position">the parameter's index</param>
		/// <param name="c">a container</param>
		/// <returns>the parameter's value</returns>
		public static T At<T>(this Param[] pp, int position, IContainer c)
		{
			var v = pp[position];
			if (typeof(T).IsAssignableFrom(v.TypeofValue))
			{
				return (T) v.GetValue(c);
			}
			throw new InvalidOperationException(String.Concat("Value not assignable: ",
																												pp[position].TypeofValue.GetReadableFullName()));
		}

		/// <summary>
		///   Gets the first parameter from <paramref name="pp" /> that satisfies the predticate.
		/// </summary>
		/// <param name="pp">array of paramters</param>
		/// <param name="predicate">the predicate</param>
		/// <returns>the first param succeeding the predicate</returns>
		public static Param First(this Param[] pp, Func<Param, bool> predicate)
		{
			if (predicate == null)
			{
				throw new ArgumentNullException("predicate");
			}

			#region switch-unrolled - if this seems wierd, search unit tests for 'switch-unrolled'

			var plen = pp.Length;
			switch (plen)
			{
				case 0:
					break;
				case 1:
					if (predicate(pp[0]))
					{
						return pp[0];
					}
					break;
				case 2:
					if (predicate(pp[0]))
					{
						return pp[0];
					}
					if (predicate(pp[1]))
					{
						return pp[1];
					}
					break;
				case 3:
					if (predicate(pp[0]))
					{
						return pp[0];
					}
					if (predicate(pp[1]))
					{
						return pp[1];
					}
					if (predicate(pp[2]))
					{
						return pp[2];
					}
					break;
				case 4:
					if (predicate(pp[0]))
					{
						return pp[0];
					}
					if (predicate(pp[1]))
					{
						return pp[1];
					}
					if (predicate(pp[2]))
					{
						return pp[2];
					}
					if (predicate(pp[3]))
					{
						return pp[3];
					}
					break;
				case 5:
					if (predicate(pp[0]))
					{
						return pp[0];
					}
					if (predicate(pp[1]))
					{
						return pp[1];
					}
					if (predicate(pp[2]))
					{
						return pp[2];
					}
					if (predicate(pp[3]))
					{
						return pp[3];
					}
					if (predicate(pp[4]))
					{
						return pp[4];
					}
					break;
				case 6:
					if (predicate(pp[0]))
					{
						return pp[0];
					}
					if (predicate(pp[1]))
					{
						return pp[1];
					}
					if (predicate(pp[2]))
					{
						return pp[2];
					}
					if (predicate(pp[3]))
					{
						return pp[3];
					}
					if (predicate(pp[4]))
					{
						return pp[4];
					}
					if (predicate(pp[5]))
					{
						return pp[5];
					}
					break;
				case 7:
					if (predicate(pp[0]))
					{
						return pp[0];
					}
					if (predicate(pp[1]))
					{
						return pp[1];
					}
					if (predicate(pp[2]))
					{
						return pp[2];
					}
					if (predicate(pp[3]))
					{
						return pp[3];
					}
					if (predicate(pp[4]))
					{
						return pp[4];
					}
					if (predicate(pp[5]))
					{
						return pp[5];
					}
					if (predicate(pp[6]))
					{
						return pp[6];
					}
					break;
				case 8:
					if (predicate(pp[0]))
					{
						return pp[0];
					}
					if (predicate(pp[1]))
					{
						return pp[1];
					}
					if (predicate(pp[2]))
					{
						return pp[2];
					}
					if (predicate(pp[3]))
					{
						return pp[3];
					}
					if (predicate(pp[4]))
					{
						return pp[4];
					}
					if (predicate(pp[5]))
					{
						return pp[5];
					}
					if (predicate(pp[6]))
					{
						return pp[6];
					}
					if (predicate(pp[7]))
					{
						return pp[7];
					}
					break;
				case 9:
					if (predicate(pp[0]))
					{
						return pp[0];
					}
					if (predicate(pp[1]))
					{
						return pp[1];
					}
					if (predicate(pp[2]))
					{
						return pp[2];
					}
					if (predicate(pp[3]))
					{
						return pp[3];
					}
					if (predicate(pp[4]))
					{
						return pp[4];
					}
					if (predicate(pp[5]))
					{
						return pp[5];
					}
					if (predicate(pp[6]))
					{
						return pp[6];
					}
					if (predicate(pp[7]))
					{
						return pp[7];
					}
					if (predicate(pp[8]))
					{
						return pp[8];
					}
					break;
				default:
					if (predicate(pp[0]))
					{
						return pp[0];
					}
					if (predicate(pp[1]))
					{
						return pp[1];
					}
					if (predicate(pp[2]))
					{
						return pp[2];
					}
					if (predicate(pp[3]))
					{
						return pp[3];
					}
					if (predicate(pp[4]))
					{
						return pp[4];
					}
					if (predicate(pp[5]))
					{
						return pp[5];
					}
					if (predicate(pp[6]))
					{
						return pp[6];
					}
					if (predicate(pp[7]))
					{
						return pp[7];
					}
					if (predicate(pp[8]))
					{
						return pp[8];
					}
					if (predicate(pp[9]))
					{
						return pp[9];
					}
					for (var i = 10; i < plen; i++)
					{
						if (predicate(pp[i]))
						{
							return pp[i];
						}
					}
					break;
			}

			#endregion

			throw new InvalidOperationException("Parameter not found");
		}

		/// <summary>
		///   Gets a value from the first parameter assignable to T and with the name given.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="pp">array of parameters</param>
		/// <param name="name">the parameter's name</param>
		/// <param name="container">a container</param>
		/// <returns>the parameter's value</returns>
		public static T Named<T>(this Param[] pp, string name, IContainer container)
		{
			Contract.Requires<ArgumentNullException>(name != null);
			Contract.Requires<ArgumentException>(name.Length > 0);
			Contract.Requires<ArgumentNullException>(container != null);

			var t = typeof(T);
			var v = First(pp, p => t.IsAssignableFrom(p.TypeofValue) && name == p.Name);
			return (T) v.GetValue(container);
		}

		/// <summary>
		///   Gets a value from the first parameter of type T.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="pp">array of parameters</param>
		/// <param name="container">a container</param>
		/// <returns>the parameter's value</returns>
		public static T OfType<T>(this Param[] pp, IContainer container)
		{
			var t = typeof(T);
			var v = First(pp, p => t == p.TypeofValue);
			return (T) v.GetValue(container);
		}
	}
}