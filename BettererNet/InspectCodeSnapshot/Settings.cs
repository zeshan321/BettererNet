using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace InspectCodeSnapshot
{
    public class Settings
    {
        public bool Debug { get; set; }
        public IReadOnlyList<string> Violations { get; set; }
        public ProcessStartInfo SnapshotProcessStartInfo { get; set; }
        public ProcessStartInfo CompareProcessStartInfo { get; set; }
    }
}