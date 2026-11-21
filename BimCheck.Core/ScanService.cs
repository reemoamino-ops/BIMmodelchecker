using System;
using BimCheck.Core;

namespace BimCheck.Core;

public static class ScanService
{
    /// <summary>Reads doors from the source and checks their Marks. Read-only by design.</summary>
    public static CheckResult Scan(IDoorSource source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        return MarkChecker.Check(source.ReadDoors());
    }
}
