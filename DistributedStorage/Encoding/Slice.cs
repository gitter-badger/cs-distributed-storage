﻿namespace DistributedStorage.Encoding
{
    public sealed class Slice
    {
        public bool[] Coefficients { get; set; }

        public byte[] EncodingSymbol { get; set; }
    }
}
