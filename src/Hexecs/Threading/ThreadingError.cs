namespace Hexecs.Threading;

internal static class ThreadingError
{
    [DoesNotReturn]
    public static void WrongDegreeOfParallelism()
    {
        throw new ArgumentException("Degree of parallelism must be greater or equal than two");
    }
}