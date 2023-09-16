namespace MazeDatatype
{
    public class MazeGraph
    {
        private int _width;
        private int _height;
        
        private MazeCell[,] _mazeCells;
        
        public MazeGraph(int width, int height, bool withWalls = true)
        {
            _width = width;
            _height = height;
            _mazeCells = new MazeCell[width, height];
            
            // Initialize the maze cells
            for (var x = 0; x < _width; x++)
            {
                for (var z = 0; z < _height; z++)
                {
                    _mazeCells[x, z] = new MazeCell(x, z, withWalls);
                }
            }
        }
        
        public MazeCell[,] GetMazeCells()
        {
            return _mazeCells;
        }
        
        public MazeCell GetCell(int x, int z)
        {
            return _mazeCells[x, z];
        }
        
        public void SetCell(int x, int z, MazeCell cell)
        {
            _mazeCells[x, z] = cell;
        }
    }
}
