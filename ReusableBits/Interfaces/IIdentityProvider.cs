using System;

namespace ReusableBits.Interfaces {
	public enum IdentityType {
		Guid,
		SequentialGuid,
		SequentialEndingGuid
	}

	public interface IIdentityProvider {
		IdentityType	IdentityType { get; set; }

		Guid	NewIdentityAsGuid();
		long	NewIdentityAsLong();
		string	NewIdentityAsString();
	}
}
