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
    public Field? PlayField;
    /// <summary>
    /// Player actions
    /// </summary>
    public Player? Actor;
    /// <summary>
    /// Solver for the game
    /// </summary>
    public Solver? AutoActor;
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
        var pattern = result.GetSpiralPattern(startingPoint);
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
        var pattern = result.GetSpiralPattern(startingPoint);
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
        return result;
    }

    /// <summary>
    /// Creates a sequence of points that forms a spiral, the sequence excludes the starter area (usually a 3x3 square).
    /// </summary>
    /// <param name="startingPoint"> Where to start the spiral</param>
    /// <returns>Spiral pattern with center at startingPoint</returns>
    private List<Point> GetSpiralPattern(Point startingPoint)
    {
        List<Point> sequence = new List<Point>();
        sequence.Add(startingPoint);
        int x = startingPoint.X;
        int y = startingPoint.Y;
        int boundLeft = x - 2 >= 0 ? x - 2 : 0;
        int boundRight = x + 2 < PlayField.Width ? x + 2 : PlayField.Width;
        int boundUp = y - 2 >= 0 ? y - 2 : 0;
        int boundDown = y + 2 < PlayField.Height ? y + 2 : PlayField.Height;
        bool skipLeft = x - 1 <= 0;
        bool skipRight = x + 1 >= PlayField.Width - 1;
        bool skipUp = y - 1 <= 0;
        bool skipDown = y + 1 >= PlayField.Height - 1;
        x = boundRight;
        y = boundDown - 1;
        while (true)
        {
            if (skipRight) y = boundUp;
            for (; y > boundUp; y--) sequence.Add(new Point(x, y));
            if (!skipRight)
            {
                boundRight += 1;
                if (boundRight == PlayField.Width)
                {
                    skipRight = true;
                    boundRight = PlayField.Width - 1;
                }
            }

            if (skipUp) x = boundLeft;
            for (; x > boundLeft; x--) sequence.Add(new Point(x, y));
            if (!skipUp)
            {
                boundUp += 1;
                if (boundUp == PlayField.Height)
                {
                    skipUp = true;
                    boundUp = PlayField.Height - 1;
                }
            }

            if (skipLeft) y = boundDown;
            for (; y < boundDown; y++) sequence.Add(new Point(x, y));
            if (!skipLeft)
            {
                boundLeft += 1;
                if (boundLeft == PlayField.Width)
                {
                    skipLeft = true;
                    boundLeft = PlayField.Width - 1;
                }
            }

            if (skipDown) x = boundRight;
            for (; x < boundRight; x++) sequence.Add(new Point(x, y));
            if (!skipDown)
            {
                boundDown -= 1;
                if (boundDown == PlayField.Height)
                {
                    skipDown = true;
                    boundDown = PlayField.Height - 1;
                }
            }

            if (skipDown && skipLeft && skipRight && skipUp) break;
        }
        return sequence;
    }
}