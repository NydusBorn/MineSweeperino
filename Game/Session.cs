using System.Drawing;

namespace Game;

public class Session
{
    public Field PlayField;
    public Player Actor = new Player();
    public Solver AutoActor = new Solver();
    public int MineCount { get; private set; }
    public static Session GenerateFromPercentage(int width, int height, double percentage, bool guaranteedSolution, Point startingPoint)
    {
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
        return result;
    }
    public static Session GenerateFromCount(int width, int height, int mineCount, bool guaranteedSolution, Point startingPoint)
    {
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