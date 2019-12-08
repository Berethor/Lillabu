using System;
using System.Diagnostics;

namespace LilaApp.Models
{
    [DebuggerDisplay("{FirstBlock} {SecondBlock} {Direction}")]
    public class TopologyItem : ICloneable
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
        public TopologyItem(TopologyItem item)
        {
            FirstBlock = item.FirstBlock;
            SecondBlock = item.SecondBlock;
            Direction = item.Direction;
        }

        public override string ToString()
        {
            return $"{FirstBlock} {SecondBlock} {Direction}";
        }

        #region Implementation of ICloneable

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
