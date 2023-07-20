using System.Drawing;

namespace Game;

/// <summary>
/// Class for interacting with the field
/// </summary>
public class Player
{
    /// <summary>
    /// Reference to the field for easy access
    /// </summary>
    private Field _playField;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="field">Field the player is bound to</param>
    public Player(Field field)
    {
        _playField = field;
    }

    /// <summary>
    /// Cycles the marking on the tile in the next order: None -> Flag -> Question -> None...
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <param name="questionMarkEnabled">Whether to enable question marks: None -> Flag -> None...</param>
    public void CycleMark(int x, int y, bool questionMarkEnabled)
    {
        if (!questionMarkEnabled && _playField.GetTileState(x,y).HasFlag)
        {
            _playField.ChangeTileMark(x,y);
            _playField.ChangeTileMark(x,y);
        }
        else
        {
            _playField.ChangeTileMark(x,y);
        }
    }

    /// <summary>
    /// Opens the tile and attempts to open adjacent tiles
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <returns>True if the tile had a mine, false otherwise</returns>
    public bool OpenTile(int x, int y)
    {
        if (_playField.OpenTile(x, y))
        {
            return true;
        }

        if (_playField.GetTileState(x,y).NeighboringMineCount == 0)
        {
            OpenZeroes(x,y);
        }
        else
        {
            //TODO: add flagging adjacent unopened tiles if the amount of unopened tiles is equal to tile's mine count
            int adjCount = _playField.GetTileState(x, y).NeighboringMineCount;
            int flagCount = 0;
            bool foundQuestion = false;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if ((i != 0 || j != 0) && x + i >= 0 && x + i < _playField.Width && y + j >= 0 && y + j < _playField.Height )
                    {
                        if (_playField.GetTileState(x + i,y + j).HasFlag)
                        {
                            flagCount += 1;
                        }
                        if (_playField.GetTileState(x + i,y + j).HasQuestionMark)
                        {
                            foundQuestion = true;
                        }
                    }
                }
            }

            if (flagCount == adjCount && !foundQuestion)
            {
                if (OpenAdjacentTiles(x, y))
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    /// <summary>
    /// Opens adjacent tiles non flagged tiles
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <returns>True if one of the tiles had a mine, false otherwise</returns>
    private bool OpenAdjacentTiles(int x, int y)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if ((i != 0 || j != 0) && x + i >= 0 && x + i < _playField.Width && y + j >= 0 && y + j < _playField.Height && !_playField.GetTileState(x + i, y + j).HasFlag)
                {
                    VoidMark(x + i, y + j);
                    if (_playField.OpenTile(x + i,y + j))
                    {
                        return true;
                    }

                    if (_playField.GetTileState(x + i,y + j).NeighboringMineCount == 0)
                    {
                        OpenZeroes(x + i,y + j);
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Removes any mark from the tile
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    private void VoidMark(int x, int y)
    {
        var tile = _playField.GetTileState(x, y);
        if (tile.HasFlag)
        {
            CycleMark(x,y, false);
        }
        else if (tile.HasQuestionMark)
        {
            CycleMark(x,y,true);
        }
    }

    /// <summary>
    /// Opens a contiguous region of zeroes
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    private void OpenZeroes(int x, int y)
    {
        HashSet<Point> toCheck = new ();
        HashSet<Point> visited = new ();
        toCheck.Add(new Point(x, y));
        while (toCheck.Count > 0)
        {
            HashSet<Point> upForCheck = new ();
            foreach (var point in toCheck)
            {
                visited.Add(point);
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if ((i != 0 || j != 0) && point.X + i >= 0 && point.X + i < _playField.Width && point.Y + j >= 0 && point.Y + j < _playField.Height)
                        {
                            VoidMark(point.X + i, point.Y + j);
                            _playField.OpenTile(point.X + i, point.Y + j);
                            if (_playField.GetTileState(point.X + i, point.Y + j).NeighboringMineCount == 0 && !visited.Contains(new Point(point.X + i, point.Y + j)) && !toCheck.Contains(new Point(point.X + i,point.Y + j)))
                            {
                                upForCheck.Add(new Point(point.X + i, point.Y + j));
                            }
                        }
                        
                    }
                }
            }

            toCheck.Clear();
            foreach (var point in upForCheck)
            {
                toCheck.Add(point);
            }
        }
    }
}