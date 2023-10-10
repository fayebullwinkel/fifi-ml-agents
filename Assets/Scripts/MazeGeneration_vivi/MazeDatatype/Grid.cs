using System;
using System.Collections.Generic;
using System.Linq;
using MazeGeneration_vivi.MazeDatatype.Enums;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MazeGeneration_vivi.MazeDatatype
{
    public class Grid
    {
        public Maze Maze { get; }
        public ECubeFace Face { get; }
        private int Size { get; }
        public GameObject Parent;
        public MazeCell[,] Cells { get; }
        public List<MazeWall> Walls { get; }
        public List<MazeCorner> Corners { get; }
        public List<GameObject> VisitedCellsPath { get; set; }
        public GameObject VisitedCellsPathParent { get; set; }
        private PrefabCollection prefabCollection;
        private TextMeshProUGUI visitedCellsText;

        public Grid(Maze maze, int size, GameObject parent, ECubeFace face = ECubeFace.None, TextMeshProUGUI visitedCellsTextPro = null)
        {
            Maze = maze;
            Size = size;
            Face = face;
            visitedCellsText = visitedCellsTextPro;
            Cells = new MazeCell[size, size];
            Walls = new List<MazeWall>();
            Corners = new List<MazeCorner>();
            VisitedCellsPath = new List<GameObject>();
            Parent = parent;
            prefabCollection = Maze.prefabCollection;
            
            // Initialize the maze cells
            for (var x = 0; x < Size; x++)
            {
                for (var z = 0; z < Size; z++)
                {
                    Cells[x, z] = new MazeCell(x, z, this);
                }
            }
        }

        public void SetupGrid()
        {
            InitializeNeighbours();
            InitializeWalls();

            if (Maze.showCellCoordinates)
            {
                //PrintAllCells();
                ShowAllCells();
            }
        }

        #region InitializationMethods

        private void InitializeNeighbours()
        {
            for (var x = 0; x < Size; x++)
            {
                for (var z = 0; z < Size; z++)
                {
                    var cell = Cells[x, z];
                    // if the cell is not on the left edge, add the cell to the left as a neighbour
                    if (x > 0)
                    {
                        cell.AddNeighbour(Cells[x - 1, z]);
                    }
                    // if the cell is on the left edge, add the cell on the right edge from the left neighbour graph as a neighbour
                    else if (x == 0)
                    {
                        var neighbourCell = FindNeighbourCellFromNeighbourGrid(cell, EDirection.Left);
                        cell.AddNeighbour(neighbourCell);
                    }
                    // if the cell is not on the right edge, add the cell to the right as a neighbour
                    if (x < Size - 1)
                    {
                        cell.AddNeighbour(Cells[x + 1, z]);
                    }
                    else if (x == Size - 1)
                    {
                        var neighbourCell = FindNeighbourCellFromNeighbourGrid(cell, EDirection.Right);
                        cell.AddNeighbour(neighbourCell);
                    }
                    // if the cell is not on the bottom edge, add the cell to the bottom as a neighbour
                    if (z > 0)
                    {
                        cell.AddNeighbour(Cells[x, z - 1]);
                    }
                    else if (z == 0)
                    {
                        var neighbourCell = FindNeighbourCellFromNeighbourGrid(cell, EDirection.Bottom);
                        cell.AddNeighbour(neighbourCell);
                    }
                    // if the cell is not on the top edge, add the cell to the top as a neighbour
                    if (z < Size - 1)
                    {
                        cell.AddNeighbour(Cells[x, z + 1]);
                    }
                    else if (z == Size - 1)
                    {
                        var neighbourCell = FindNeighbourCellFromNeighbourGrid(cell, EDirection.Top);
                        cell.AddNeighbour(neighbourCell);
                    }
                }
            }
        }

        private void InitializeWalls()
        {
            var wallParent = new GameObject("wallParent");
            wallParent.transform.parent = Parent.transform;
            wallParent.transform.localPosition = new Vector3(0, 0.5f, 0);
            
            for (var x = 0; x < Size; x++)
            {
                for (var z = 0; z < Size; z++)
                {
                    var cell = Cells[x, z];
                    PlaceWall(cell, EDirection.Left, WallType.Vertical, wallParent);
                    PlaceWall(cell, EDirection.Right, WallType.Vertical, wallParent);
                    PlaceWall(cell, EDirection.Bottom, WallType.Horizontal, wallParent);
                    PlaceWall(cell, EDirection.Top, WallType.Horizontal, wallParent);
                }
            }
        }

        public void InitializeCorners()
        {
            // create four corners for every cell in the grid 
            for (var i = 0; i <= Size; i++)
            {
                for(var j = 0; j <= Size; j++)
                {
                    // CubeCorner Corners -> only three cells
                    if (i == 0 && j == 0)
                    {
                        var cornerCells = new List<MazeCell>()
                        {
                            Cells[i, j],
                        };
                        cornerCells.Add(GetNeighborCell(cornerCells[0], EDirection.Bottom));
                        cornerCells.Add(GetNeighborCell(cornerCells[0], EDirection.Left));
                        var cornerWalls = new List<MazeWall>()
                        {
                            Walls.Find(x => x.Cells.Contains(cornerCells[0]) && x.Cells.Contains(cornerCells[1])),
                            Walls.Find(x => x.Cells.Contains(cornerCells[0]) && x.Cells.Contains(cornerCells[2])),
                        };
                        // add walls from the neighbor grids
                        var neighborGrid1 = cornerCells[1].Grid;
                        cornerWalls.Add(neighborGrid1.Walls.Find(x => x.Cells.Contains(cornerCells[1]) && x.Cells.Contains(cornerCells[2])));
                        cornerWalls.Add(neighborGrid1.Walls.Find(x => x.Cells.Contains(cornerCells[0]) && x.Cells.Contains(cornerCells[1])));
                        var neighborGrid2 = cornerCells[2].Grid;
                        cornerWalls.Add(neighborGrid2.Walls.Find(x => x.Cells.Contains(cornerCells[1]) && x.Cells.Contains(cornerCells[2])));
                        cornerWalls.Add(neighborGrid2.Walls.Find(x => x.Cells.Contains(cornerCells[0]) && x.Cells.Contains(cornerCells[2])));

                        var cornerCorner = new MazeCorner(cornerCells, cornerWalls);
                        Corners.Add(cornerCorner);
                        continue;
                    }
                    if (i == 0 && j == Size)
                    {
                        var cornerCells = new List<MazeCell>()
                        {
                            Cells[i, j - 1],
                        };
                        cornerCells.Add(GetNeighborCell(cornerCells[0], EDirection.Top));
                        cornerCells.Add(GetNeighborCell(cornerCells[0], EDirection.Left));
                        var cornerWalls = new List<MazeWall>()
                        {
                            Walls.Find(x => x.Cells.Contains(cornerCells[0]) && x.Cells.Contains(cornerCells[1])),
                            Walls.Find(x => x.Cells.Contains(cornerCells[0]) && x.Cells.Contains(cornerCells[2])),
                        };
                        // add walls from the neighbor grids
                        var neighborGrid1 = cornerCells[1].Grid;
                        cornerWalls.Add(neighborGrid1.Walls.Find(x => x.Cells.Contains(cornerCells[1]) && x.Cells.Contains(cornerCells[2])));
                        cornerWalls.Add(neighborGrid1.Walls.Find(x => x.Cells.Contains(cornerCells[0]) && x.Cells.Contains(cornerCells[1])));
                        var neighborGrid2 = cornerCells[2].Grid;
                        cornerWalls.Add(neighborGrid2.Walls.Find(x => x.Cells.Contains(cornerCells[1]) && x.Cells.Contains(cornerCells[2])));
                        cornerWalls.Add(neighborGrid2.Walls.Find(x => x.Cells.Contains(cornerCells[0]) && x.Cells.Contains(cornerCells[2])));

                        var cornerCorner = new MazeCorner(cornerCells, cornerWalls);
                        Corners.Add(cornerCorner);
                        continue;
                    }
                    if (i == Size && j == 0)
                    {
                        var cornerCells = new List<MazeCell>()
                        {
                            Cells[i - 1, j],
                        };
                        cornerCells.Add(GetNeighborCell(cornerCells[0], EDirection.Bottom));
                        cornerCells.Add(GetNeighborCell(cornerCells[0], EDirection.Right));
                        var cornerWalls = new List<MazeWall>()
                        {
                            Walls.Find(x => x.Cells.Contains(cornerCells[0]) && x.Cells.Contains(cornerCells[1])),
                            Walls.Find(x => x.Cells.Contains(cornerCells[0]) && x.Cells.Contains(cornerCells[2])),
                        };
                        // add walls from the neighbor grids
                        var neighborGrid1 = cornerCells[1].Grid;
                        cornerWalls.Add(neighborGrid1.Walls.Find(x => x.Cells.Contains(cornerCells[1]) && x.Cells.Contains(cornerCells[2])));
                        cornerWalls.Add(neighborGrid1.Walls.Find(x => x.Cells.Contains(cornerCells[0]) && x.Cells.Contains(cornerCells[1])));
                        var neighborGrid2 = cornerCells[2].Grid;
                        cornerWalls.Add(neighborGrid2.Walls.Find(x => x.Cells.Contains(cornerCells[1]) && x.Cells.Contains(cornerCells[2])));
                        cornerWalls.Add(neighborGrid2.Walls.Find(x => x.Cells.Contains(cornerCells[0]) && x.Cells.Contains(cornerCells[2])));

                        var cornerCorner = new MazeCorner(cornerCells, cornerWalls);
                        Corners.Add(cornerCorner);
                        continue;
                    }
                    if(i == Size && j == Size)
                    {
                        var cornerCells = new List<MazeCell>()
                        {
                            Cells[i - 1, j - 1],
                        };
                        cornerCells.Add(GetNeighborCell(cornerCells[0], EDirection.Top));
                        cornerCells.Add(GetNeighborCell(cornerCells[0], EDirection.Right));
                        var cornerWalls = new List<MazeWall>()
                        {
                            Walls.Find(x => x.Cells.Contains(cornerCells[0]) && x.Cells.Contains(cornerCells[1])),
                            Walls.Find(x => x.Cells.Contains(cornerCells[0]) && x.Cells.Contains(cornerCells[2])),
                        };
                        // add walls from the neighbor grids
                        var neighborGrid1 = cornerCells[1].Grid;
                        cornerWalls.Add(neighborGrid1.Walls.Find(x => x.Cells.Contains(cornerCells[1]) && x.Cells.Contains(cornerCells[2])));
                        cornerWalls.Add(neighborGrid1.Walls.Find(x => x.Cells.Contains(cornerCells[0]) && x.Cells.Contains(cornerCells[1])));
                        var neighborGrid2 = cornerCells[2].Grid;
                        cornerWalls.Add(neighborGrid2.Walls.Find(x => x.Cells.Contains(cornerCells[1]) && x.Cells.Contains(cornerCells[2])));
                        cornerWalls.Add(neighborGrid2.Walls.Find(x => x.Cells.Contains(cornerCells[0]) && x.Cells.Contains(cornerCells[2])));

                        var cornerCorner = new MazeCorner(cornerCells, cornerWalls);
                        Corners.Add(cornerCorner);
                        continue;
                    }

                    var cells = new List<MazeCell>();
                    var walls = new List<MazeWall>();
                    // corner on left edge
                    if (i == 0)
                    {
                        cells = new List<MazeCell>
                        {
                            Cells[i, j - 1],
                            Cells[i, j]
                        };
                        cells.Add(GetNeighborCell(cells[0], EDirection.Left));
                        cells.Add(GetNeighborCell(cells[1], EDirection.Left));
                        walls = new List<MazeWall>()
                        {
                            Walls.Find(x => x.Cells.Contains(cells[0]) && x.Cells.Contains(cells[1])),
                            Walls.Find(x => x.Cells.Contains(cells[0]) && x.Cells.Contains(cells[2])),
                            Walls.Find(x => x.Cells.Contains(cells[1]) && x.Cells.Contains(cells[3])),
                        };
                        var neighborGrid = cells[2].Grid;
                        walls.Add(neighborGrid.Walls.Find(x => x.Cells.Contains(cells[2]) && x.Cells.Contains(cells[3])));
                        walls.Add(neighborGrid.Walls.Find(x => x.Cells.Contains(cells[0]) && x.Cells.Contains(cells[2])));
                        walls.Add(neighborGrid.Walls.Find(x => x.Cells.Contains(cells[1]) && x.Cells.Contains(cells[3])));
                    }
                    // corner on right edge
                    else if(i == Size)
                    {
                        cells = new List<MazeCell>
                        {
                            Cells[i - 1, j - 1],
                            Cells[i - 1, j],
                        };
                        cells.Add(GetNeighborCell(cells[0], EDirection.Right));
                        cells.Add(GetNeighborCell(cells[1], EDirection.Right));
                        walls = new List<MazeWall>()
                        {
                            Walls.Find(x => x.Cells.Contains(cells[0]) && x.Cells.Contains(cells[1])),
                            Walls.Find(x => x.Cells.Contains(cells[0]) && x.Cells.Contains(cells[2])),
                            Walls.Find(x => x.Cells.Contains(cells[1]) && x.Cells.Contains(cells[3])),
                        };
                        var neighborGrid = cells[2].Grid;
                        walls.Add(neighborGrid.Walls.Find(x => x.Cells.Contains(cells[2]) && x.Cells.Contains(cells[3])));
                        walls.Add(neighborGrid.Walls.Find(x => x.Cells.Contains(cells[0]) && x.Cells.Contains(cells[2])));
                        walls.Add(neighborGrid.Walls.Find(x => x.Cells.Contains(cells[1]) && x.Cells.Contains(cells[3])));
                    }
                    // corner on bottom edge
                    else if (j == 0)
                    {
                        cells = new List<MazeCell>
                        {
                            Cells[i - 1, j],
                            Cells[i, j]
                        };
                        cells.Add(GetNeighborCell(cells[0], EDirection.Bottom));
                        cells.Add(GetNeighborCell(cells[1], EDirection.Bottom));
                        walls = new List<MazeWall>()
                        {
                            Walls.Find(x => x.Cells.Contains(cells[0]) && x.Cells.Contains(cells[1])),
                            Walls.Find(x => x.Cells.Contains(cells[0]) && x.Cells.Contains(cells[2])),
                            Walls.Find(x => x.Cells.Contains(cells[1]) && x.Cells.Contains(cells[3])),
                        };
                        var neighborGrid = cells[2].Grid;
                        walls.Add(neighborGrid.Walls.Find(x => x.Cells.Contains(cells[2]) && x.Cells.Contains(cells[3])));
                        walls.Add(neighborGrid.Walls.Find(x => x.Cells.Contains(cells[0]) && x.Cells.Contains(cells[2])));
                        walls.Add(neighborGrid.Walls.Find(x => x.Cells.Contains(cells[1]) && x.Cells.Contains(cells[3])));
                    }
                    // corner on top edge
                    else if (j == Size)
                    {
                        cells = new List<MazeCell>
                        {
                            Cells[i - 1, j - 1],
                            Cells[i, j - 1],
                        };
                        cells.Add(GetNeighborCell(cells[0], EDirection.Top));
                        cells.Add(GetNeighborCell(cells[1], EDirection.Top));
                        walls = new List<MazeWall>()
                        {
                            Walls.Find(x => x.Cells.Contains(cells[0]) && x.Cells.Contains(cells[1])),
                            Walls.Find(x => x.Cells.Contains(cells[0]) && x.Cells.Contains(cells[2])),
                            Walls.Find(x => x.Cells.Contains(cells[1]) && x.Cells.Contains(cells[3])),
                        };
                        var neighborGrid = cells[2].Grid;
                        walls.Add(neighborGrid.Walls.Find(x => x.Cells.Contains(cells[2]) && x.Cells.Contains(cells[3])));
                        walls.Add(neighborGrid.Walls.Find(x => x.Cells.Contains(cells[0]) && x.Cells.Contains(cells[2])));
                        walls.Add(neighborGrid.Walls.Find(x => x.Cells.Contains(cells[1]) && x.Cells.Contains(cells[3])));
                    }
                    else
                    {
                        // get the four cells that are connected to the corner
                        cells = new List<MazeCell>
                        {
                            Cells[i - 1, j - 1],
                            Cells[i - 1, j],
                            Cells[i, j - 1],
                            Cells[i, j]
                        };
                        // get the four walls that are connected to the corner
                        // the walls that connects two cells in cells are connected to the corner
                        walls = new List<MazeWall>()
                        {
                            Walls.Find(x => x.Cells.Contains(cells[0]) && x.Cells.Contains(cells[1])),
                            Walls.Find(x => x.Cells.Contains(cells[0]) && x.Cells.Contains(cells[2])),
                            Walls.Find(x => x.Cells.Contains(cells[1]) && x.Cells.Contains(cells[3])),
                            Walls.Find(x => x.Cells.Contains(cells[2]) && x.Cells.Contains(cells[3]))
                        };
                    }
                    var corner = new MazeCorner(cells, walls);
                    // check if the corner is already in the list
                    if (!Corners.Contains(corner))
                    {
                        Corners.Add(corner);
                    }
                }
            }
        }
        
        #endregion
        
        #region NeighbourMethods

        public Grid GetNeighbourGrid(EDirection direction)
        {
            var thisFace = Face;
            if (!NeighbourMap.Map.TryGetValue(thisFace, out var neighbourMap) ||
                !neighbourMap.TryGetValue(direction, out var neighbourFace))
            {
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            return Maze.Grids[neighbourFace];
        }

        private MazeCell FindNeighbourCellFromNeighbourGrid(MazeCell cell, EDirection direction)
        {
            var neighbourGraph = GetNeighbourGrid(direction);
            var face = cell.Grid.Face;
            var neighbourCell = neighbourGraph.Cells[cell.X, cell.Z];
            switch (face)
            {
                case ECubeFace.None:
                    break;
                case ECubeFace.Front:
                    neighbourCell = direction switch
                    {
                        EDirection.Left => neighbourGraph.Cells[Size - 1, cell.Z],
                        EDirection.Right => neighbourGraph.Cells[0, cell.Z],
                        EDirection.Top => neighbourGraph.Cells[cell.X, 0],
                        EDirection.Bottom => neighbourGraph.Cells[cell.X, Size - 1],
                        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                    };
                    break;
                case ECubeFace.Back:
                    neighbourCell = direction switch
                    {
                        EDirection.Left => neighbourGraph.Cells[Size - 1, cell.Z],
                        EDirection.Right => neighbourGraph.Cells[0, cell.Z],
                        // for top: z is size -1 and x is inverted (x = size - 1 - x)
                        EDirection.Top => neighbourGraph.Cells[Size - 1 - cell.X, Size - 1],
                        // for bottom: z is 0 and x is inverted (x = size - 1 - x)
                        EDirection.Bottom => neighbourGraph.Cells[Size - 1 - cell.X, 0],
                        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                    };
                    break;
                case ECubeFace.Left:
                    neighbourCell = direction switch
                    {
                        EDirection.Left => neighbourGraph.Cells[Size - 1, cell.Z],
                        EDirection.Right => neighbourGraph.Cells[0, cell.Z],
                        // for top: x is 0 and z is inverted (z = size - 1 - x)
                        EDirection.Top => neighbourGraph.Cells[0, Size - 1 - cell.X],
                        // for bottom: x is 0  and z is x
                        EDirection.Bottom => neighbourGraph.Cells[0, cell.X],
                    };
                    break;
                case ECubeFace.Right:
                    neighbourCell = direction switch
                    {
                        EDirection.Left => neighbourGraph.Cells[Size - 1, cell.Z],
                        EDirection.Right => neighbourGraph.Cells[0, cell.Z],
                        // for top: x is size - 1 and z is x
                        EDirection.Top => neighbourGraph.Cells[Size - 1, cell.X],
                        // for bottom: x is size - 1 and z is inverted (z = size - 1 - x)
                        EDirection.Bottom => neighbourGraph.Cells[Size - 1, Size - 1 - cell.X],
                    };
                    break;
                case ECubeFace.Top:
                    neighbourCell = direction switch
                    {
                        // for left: z is size - 1 and x is inverted (x = size - 1 - z)
                        EDirection.Left => neighbourGraph.Cells[Size - 1 - cell.Z, Size - 1],
                        // for right: z is size - 1 and x is z
                        EDirection.Right => neighbourGraph.Cells[cell.Z, Size - 1],
                        // for top: z is size - 1 and x is inverted (x = size - 1 - x)
                        EDirection.Top => neighbourGraph.Cells[Size - 1 - cell.X, Size - 1],
                        // for bottom: z is size - 1 and x is x
                        EDirection.Bottom => neighbourGraph.Cells[cell.X, Size - 1],
                    };
                    break;
                case ECubeFace.Bottom:
                    neighbourCell = direction switch
                    {
                        // for left: z is 0 and x is z
                        EDirection.Left => neighbourGraph.Cells[cell.Z, 0],
                        // for right: z is 0 and x is inverted (x = size - 1 - z)
                        EDirection.Right => neighbourGraph.Cells[Size - 1 - cell.Z, 0],
                        // for top: z is 0 and x is x
                        EDirection.Top => neighbourGraph.Cells[cell.X, 0],
                        // for bottom: z is 0 and x is inverted (x = size - 1 - x)
                        EDirection.Bottom => neighbourGraph.Cells[Size - 1 - cell.X, 0],
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return neighbourCell;
        }

        public MazeCell GetNeighborCell(MazeCell currentCell, EDirection direction)
        {
            var (x, z) = GetNeighborCoordinates(currentCell.X, currentCell.Z, direction);
            // check if coordinates are out of bounds
            if (x < 0 || x >= Size || z < 0 || z >= Size)
            {
                // get the neighbour cell from the neighbour grid
                var neighbourGridFace = GetNeighbourGrid(direction).Face;
                var neighbour = currentCell.Neighbours.Find(c => c.Grid.Face == neighbourGridFace);
                return neighbour;
            }
            var cell = Cells[x, z];
            return cell;
        }

        private (int, int) GetNeighborCoordinates(int x, int z, EDirection direction)
        {
            return direction switch
            {
                EDirection.Left => (x - 1, z),
                EDirection.Right => (x + 1, z),
                EDirection.Bottom => (x, z - 1),
                EDirection.Top => (x, z + 1),
                _ => throw new ArgumentException("Invalid wall orientation")
            };
        }
        
        #endregion

        #region WallMethods

        private void PlaceWall(MazeCell currentCell, EDirection direction, WallType wallType,
            GameObject wallParent)
        {
            var neighborCell = GetNeighborCell(currentCell, direction);
            var position = GetWallPosition(currentCell, direction);
            var grid = currentCell.Grid;
            var existingWall = GetExistingWall(position);

            if (existingWall != null || neighborCell == null)
            {
                return;
            }
            var wallObject = Object.Instantiate(prefabCollection.wallPrefab, wallParent.transform);
            wallObject.transform.localPosition = position;
            wallObject.transform.localScale = GetWallScale(wallType);
            var wall = wallObject.GetComponent<MazeWall>();
            var cells = new List<MazeCell> {currentCell, neighborCell};
            wall.InitMazeWall(Maze, grid, wallType, cells);
            currentCell.AddWall(wall);
            neighborCell.AddWall(wall);
            Walls.Add(wall);
        }
        
        public MazeWall GetExistingWall(Vector3 position)
        {
            return Walls.Find(x => x.gameObject.transform.localPosition == position);
        }
        
        private Vector3 GetWallPosition(MazeCell cell, EDirection direction)
        {
            var cellSize = Maze.cellSize;
            var position = GetPositionFromCell(cell);

            switch (direction)
            {
                case EDirection.Top:
                    position.z += cellSize / 2;
                    break;
                case EDirection.Bottom:
                    position.z -= cellSize / 2;
                    break;
                case EDirection.Left:
                    position.x -= cellSize / 2;
                    break;
                case EDirection.Right:
                    position.x += cellSize / 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            return position;
        }
        
        private Vector3 GetWallScale(WallType type)
        {
            return type == WallType.Horizontal ? new Vector3(Maze.cellSize, 1, 0.1f) : new Vector3(0.1f, 1, Maze.cellSize);
        }

        public void RemoveWall(MazeWall wall)
        {
            Walls.Remove(wall);
            // remove wall from every corner that contains it
            foreach (var corner in Corners)
            {
                if (corner.Walls.Contains(wall))
                {
                    corner.Walls.Remove(wall);
                }
            }
            // Remove wall from cells
            foreach (var cell in wall.Cells)
            {
                cell.Walls.Remove(wall);
            }
        }
        
        #endregion

        #region PositionMethods
        
        public MazeCell GetCellFromPosition(Vector3 position)
        {
            var size = Maze.size;
            var cellSize = Maze.cellSize;
            var xOffset = size * cellSize / 2;
            var zOffset = size * cellSize / 2;
            var xPos = Mathf.FloorToInt((position.x + xOffset) / cellSize);
            var zPos = Mathf.FloorToInt((position.z + zOffset) / cellSize);
            return Cells[xPos, zPos];
        }

        public Vector3 GetPositionFromCell(MazeCell cell)
        {
            var size = Maze.size;
            var cellSize = Maze.cellSize;
            
            var xOffset = size * cellSize / 2;
            var zOffset = size * cellSize / 2;

            var xPos = cell.X * cellSize - xOffset + 1;
            var zPos = cell.Z * cellSize - zOffset + 1;

            return new Vector3(xPos, 0f, zPos);
        }

        #endregion

        #region GeneralMethods
        
        public void UpdateVisitedCellsText()
        {
            visitedCellsText.text = $"Visited Cells: {GetVisitedCells()}/{Cells.Length}";
        }

        public int GetVisitedCells()
        {
            return Cells.Cast<MazeCell>().Count(cell => cell.Visited);
        }
        
        #endregion

        #region DebugMethods
        
        public void PrintAllCells()
        {
            foreach (var cell in Cells)
            {
                Debug.Log($"------- Face: {Face} -------");
                Debug.Log($"Cell: ({cell.X}, {cell.Z})");

                // Print neighbors
                foreach (var neighbor in cell.Neighbours)
                {
                    Debug.Log($"Neighbour: ({neighbor.X}, {neighbor.Z}), Face: {neighbor.Grid.Face}");
                }
            }
        }
        
        public void ShowAllCells()
        {
            foreach (var cell in Cells)
            {
                var position = GetPositionFromCell(cell);
                var cellObject = Object.Instantiate(prefabCollection.cellWithCoordinatesPrefab, Parent.transform);
                cellObject.transform.localPosition = position;
                cellObject.transform.localScale = new Vector3(Maze.cellSize, 0.01f, Maze.cellSize);

                var textMesh = cellObject.GetComponentInChildren<TextMeshPro>();
                textMesh.text = $"({cell.X}, {cell.Z})";
                textMesh.alignment = TextAlignmentOptions.Center;
            }
        }
        
        #endregion
    }
}
