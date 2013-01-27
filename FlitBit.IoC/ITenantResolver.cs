#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

namespace FlitBit.IoC
{
	/// <summary>
	/// Interface for tenant resolvers
	/// </summary>
	public interface ITenantResolver
	{
		/// <summary>
		/// Tries to resolve the current tenant ID.
		/// </summary>
		/// <param name="tenantID">variable to hold the tenant id</param>
		/// <returns>true if a tenant ID is resolved; otherwise false</returns>
		bool TryResolveTenant(out object tenantID);
	}
}
