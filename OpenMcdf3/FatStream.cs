﻿namespace OpenMcdf3;

/// <summary>
/// Provides a <inheritdoc cref="Stream"/> for a stream object in a compound file./>
/// </summary>
internal class FatStream : Stream
{
    readonly IOContext ioContext;
    readonly FatSectorChainEnumerator chain;
    readonly long length;
    long position;
    bool disposed;

    internal FatStream(IOContext ioContext, DirectoryEntry directoryEntry)
    {
        this.ioContext = ioContext;
        DirectoryEntry = directoryEntry;
        length = directoryEntry.StreamLength;
        chain = new(ioContext, directoryEntry.StartSectorId);
    }

    /// <inheritdoc/>
    internal DirectoryEntry DirectoryEntry { get; private set; }

    /// <inheritdoc/>
    public override bool CanRead => true;

    /// <inheritdoc/>
    public override bool CanSeek => true;

    /// <inheritdoc/>
    public override bool CanWrite => false;

    /// <inheritdoc/>
    public override long Length => length;

    /// <inheritdoc/>
    public override long Position
    {
        get => position;
        set => Seek(value, SeekOrigin.Begin);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!disposed)
        {
            chain.Dispose();
            disposed = true;
        }

        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    public override void Flush() => this.ThrowIfDisposed(disposed);

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        if (buffer is null)
            throw new ArgumentNullException(nameof(buffer));

        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be a non-negative number");

        if ((uint)count > buffer.Length - offset)
            throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection");

        this.ThrowIfDisposed(disposed);

        if (count == 0)
            return 0;

        uint chainIndex = (uint)Math.DivRem(position, ioContext.Header.SectorSize, out long sectorOffset);
        if (!chain.MoveTo(chainIndex))
            return 0;

        int maxCount = (int)Math.Min(Math.Max(length - position, 0), int.MaxValue);
        if (maxCount == 0)
            return 0;

        int realCount = Math.Min(count, maxCount);
        int readCount = 0;
        do
        {
            Sector sector = chain.Current;
            int remaining = realCount - readCount;
            long readLength = Math.Min(remaining, sector.Length - sectorOffset);
            ioContext.Reader.Seek(sector.Position + sectorOffset);
            int localOffset = offset + readCount;
            int read = ioContext.Reader.Read(buffer, localOffset, (int)readLength);
            if (read == 0)
                return readCount;
            position += read;
            readCount += read;
            sectorOffset = 0;
            if (readCount >= realCount)
                return readCount;
        } while (chain.MoveNext());

        return readCount;
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        this.ThrowIfDisposed(disposed);

        switch (origin)
        {
            case SeekOrigin.Begin:
                if (offset < 0)
                    throw new IOException("Seek before origin");
                position = offset;
                break;

            case SeekOrigin.Current:
                if (position + offset < 0)
                    throw new IOException("Seek before origin");
                position += offset;
                break;

            case SeekOrigin.End:
                if (Length - offset < 0)
                    throw new IOException("Seek before origin");
                position = Length - offset;
                break;

            default:
                throw new ArgumentException(nameof(origin), "Invalid seek origin");
        }

        return position;
    }

    /// <inheritdoc/>
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
