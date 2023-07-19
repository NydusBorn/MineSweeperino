using System.Drawing;

namespace Game;

/// <summary>
/// Game container used for starting and organizing the game
/// </summary>
public class Session
{
    /// <summary>
    /// Field for the game
    /// </summary>
    public Field PlayField;
    /// <summary>
    /// Player actions
    /// </summary>
    public Player Actor;
    /// <summary>
    /// Solver for the game
    /// </summary>
    public Solver AutoActor;
    /// <summary>
    /// Amount of mines generated for the game
    /// </summary>
    public int MineCount { get; private set; }
    
    /// <summary>
    /// Generates a session from a mine percentage
    /// </summary>
    /// <param name="width">Width of the field</param>
    /// <param name="height">Height of the field</param>
    /// <param name="percentage">How likely is the tile going to be a mine</param>
    /// <param name="guaranteedSolution">Must be possible to solve without guessing</param>
    /// <param name="startingPoint">Where to place the starter zone</param>
    /// <returns>Session ready to play</returns>
    /// <exception cref="ArgumentException">Percentage must be between 0 and 1, and starting point must be within the field</exception>
    public static Session GenerateFromPercentage(int width, int height, double percentage, bool guaranteedSolution, Point startingPoint)
    {
        if (percentage < 0 || percentage > 1)
        {
            throw new ArgumentException("Percentage must be between 0 and 1");
        }
        if (startingPoint.X < 0 || startingPoint.X >= width || startingPoint.Y < 0 || startingPoint.Y >= height)
        {
            throw new ArgumentException("Starting point must be within the field");
        }
        Session result = new Session
        {
            PlayField = new Field(width, height)
        };
        Random rnd = new Random();
        int mineCount = 0;
        var pattern = result.GetGenerationPattern(startingPoint);
        foreach (var point in pattern)
        {
            if (rnd.NextDouble() < percentage)
            {
                result.PlayField.CycleMine(point.X, point.Y);
                mineCount += 1;
            }
        }

        result.MineCount = mineCount;
        result.Actor = new Player(result.PlayField);
        result.AutoActor = new Solver(result.PlayField);
        result.Actor.OpenTile(startingPoint.X, startingPoint.Y);
        return result;
    }
    
   
    /// <summary>
    /// Generates a session from a mine count
    /// </summary>
    /// <param name="width">Width of the field</param>
    /// <param name="height">Height of the field</param>
    /// <param name="mineCount">How many mines have to be placed</param>
    /// <param name="guaranteedSolution">Must be possible to solve without guessing</param>
    /// <param name="startingPoint">Where to place the starter zone</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Mine count must be 0 or greater, and starting point must be within the field</exception>
    public static Session GenerateFromCount(int width, int height, int mineCount, bool guaranteedSolution, Point startingPoint)
    {
        if (mineCount < 0)
        {
            throw new ArgumentException("Mine count must be 0 or greater");
        }
        if (startingPoint.X < 0 || startingPoint.X >= width || startingPoint.Y < 0 || startingPoint.Y >= height)
        {
            throw new ArgumentException("Starting point must be within the field");
        }
        Session result = new Session
        {
            PlayField = new Field(width, height)
        };
        Random rnd = new Random();
        var pattern = result.GetGenerationPattern(startingPoint);
        int prospected = 0;
        int placed = 0;
        foreach (var point in pattern)
        {
            if (rnd.NextDouble() < (double)(mineCount - placed) / (pattern.Count - prospected))
            {
                result.PlayField.CycleMine(point.X, point.Y);
                placed += 1;
            }
            prospected += 1;
        }

        result.MineCount = placed;
        result.Actor = new Player(result.PlayField);
        result.AutoActor = new Solver(result.PlayField);
        result.Actor.OpenTile(startingPoint.X, startingPoint.Y);
        return result;
    }

    /// <summary>
    /// Creates a sequence of points that starts from points close to the start and ends with the furthest points,, the sequence excludes the starter area (usually a 3x3 square).
    /// </summary>
    /// <param name="startingPoint"> Where to start the spiral</param>
    /// <returns>Spiral pattern with center at startingPoint</returns>
    private List<Point> GetGenerationPattern(Point startingPoint)
    {
        List<Point> sequence = new List<Point>();
        for (int i = 0; i < PlayField.Width; i++)
        {
            for (int j = 0; j < PlayField.Height; j++)
            {
                if (i < startingPoint.X - 1 || i > startingPoint.X + 1 || j < startingPoint.Y - 1 || j > startingPoint.Y + 1)
                {
                    sequence.Add(new Point(i,j));
                }
            }
        }
        sequence.Sort((x, y) => (Math.Abs(x.X - startingPoint.X) + Math.Abs(x.Y - startingPoint.Y)) - (Math.Abs(y.X - startingPoint.X) - Math.Abs(y.Y - startingPoint.Y)));
        return sequence;
    }
}