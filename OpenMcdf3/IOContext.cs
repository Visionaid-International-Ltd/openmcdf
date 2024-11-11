﻿namespace OpenMcdf3;

enum IOContextFlags
{
    None = 0,
    Create = 1,
    LeaveOpen = 2,
    Transacted = 4,
}

/// <summary>
/// Encapsulates the objects required to read and write data to and from a compound file.
/// </summary>
internal sealed class IOContext : IDisposable
{
    readonly IOContextFlags contextFlags;
    readonly CfbBinaryWriter? writer;
    readonly TransactedStream? transactedStream;
    MiniFat? miniFat;
    FatStream? miniStream;

    public Header Header { get; }

    public Stream BaseStream { get; }

    public CfbBinaryReader Reader { get; }

    public CfbBinaryWriter Writer
    {
        get
        {
            if (writer is null)
                throw new InvalidOperationException("Stream is not writable");
            return writer;
        }
    }

    public Fat Fat { get; }

    public Directories Directories { get; }

    public DirectoryEntry RootEntry { get; }

    public MiniFat MiniFat
    {
        get
        {
            miniFat ??= new(this);
            return miniFat;
        }
    }

    public FatStream MiniStream
    {
        get
        {
            miniStream ??= new(this, RootEntry);
            return miniStream;
        }
    }

    public bool CanWrite => writer is not null;

    public bool IsDisposed { get; private set; }

    /// <summary>
    /// The size of a regular sector.
    /// </summary>
    public int SectorSize { get; }

    public int MiniSectorSize { get; }

    public Version Version => (Version)Header.MajorVersion;

    public long Length { get; private set; }

    public uint SectorCount => (uint)Math.Max(0, (Length - SectorSize) / SectorSize); // TODO: Check

    bool isDirty;

    public IOContext(Stream stream, Version version, IOContextFlags contextFlags = IOContextFlags.None)
    {
        BaseStream = stream;
        this.contextFlags = contextFlags;

        using CfbBinaryReader reader = new(stream);
        Header = contextFlags.HasFlag(IOContextFlags.Create) ? new(version) : reader.ReadHeader();
        SectorSize = 1 << Header.SectorShift;
        MiniSectorSize = 1 << Header.MiniSectorShift;
        Length = stream.Length;

        Stream actualStream = stream;
        if (contextFlags.HasFlag(IOContextFlags.Transacted))
        {
            Stream overlayStream = stream is MemoryStream ? new MemoryStream() : File.Create(Path.GetTempFileName());
            transactedStream = new TransactedStream(this, stream, overlayStream);
            actualStream = new BufferedStream(transactedStream, SectorSize);
        }

        Reader = new(actualStream);
        if (stream.CanWrite)
            writer = new(actualStream);

        Fat = new(this);
        Directories = new(this);

        if (contextFlags.HasFlag(IOContextFlags.Create))
        {
            RootEntry = Directories.CreateOrRecycleDirectoryEntry();
            RootEntry.RecycleRoot();

            WriteHeader();
            Directories.Write(RootEntry);
        }
        else
        {
            RootEntry = Directories.GetDictionaryEntry(0);
        }
    }

    public void Dispose()
    {
        if (!IsDisposed)
        {
            Flush();

            miniStream?.Dispose();
            miniFat?.Dispose();
            Directories.Dispose();
            Fat.Dispose();
            writer?.Dispose();
            Reader.Dispose();
            string? overlayFileName = (transactedStream?.OverlayStream as FileStream)?.Name;
            transactedStream?.Dispose();
            if (overlayFileName is not null)
                File.Delete(overlayFileName);
            if (!contextFlags.HasFlag(IOContextFlags.LeaveOpen))
                BaseStream.Dispose();
            IsDisposed = true;
        }
    }

    public void Flush()
    {
        if (isDirty && writer is not null && transactedStream is null)
        {
            // Ensure the stream is as long as expected
            BaseStream.SetLength(Length);
            WriteHeader();
            isDirty = false;
        }
    }

    public void ExtendStreamLength(long length)
    {
        if (Length < length)
            Length = length;
        isDirty = true;
    }

    public void WriteHeader()
    {
        CfbBinaryWriter writer = Writer;
        writer.Seek(0, SeekOrigin.Begin);
        writer.Write(Header);
    }

    public void Commit()
    {
        if (writer is null || transactedStream is null)
            throw new InvalidOperationException("Cannot commit non-transacted storage.");

        miniStream?.Flush();
        miniFat?.Flush();
        Fat.Flush();
        WriteHeader();
        writer.BaseStream.Flush();
        transactedStream.Commit();
    }

    public void Revert()
    {
        if (writer is null || transactedStream is null)
            throw new InvalidOperationException("Cannot revert non-transacted storage.");

        transactedStream.Revert();
    }
}
