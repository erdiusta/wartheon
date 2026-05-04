public class WartheonRNG
{
    private System.Random rng;

    public WartheonRNG(int seed)
    {
        rng = new System.Random(seed);
    }

    public int Range(int min, int max)
    {
        return rng.Next(min, max);
    }

    public float Range(float min, float max)
    {
        return (float)(rng.NextDouble() * (max - min) + min);
    }
}
