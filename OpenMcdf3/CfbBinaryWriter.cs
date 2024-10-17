﻿using System.Text;

namespace OpenMcdf3;

/// <summary>
/// Writes CFB data types to a stream.
/// </summary>
internal sealed class CfbBinaryWriter : BinaryWriter
{
    readonly byte[] buffer = new byte[DirectoryEntry.NameFieldLength];

    public CfbBinaryWriter(Stream input)
        : base(input, Encoding.Unicode, true)
    {
    }

    public void Write(Guid value)
    {
        // TODO: Avoid heap allocation
        byte[] bytes = value.ToByteArray();
        Write(bytes, 0, bytes.Length);
    }

    public void Write(DateTime value)
    {
        long fileTime = value.ToFileTimeUtc();
        Write(fileTime);
    }

    private void WriteBytes(byte[] buffer) => Write(buffer, 0, buffer.Length);

    public void Write(Header header)
    {
        Write(Header.Signature);
        Write(header.CLSID);
        Write(header.MinorVersion);
        Write(header.MajorVersion);
        Write(Header.LittleEndian);
        Write(header.SectorShift);
        Write(Header.MiniSectorShift);
        WriteBytes(Header.Unused);
        Write(header.DirectorySectorCount);
        Write(header.FatSectorCount);
        Write(header.FirstDirectorySectorId);
        Write((uint)0);
        Write(Header.MiniStreamCutoffSize);
        Write(header.FirstMiniFatSectorId);
        Write(header.MiniFatSectorCount);
        Write(header.FirstDifatSectorId);
        Write(header.DifatSectorCount);
    }

    public void Write(DirectoryEntry entry)
    {
        int nameLength = Encoding.Unicode.GetBytes(entry.Name, 0, entry.Name.Length, buffer, 0);
        Write(nameLength);
        Write(buffer, 0, DirectoryEntry.NameFieldLength);
        Write((byte)entry.Type);
        Write((byte)entry.Color);
        Write(entry.LeftSiblingId);
        Write(entry.RightSiblingId);
        Write(entry.ChildId);
        Write(entry.CLSID);
        Write(entry.StateBits);
        Write(entry.CreationTime);
        Write(entry.ModifiedTime);
        Write(entry.StartSectorId);
        Write(entry.StreamLength);
    }
}
