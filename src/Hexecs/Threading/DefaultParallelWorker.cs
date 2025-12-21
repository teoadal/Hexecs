namespace Hexecs.Threading;

public sealed class DefaultParallelWorker : IParallelWorker
{
    public int DegreeOfParallelism { get; }

    private readonly Barrier _barrier;
    private readonly Thread[] _workers;

    private IParallelJob? _job;
    private volatile bool _disposed;

    public DefaultParallelWorker(
        int degreeOfParallelism,
        ThreadPriority priority = ThreadPriority.AboveNormal)
    {
        if (degreeOfParallelism < 2) ThreadingError.WrongDegreeOfParallelism();

        DegreeOfParallelism = degreeOfParallelism;

        _barrier = new Barrier(participantCount: degreeOfParallelism + 1); // +1 — основной поток
        _workers = new Thread[degreeOfParallelism];

        for (var i = 0; i < degreeOfParallelism; i++)
        {
            var workerIndex = i;
            var thread = new Thread(() => ExecuteWorker(workerIndex))
            {
                IsBackground = false,
                Priority = priority,
                Name = $"ParallelRunner {workerIndex} of {degreeOfParallelism}"
            };

            _workers[i] = thread;
            thread.Start();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Сигналим барьер, чтобы разбудить воркеров — они увидят _disposed и выйдут
        try
        {
            _barrier.SignalAndWait();
        }
        catch (ObjectDisposedException)
        {
            // Уже disposed
        }

        // Ждём завершения воркеров
        foreach (var thread in _workers)
        {
            thread.Join();
        }

        ArrayUtils.Clear(_workers);
        _barrier.Dispose();
    }

    public void Run(IParallelJob job)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(DefaultParallelWorker));

        _job = job;

        // Фаза старта: отпускаем всех воркеров
        _barrier.SignalAndWait();

        // Фаза завершения: ждём всех воркеров
        _barrier.SignalAndWait();

        _job = null;
    }

    private void ExecuteWorker(int workerIndex)
    {
        var lastWorkerIndex = _workers.Length - 1;

        try
        {
            while (true)
            {
                // Ждём старт сигнала от основного потока
                _barrier.SignalAndWait();

                if (_disposed) return;

                _job?.Execute(workerIndex, lastWorkerIndex);

                // Сообщаем о завершении фазы
                _barrier.SignalAndWait();
            }
        }
        catch (ObjectDisposedException)
        {
            // Барьер уже утилизирован — нормальный выход
        }
    }
}