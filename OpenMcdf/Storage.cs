﻿namespace OpenMcdf;

/// <summary>
/// An object in a compound file that is analogous to a file system directory.
/// </summary>
public class Storage : ContextBase
{
    internal readonly DirectoryTree directoryTree;

    internal DirectoryEntry DirectoryEntry { get; }

    internal Storage(RootContextSite rootContextSite, DirectoryEntry directoryEntry)
        : base(rootContextSite)
    {
        if (directoryEntry.Type is not StorageType.Storage and not StorageType.Root)
            throw new ArgumentException("DirectoryEntry must be a Storage or Root.", nameof(directoryEntry));

        directoryTree = new(Context.DirectoryEntries, directoryEntry);
        DirectoryEntry = directoryEntry;
    }

    public EntryInfo EntryInfo => DirectoryEntry.ToEntryInfo();

    public IEnumerable<EntryInfo> EnumerateEntries()
    {
        this.ThrowIfDisposed(Context.IsDisposed);

        return EnumerateDirectoryEntries()
            .Select(e => e.ToEntryInfo());
    }

    IEnumerable<DirectoryEntry> EnumerateDirectoryEntries()
    {
        using DirectoryTreeEnumerator treeEnumerator = new(Context.DirectoryEntries, DirectoryEntry);
        while (treeEnumerator.MoveNext())
        {
            yield return treeEnumerator.Current;
        }
    }

    IEnumerable<DirectoryEntry> EnumerateDirectoryEntries(StorageType type) => EnumerateDirectoryEntries()
        .Where(e => e.Type == type);

    DirectoryEntry AddDirectoryEntry(StorageType storageType, string name)
    {
        DirectoryEntry entry = Context.DirectoryEntries.CreateOrRecycleDirectoryEntry();
        entry.Recycle(storageType, name);
        directoryTree.Add(entry);
        return entry;
    }

    public Storage CreateStorage(string name)
    {
        ThrowHelper.ThrowIfNameIsInvalid(name);

        this.ThrowIfDisposed(Context.IsDisposed);

        DirectoryEntry entry = AddDirectoryEntry(StorageType.Storage, name);
        return new Storage(ContextSite, entry);
    }

    public CfbStream CreateStream(string name)
    {
        ThrowHelper.ThrowIfNameIsInvalid(name);

        this.ThrowIfDisposed(Context.IsDisposed);

        // TODO: Return a Stream that can transition between FAT and mini FAT
        DirectoryEntry entry = AddDirectoryEntry(StorageType.Stream, name);
        return new CfbStream(ContextSite, entry);
    }

    public Storage OpenStorage(string name)
    {
        ThrowHelper.ThrowIfNameIsInvalid(name);

        this.ThrowIfDisposed(Context.IsDisposed);

        directoryTree.TryGetDirectoryEntry(name, out DirectoryEntry? entry);
        if (entry is null || entry.Type is not StorageType.Storage)
            throw new DirectoryNotFoundException($"Storage not found: {name}.");
        return new Storage(ContextSite, entry);
    }

    public CfbStream OpenStream(string name)
    {
        ThrowHelper.ThrowIfNameIsInvalid(name);

        this.ThrowIfDisposed(Context.IsDisposed);

        directoryTree.TryGetDirectoryEntry(name, out DirectoryEntry? entry);
        if (entry is null || entry.Type is not StorageType.Stream)
            throw new FileNotFoundException($"Stream not found: {name}.", name);

        // TODO: Return a Stream that can transition between FAT and mini FAT
        return new CfbStream(ContextSite, entry);
    }

    public void CopyTo(Storage destination)
    {
        foreach (DirectoryEntry entry in EnumerateDirectoryEntries())
        {
            if (entry.Type is StorageType.Storage)
            {
                Storage subSource = new(ContextSite, entry);
                Storage subDestination = destination.CreateStorage(entry.NameString);
                subSource.CopyTo(subDestination);
            }
            else if (entry.Type is StorageType.Stream)
            {
                CfbStream stream = new(ContextSite, entry);
                CfbStream destinationStream = destination.CreateStream(entry.NameString);
                stream.CopyTo(destinationStream);
            }
        }
    }

    public void Delete(string name)
    {
        ThrowHelper.ThrowIfNameIsInvalid(name);

        this.ThrowIfDisposed(Context.IsDisposed);

        directoryTree.TryGetDirectoryEntry(name, out DirectoryEntry? entry);
        if (entry is null)
            return;

        if (entry.Type is StorageType.Storage && entry.ChildId is not StreamId.NoStream)
        {
            Storage storage = new(ContextSite, entry);
            foreach (EntryInfo childEntry in storage.EnumerateEntries())
            {
                storage.Delete(childEntry.Name);
            };
        }

        if (entry.Type is StorageType.Stream && entry.StartSectorId is not StreamId.NoStream)
        {
            if (entry.StreamLength < Header.MiniStreamCutoffSize)
            {
                using MiniFatChainEnumerator miniFatChainEnumerator = new(ContextSite, entry.StartSectorId);
                miniFatChainEnumerator.Shrink(0);
            }
            else
            {
                using FatChainEnumerator fatChainEnumerator = new(Context.Fat, entry.StartSectorId);
                fatChainEnumerator.Shrink(0);
            }
        }

        directoryTree.Remove(entry);
    }

    internal void TraceDirectoryEntries(TextWriter writer)
    {
        directoryTree.WriteTrace(writer);
    }
}
