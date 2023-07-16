namespace Game;
/// <summary>
/// Internal representation of a tile
/// </summary>
[Flags]
public enum Tile
{
    None = 0,
    Open = 1,
    HasMine = 2,
    HasFlag = 4,
    HasQuestionMark = 8
}

/// <summary>
/// State of the tile
/// </summary>
public struct TileState
{
    public bool IsOpen;
    public bool HasMine;
    public bool HasFlag;
    public bool HasQuestionMark;
    public int NeighboringMineCount;
}

public class Field
{
    protected List<List<Tile>> Tiles;
    public readonly int Width;
    public readonly int Height;

    
    public Field(int width, int height)
    {
        if (width <= 0 && height <= 0)
        {
            throw new ArgumentException("Width and Height must be greater than 0");
        }

        Width = width;
        Height = height;
        Tiles = new(width);
        for (int i = 0; i < width; i++)
        {
            Tiles.Add(new List<Tile>(height));
            for (int j = 0; j < height; j++)
            {
                Tiles[i].Add(Tile.None);
            }
        }
    }

    public TileState GetTileState(int x, int y)
    {
        TileState state;
        state.IsOpen = (Tiles[x][y] & Tile.Open) != 0;
        state.HasMine = (Tiles[x][y] & Tile.HasMine) != 0;
        state.HasFlag = (Tiles[x][y] & Tile.HasFlag) != 0;
        state.HasQuestionMark = (Tiles[x][y] & Tile.HasQuestionMark) != 0;
        state.NeighboringMineCount = GetNeighboringMineCount(x, y);
        return state;
    }

    private int GetNeighboringMineCount(int x, int y)
    {
        int count = 0;
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (i != j && i >= 0 && i < Width && j >= 0 && j < Height && (Tiles[i][j] & Tile.HasMine) != 0)
                {
                    count += 1;
                }
            }
        }
        return count;
    }

    /// <summary>
    /// Opens the tile at x, y
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <returns>True if the tile had a mine, false otherwise</returns>
    public bool OpenTile(int x, int y)
    {
        Tiles[x][y] |= Tile.Open;
        return (Tiles[x][y] & Tile.HasMine) != 0;
    }

    /// <summary>
    /// Cycles the marking on the tile in the next order: None -> Flag -> Question -> None...
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    public void ChangeTileMark(int x, int y)
    {
        if ((Tiles[x][y] & Tile.Open) != 0) {}
        else if ((Tiles[x][y] & Tile.HasFlag) != 0)
        {
            Tiles[x][y] ^= Tile.HasFlag;
            Tiles[x][y] ^= Tile.HasQuestionMark;
        }
        else if ((Tiles[x][y] & Tile.HasQuestionMark) != 0) Tiles[x][y] ^= Tile.HasQuestionMark;
        else Tiles[x][y] ^= Tile.HasFlag;
    }

    /// <summary>
    /// Cycles between having and not having a mine
    /// </summary>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    public void CycleMine(int x, int y)
    {
        Tiles[x][y] ^= Tile.HasMine;
    }
    
}