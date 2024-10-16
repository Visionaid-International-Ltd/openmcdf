﻿using System.Text;

namespace OpenMcdf3;

public enum StorageType
{
    Unallocated = 0,
    Storage = 1,
    Stream = 2,
    Root = 5
}

enum Color
{
    Red = 0,
    Black = 1
}

internal static class StreamId
{
    public const uint Maximum = 0xFFFFFFFA;
    public const uint NoStream = 0xFFFFFFFF;
}

internal sealed class DirectoryEntry
{
    internal const int Length = 128;
    internal const int NameFieldLength = 64;
    internal const uint MaxV3StreamLength = 0x80000000;

    internal static readonly DateTime ZeroFileTime = DateTime.FromFileTimeUtc(0);

    string name = string.Empty;
    DateTime creationTime;
    DateTime modifiedTime;

    public string Name
    {
        get => name;
        set
        {
            if (value.Contains(@"\") || value.Contains(@"/") || value.Contains(@":") || value.Contains(@"!"))
                throw new ArgumentException("Name cannot contain any of the following characters: '\\', '/', ':','!'");

            if (Encoding.Unicode.GetByteCount(value) + 2 > NameFieldLength)
                throw new ArgumentException($"{value} exceeds maximum encoded length of {NameFieldLength} bytes");

            name = value;
        }
    }

    public StorageType Type { get; set; } = StorageType.Unallocated;

    public Color Color { get; set; }

    public uint LeftSiblingID { get; set; }

    public uint RightSiblingID { get; set; }

    public uint ChildID { get; set; }

    public Guid CLSID { get; set; }

    public uint StateBits { get; set; }

    public DateTime CreationTime
    {
        get => creationTime;
        set
        {
            if (Type is StorageType.Stream or StorageType.Root && value != ZeroFileTime)
                throw new ArgumentException("Creation time must be zero for streams and root");

            creationTime = value;
        }
    }

    public DateTime ModifiedTime
    {
        get => modifiedTime;
        set
        {
            if (Type is StorageType.Stream && value != ZeroFileTime)
                throw new ArgumentException("Modified time must be zero for streams");

            modifiedTime = value;
        }
    }

    // Also the first sector of the mini-stream for the root storage object
    public uint StartSectorLocation { get; set; }

    public long StreamLength { get; set; }

    public EntryInfo ToEntryInfo() => new() { Name = Name };

    public override string ToString() => Name;
}
