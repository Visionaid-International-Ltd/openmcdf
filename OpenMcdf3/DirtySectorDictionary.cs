using System.Collections.Concurrent;

namespace OpenMcdf3;

internal class DirtySectorDictionary : ConcurrentDictionary<uint, long>
{
}
