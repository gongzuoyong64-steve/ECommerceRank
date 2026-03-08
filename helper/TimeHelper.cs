


public class TimeHelper
{
    private static readonly Random _random = new Random();

    public static int GenerateJitter()
    {
        return _random.Next(1, 6);
    }
}


