﻿namespace DistributedStorage.Networking
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Encoding;
    using Model;
    using Protocol;
    using Protocol.Methods;
    using Serialization;

    /// <summary>
    /// Implements the remote side of a networked <see cref="IBucket{TIdentity}"/>
    /// </summary>
    public sealed class RemoteBucket<TIdentity> : IBucket<TIdentity>
        where TIdentity : IIdentity
    {
        /// <summary>
        /// A factory which can create instances of <see cref="RemoteBucket{TIdentity}"/> using injected <see cref="IConverter{TFrom, TTo}"/>s
        /// </summary>
        public sealed class Factory
        {
            #region Private fields
            
            private readonly IConverter<Nothing, byte[]> _nothingToBytesConverter;
            private readonly IConverter<byte[], long> _bytesToLongConverter;
            private readonly IConverter<Manifest, byte[]> _manifestToBytesConverter;
            private readonly IConverter<byte[], Hash[]> _bytesToHashArrayConverter;
            private readonly IConverter<byte[], Manifest[]> _bytesToManifestArrayConverter;
            private readonly IConverter<Tuple<Manifest, Hash[]>, byte[]> _manifestAndHashArrayTupleToBytesConverter;
            private readonly IConverter<byte[], Slice[]> _bytesToSliceArrayConverter;
            private readonly IConverter<byte[], TIdentity> _bytesToTIdentityConverter;

            #endregion

            #region Constructor

            /// <summary>
            /// Creates a new <see cref="Factory"/> that will use all the given <see cref="IConverter{TFrom, TTo}"/>s to create instances of <see cref="RemoteBucket{TIdentity}"/>
            /// </summary>
            public Factory(
                IConverter<Nothing, byte[]> nothingToBytesConverter,
                IConverter<byte[], long> bytesToLongConverter,
                IConverter<Manifest, byte[]> manifestToBytesConverter,
                IConverter<byte[], Hash[]> bytesToHashArrayConverter,
                IConverter<byte[], Manifest[]> bytesToManifestArrayConverter,
                IConverter<Tuple<Manifest, Hash[]>, byte[]> manifestAndHashArrayTupleToBytesConverter,
                IConverter<byte[], Slice[]> bytesToSliceArrayConverter,
                IConverter<byte[], TIdentity> bytesToTIdentityConverter
                )
            {
                _nothingToBytesConverter = nothingToBytesConverter;
                _bytesToLongConverter = bytesToLongConverter;
                _manifestToBytesConverter = manifestToBytesConverter;
                _bytesToHashArrayConverter = bytesToHashArrayConverter;
                _bytesToManifestArrayConverter = bytesToManifestArrayConverter;
                _manifestAndHashArrayTupleToBytesConverter = manifestAndHashArrayTupleToBytesConverter;
                _bytesToSliceArrayConverter = bytesToSliceArrayConverter;
                _bytesToTIdentityConverter = bytesToTIdentityConverter;
            }

            #endregion

            #region Public methods

            /// <summary>
            /// Creates a new <see cref="RemoteBucket{TIdentity}"/> from the given <paramref name="protocol"/>
            /// </summary>
            public RemoteBucket<TIdentity> CreateFrom(IProtocol protocol) => new RemoteBucket<TIdentity>(
                protocol,
                _nothingToBytesConverter,
                _bytesToLongConverter,
                _manifestToBytesConverter,
                _bytesToHashArrayConverter,
                _bytesToManifestArrayConverter,
                _manifestAndHashArrayTupleToBytesConverter,
                _bytesToSliceArrayConverter,
                _bytesToTIdentityConverter
            );

            #endregion
        }

        #region Public properties

        public long MaxSize => _getMaxSizePropertyMethod.InvokeAndWait(Nothing.Instance);
        public TIdentity OwnerIdentity => _getOwnerIdentityPropertyMethod.InvokeAndWait(Nothing.Instance);
        public TIdentity PoolIdentity => _getPoolIdentityPropertyMethod.InvokeAndWait(Nothing.Instance);
        public TIdentity SelfIdentity => _getSelfIdentityPropertyMethod.InvokeAndWait(Nothing.Instance);

        #endregion

        #region Private fields

        private readonly IMethod<Nothing, long> _getCurrentSizeMethod;
        private readonly IMethod<Manifest, Hash[]> _getHashesMethod;
        private readonly IMethod<Nothing, Manifest[]> _getManifestsMethod;
        private readonly IMethod<Tuple<Manifest, Hash[]>, Slice[]> _getSlicesMethod;
        private readonly IMethod<Nothing, long> _getMaxSizePropertyMethod;
        private readonly IMethod<Nothing, TIdentity> _getOwnerIdentityPropertyMethod;
        private readonly IMethod<Nothing, TIdentity> _getPoolIdentityPropertyMethod;
        private readonly IMethod<Nothing, TIdentity> _getSelfIdentityPropertyMethod;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new <see cref="RemoteBucket{TIdentity}"/>, which implements the remote side of a networked <see cref="IBucket{TIdentity}"/>
        /// </summary>
        public RemoteBucket(
            IProtocol protocol,
            IConverter<Nothing, byte[]> nothingToBytesConverter,
            IConverter<byte[], long> bytesToLongConverter,
            IConverter<Manifest, byte[]> manifestToBytesConverter,
            IConverter<byte[], Hash[]> bytesToHashArrayConverter,
            IConverter<byte[], Manifest[]> bytesToManifestArrayConverter,
            IConverter<Tuple<Manifest, Hash[]>, byte[]> manifestAndHashArrayTupleToBytesConverter,
            IConverter<byte[], Slice[]> bytesToSliceArrayConverter,
            IConverter<byte[], TIdentity> bytesToTIdentityConverter
            )
        {
            _getCurrentSizeMethod = ProtocolMethod.Create(protocol, nameof(IBucket<TIdentity>.GetCurrentSize), nothingToBytesConverter, bytesToLongConverter, () => { });
            _getHashesMethod = ProtocolMethod.Create(protocol, nameof(IBucket<TIdentity>.GetHashes), manifestToBytesConverter, bytesToHashArrayConverter, () => { });
            _getManifestsMethod = ProtocolMethod.Create(protocol, nameof(IBucket<TIdentity>.GetManifests), nothingToBytesConverter, bytesToManifestArrayConverter, () => { });
            _getMaxSizePropertyMethod = ProtocolMethod.Create(protocol, nameof(IBucket<TIdentity>.MaxSize), nothingToBytesConverter, bytesToLongConverter, () => { });
            _getOwnerIdentityPropertyMethod = ProtocolMethod.Create(protocol, nameof(IBucket<TIdentity>.OwnerIdentity), nothingToBytesConverter, bytesToTIdentityConverter, () => { });
            _getPoolIdentityPropertyMethod = ProtocolMethod.Create(protocol, nameof(IBucket<TIdentity>.PoolIdentity), nothingToBytesConverter, bytesToTIdentityConverter, () => { });
            _getSlicesMethod = ProtocolMethod.Create(protocol, nameof(IBucket<TIdentity>.GetSlices), manifestAndHashArrayTupleToBytesConverter, bytesToSliceArrayConverter, () => { });
            _getSelfIdentityPropertyMethod = ProtocolMethod.Create(protocol, nameof(IBucket<TIdentity>.SelfIdentity), nothingToBytesConverter, bytesToTIdentityConverter, () => { });
        }

        #endregion

        #region Public methods

        public long GetCurrentSize() => _getCurrentSizeMethod.InvokeAndWait(Nothing.Instance);
        public IEnumerable<Hash> GetHashes(Manifest forManifest) => _getHashesMethod.InvokeAndWait(forManifest);
        public IEnumerable<Manifest> GetManifests() => _getManifestsMethod.InvokeAndWait(Nothing.Instance);
        public IEnumerable<Slice> GetSlices(Manifest forManifest, Hash[] hashes) => _getSlicesMethod.InvokeAndWait(Tuple.Create(forManifest, hashes));

        #endregion
    }
}
