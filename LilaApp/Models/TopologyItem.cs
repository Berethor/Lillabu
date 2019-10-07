using System.Diagnostics;

namespace LilaApp.Models
{
    [DebuggerDisplay("{FirstBlock} {SecondBlock} {Direction}")]
    public class TopologyItem
    {
        public int FirstBlock { get; set; }

        public int SecondBlock { get; set; }

        public int Direction { get; set; }
    }
}
