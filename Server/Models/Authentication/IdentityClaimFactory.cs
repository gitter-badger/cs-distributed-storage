﻿namespace Server.Models.Authentication
{
    using System.Security.Claims;

    /// <summary>
    /// Something which knows how to create <see cref="Claim"/>s of identity
    /// </summary>
    public sealed class IdentityClaimFactory
    {
        /// <summary>
        /// The claim type
        /// </summary>
        private const string ClaimType = ClaimTypes.Sid;

        /// <summary>
        /// Creates a <see cref="Claim"/> of the given <paramref name="identity"/>
        /// </summary>
        public Claim CreateClaim(string identity) => new Claim(ClaimType, identity);
    }
}
