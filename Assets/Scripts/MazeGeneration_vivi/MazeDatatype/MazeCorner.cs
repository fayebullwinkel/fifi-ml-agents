using System.Collections.Generic;

namespace MazeGeneration_vivi.MazeDatatype
{
    public class MazeCorner
    {
        public List<MazeCell> Cells { get; set; }
        public List<MazeWall> Walls { get; set; }
        
        public MazeCorner()
        {
            Cells = new List<MazeCell>();
            Walls = new List<MazeWall>();
        }

        public MazeCorner(List<MazeCell> cells, List<MazeWall> walls)
        {
            Cells = cells;
            Walls = walls;
        }
        
        public void AddCell(MazeCell cell)
        {
            Cells.Add(cell);
        }
        
        public void AddWall(MazeWall wall)
        {
            Walls.Add(wall);
        }
    }
}
