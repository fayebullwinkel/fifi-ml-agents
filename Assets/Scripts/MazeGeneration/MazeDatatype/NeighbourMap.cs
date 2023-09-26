using System.Collections.Generic;
using MazeDatatype.Enums;

namespace MazeDatatype
{
    public class NeighbourMap
    {
        public static readonly Dictionary<ECubeFace, Dictionary<EDirection, ECubeFace>> Map = new()
        {
            {
                ECubeFace.Front,
                new Dictionary<EDirection, ECubeFace>
                {
                    { EDirection.Left, ECubeFace.Left }, { EDirection.Right, ECubeFace.Right },
                    { EDirection.Top, ECubeFace.Top }, { EDirection.Bottom, ECubeFace.Bottom }
                }
            },
            {
                ECubeFace.Back,
                new Dictionary<EDirection, ECubeFace>
                {
                    { EDirection.Left, ECubeFace.Right }, { EDirection.Right, ECubeFace.Left },
                    { EDirection.Top, ECubeFace.Top }, { EDirection.Bottom, ECubeFace.Bottom }
                }
            },
            {
                ECubeFace.Left,
                new Dictionary<EDirection, ECubeFace>
                {
                    { EDirection.Left, ECubeFace.Back }, { EDirection.Right, ECubeFace.Front },
                    { EDirection.Top, ECubeFace.Top }, { EDirection.Bottom, ECubeFace.Bottom }
                }
            },
            {
                ECubeFace.Right,
                new Dictionary<EDirection, ECubeFace>
                {
                    { EDirection.Left, ECubeFace.Front }, { EDirection.Right, ECubeFace.Back },
                    { EDirection.Top, ECubeFace.Top }, { EDirection.Bottom, ECubeFace.Bottom }
                }
            },
            {
                ECubeFace.Top,
                new Dictionary<EDirection, ECubeFace>
                {
                    { EDirection.Left, ECubeFace.Left }, { EDirection.Right, ECubeFace.Right },
                    { EDirection.Top, ECubeFace.Back }, { EDirection.Bottom, ECubeFace.Front }
                }
            },
            {
                ECubeFace.Bottom,
                new Dictionary<EDirection, ECubeFace>
                {
                    { EDirection.Left, ECubeFace.Left }, { EDirection.Right, ECubeFace.Right },
                    { EDirection.Top, ECubeFace.Front }, { EDirection.Bottom, ECubeFace.Back }
                }
            },
        };
    }
}