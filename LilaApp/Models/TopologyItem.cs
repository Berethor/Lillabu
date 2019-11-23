using System.Diagnostics;

namespace LilaApp.Models
{
    [DebuggerDisplay("{FirstBlock} {SecondBlock} {Direction}")]
    public class TopologyItem
    {
        public int FirstBlock { get; set; }

        public int SecondBlock { get; set; }

        public int Direction { get; set; }

        public TopologyItem() { }

        public TopologyItem(int firstBlock, int secondBlock, int direction)
        {
            FirstBlock = firstBlock;
            SecondBlock = secondBlock;
            Direction = direction;
        }

        public override string ToString()
        {
            return $"{FirstBlock} {SecondBlock} {Direction}";
        }
    }
}
