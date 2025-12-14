public static class UpdateTestWork
{
    public static int DoWork(int v)
    {
        v ^= v << 13;
        v ^= v >> 17;
        v ^= v << 5;
        return v;
    }
}