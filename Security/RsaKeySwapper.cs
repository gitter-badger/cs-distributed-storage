﻿namespace Security
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography;
    using Common;
    
    /// <summary>
    /// Something that can be used for a protocol which communicates public keys between two parties along with proof that both parties own their corresponding private key
    /// </summary>
    internal class RsaKeySwapper
    {
        /// <summary>
        /// Performs RSA crypto stuff for us
        /// </summary>
        private readonly ICryptoRsa _cryptoRsa;
        
        /// <summary>
        /// Creates a new <see cref="RsaKeySwapper"/> that uses the given RSA crypto
        /// </summary>
        internal RsaKeySwapper(ICryptoRsa cryptoRsa)
        {
            _cryptoRsa = cryptoRsa;
        }

        /// <summary>
        /// Sends our public key and our challenge
        /// </summary>
        internal void SendChallenge(Stream stream, RSAParameters ours, byte[] ourChallenge)
        {
            stream.WritePublicKey(ours);
            stream.WriteChunk(ourChallenge);
        }

        /// <summary>
        /// Receives their public key and their challenge
        /// </summary>
        internal bool TryReceiveChallenge(Stream stream, TimeSpan timeout, out RSAParameters theirs, out byte[] theirChallenge)
        {
            theirChallenge = null;
            var start = Stopwatch.StartNew();
            return
                stream.TryReadRsaKey(timeout, out theirs)
                &&
                stream.TryBlockingReadChunk(timeout - start.Elapsed, out theirChallenge);
        }

        /// <summary>
        /// Sends our proof of owning our private key by signing the combination of both challenges
        /// </summary>
        internal void SendChallengeResponse(Stream stream, RSAParameters ours, byte[] theirChallenge, byte[] ourChallenge)
        {
            var mixed = (byte[])theirChallenge.Clone();
            mixed.Xor(ourChallenge);
            var proof = _cryptoRsa.Sign(mixed, ours);
            
            stream.WriteChunk(proof);
        }

        /// <summary>
        /// Receives their signature of the combination of both challenges, returning true if it is valid
        /// </summary>
        internal bool TryReceiveChallengeResponse(Stream stream, byte[] ourChallenge, byte[] theirChallenge, RSAParameters theirs, TimeSpan timeout)
        {
            if (!stream.TryBlockingReadChunk(timeout, out var theirProof))
                return false;

            var mixed = (byte[])ourChallenge.Clone();
            mixed.Xor(theirChallenge);

            return _cryptoRsa.Verify(mixed, theirProof, theirs);
        }
    }
}
