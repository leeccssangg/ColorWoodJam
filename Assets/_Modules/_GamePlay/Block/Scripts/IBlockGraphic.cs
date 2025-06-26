namespace Blocks
{
    public interface IBlockGraphic
    {
        
    }

    public class NullBlockGraphic : IBlockGraphic
    {
        public static readonly IBlockGraphic Instance = new NullBlockGraphic(); 
        private NullBlockGraphic() { }
    }
}