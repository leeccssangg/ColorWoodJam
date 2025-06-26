
using UnityEngine;

namespace Blocks
{
    public interface IBlock
    {
        public int ColorId { get; }

        public void SetColor(int colorId, Color color);
    }
}