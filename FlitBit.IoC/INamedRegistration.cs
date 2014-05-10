#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.IoC
{
	/// <summary>
	///   Interface for named registrations.
	/// </summary>
	public interface INamedRegistration
	{
		/// <summary>
		///   Gets the registration's name.
		/// </summary>
		string Name { get; }
	}
}