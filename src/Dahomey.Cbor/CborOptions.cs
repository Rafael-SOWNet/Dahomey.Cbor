using Dahomey.Cbor.Attributes;
using Dahomey.Cbor.Serialization;
using Dahomey.Cbor.Serialization.Conventions;
using System;

namespace Dahomey.Cbor
{
    public enum UnhandledNameMode
    {
        Silent = 0,
        ThrowException = 1,
    }

    public enum ValueFormat
    {
        WriteToInt = 0,
        WriteToString = 1,
    }

    public enum DateTimeFormat
    {
        ISO8601 = 0,
        Unix = 1,
        UnixMilliseconds = 2,
    }

    /// <summary>
    /// https://tools.ietf.org/html/rfc7049#section-2.2
    /// </summary>
    public enum LengthMode
    {
        Default = 0,
        DefiniteLength = 1,
        IndefiniteLength = 2
    }

    /// <summary>
    /// Controls whether numeric arrays are read from, and written as, RFC 8746 typed arrays (tags 64-87).
    /// </summary>
    /// <remarks>
    /// Reading and writing are separate flags so that a peer's typed arrays can be accepted without
    /// emitting any. <see cref="Never"/> is the default and is a true no-op: with it, the library reads
    /// and writes exactly what it did before typed arrays existed.
    /// </remarks>
    [Flags]
    public enum TypedArrayMode
    {
        /// <summary>
        /// Typed arrays are neither read nor written. A tag in the range 64-87 is skipped like any other
        /// unrecognised semantic tag, so its content must be an ordinary CBOR array to be readable.
        /// </summary>
        Never = 0,

        /// <summary>
        /// Read RFC 8746 typed arrays, in either byte order, into the matching numeric array or collection.
        /// A tag whose element type does not match the target throws a <see cref="CborException"/>.
        /// </summary>
        Read = 1,

        /// <summary>
        /// Write numeric arrays as little-endian typed arrays: tags 72, 69, 77, 70, 78, 71, 79, 84, 85
        /// and 86. The payload is a byte-for-byte image of the array on little-endian hardware.
        /// </summary>
        WriteLittleEndian = 2,

        /// <summary>
        /// Read typed arrays and write little-endian ones.
        /// </summary>
        ReadWriteLittleEndian = Read | WriteLittleEndian,
    }

    public class CborOptions
    {
        public static CborOptions Default { get; } = new CborOptions()
        {
            UnqualifiedTimeZoneDateTimeKind = DateTimeKind.Local
        };

        public SerializationRegistry Registry { get; private set; }
        public UnhandledNameMode UnhandledNameMode { get; set; }
        public ValueFormat EnumFormat { get; set; }
        public DateTimeFormat DateTimeFormat { get; set; }
        /// <summary>
        /// When an ISO date with an unqualified timezone is parsed, this option gives the DateTimeKind to use
        /// </summary>
        public DateTimeKind UnqualifiedTimeZoneDateTimeKind { get; set; }
        public CborDiscriminatorPolicy DiscriminatorPolicy { get; set; }
        public CborObjectFormat ObjectFormat { get; set; } = CborObjectFormat.StringKeyMap;
        public LengthMode ArrayLengthMode { get; set; } = LengthMode.DefiniteLength;
        public LengthMode MapLengthMode { get; set; } = LengthMode.DefiniteLength;

        /// <summary>
        /// Whether RFC 8746 typed arrays are read and written. Default <see cref="TypedArrayMode.Never"/>,
        /// which leaves both paths exactly as they were before typed arrays existed.
        /// </summary>
        /// <remarks>
        /// A typed array is one definite-length byte string, so <see cref="ArrayLengthMode"/> has nothing
        /// to apply to and is ignored for any array actually written as one.
        /// </remarks>
        public TypedArrayMode TypedArrayMode { get; set; } = TypedArrayMode.Never;

        /// <summary>
        /// Semantic Tag to check if the discriminator is present when ObjectFormat is Array
        /// </summary>
        /// Default value is 39 (see: https://github.com/lucas-clemente/cbor-specs/blob/master/id.md)
        public ulong DiscriminatorSemanticTag { get; set; } = 39;

        /// <summary>
        /// Maximum nesting depth of maps and arrays, for both reading and writing. Default 64.
        /// </summary>
        /// <remarks>
        /// On write this converts a reference cycle - which CBOR cannot represent, and which would
        /// otherwise recurse until the stack is exhausted - into a <see cref="CborException"/>. On read
        /// it bounds stack use on untrusted input, where a handful of bytes can describe arbitrarily
        /// deep nesting. Raise it if the data is genuinely deeper than 64 levels.
        /// </remarks>
        public int MaxDepth { get; set; } = Serialization.CborWriter.DefaultMaxDepth;

        /// <summary>
        /// The default naming convention to use when no naming convention is specified.
        /// </summary>
        public INamingConvention? DefaultNamingConvention { get; set; }

        public CborOptions()
        {
            Registry = new SerializationRegistry(this);
        }
    }
}