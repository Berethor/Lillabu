using System.Diagnostics;

namespace LilaApp.Models
{
    [DebuggerDisplay("{Name} {Count} {Price}")]
    public class Block
    {
        /// <summary>
        /// Название блока
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Количество
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Цена одного блока
        /// </summary>
        public double Price { get; set; }

        public Block() { }

        public Block(string name, int count, double price)
        {
            Name = name;
            Count = count;
            Price = price;
        }
    }
}
