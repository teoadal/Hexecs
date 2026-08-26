namespace Hexecs.Threading;

public sealed class DefaultParallelWorker : IParallelWorker
{
    public int DegreeOfParallelism { get; }

    private readonly Barrier _barrier;
    private readonly Thread[] _workers;
    private readonly CancellationTokenSource _cts;
    private readonly CancellationToken _cancellationToken;

    private IParallelJob? _job;
    private volatile bool _disposed;

    public DefaultParallelWorker(
        int degreeOfParallelism,
        ThreadPriority priority = ThreadPriority.AboveNormal)
    {
        if (degreeOfParallelism < 2)
            throw new ArgumentOutOfRangeException(nameof(degreeOfParallelism),
                "Degree of parallelism must be at least 2.");

        DegreeOfParallelism = degreeOfParallelism;

        _cts = new CancellationTokenSource();
        _cancellationToken = _cts.Token;

        // +1 для управляющего потока
        _barrier = new Barrier(participantCount: degreeOfParallelism + 1);
        _workers = new Thread[degreeOfParallelism];

        // Используем countdown, чтобы главный поток не вышел из конструктора,
        // пока все воркеры гарантированно не дойдут до первого барьера.
        using var startupLatch = new CountdownEvent(degreeOfParallelism);

        for (var i = 0; i < degreeOfParallelism; i++)
        {
            var workerIndex = i;
            var thread = new Thread(() => ExecuteWorker(workerIndex, startupLatch))
            {
                IsBackground = false, // Потоки foreground, удерживают приложение
                Priority = priority,
                Name = $"ParallelRunner {workerIndex} of {degreeOfParallelism}"
            };

            _workers[i] = thread;
            thread.Start();
        }

        // Ждем, пока все потоки инициализируются и встанут у барьера
        startupLatch.Wait();
    }

    public void Run(IParallelJob job)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(DefaultParallelWorker));

        _job = job;

        try
        {
            // Фаза 1: Разрешаем воркерам начать выполнение задачи
            _barrier.SignalAndWait(_cancellationToken);

            // Фаза 2: Ожидаем, пока все воркеры завершат задачу
            _barrier.SignalAndWait(_cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Происходит утилизация воркера во время работы
        }
        finally
        {
            _job = null;
        }
    }

    private void ExecuteWorker(int workerIndex, CountdownEvent startupLatch)
    {
        var lastWorkerIndex = _workers.Length - 1;

        // Сигнализируем конструктору, что данный поток готов
        startupLatch.Signal();

        try
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                // Точка ожидания 1: Ждем команду "Старт" от метода Run
                _barrier.SignalAndWait(_cancellationToken);

                // Выполняем работу (проверяем job на null на случай отмены)
                _job?.Execute(workerIndex, lastWorkerIndex);

                // Точка ожидания 2: Сигнализируем о завершении и ждем остальных воркеров + Главный поток
                _barrier.SignalAndWait(_cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Нормальный выход при отмене через CTS
        }
        catch (ObjectDisposedException)
        {
            // Нормальный выход, если барьер уничтожен
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Отменяем токен — это заставит методы SignalAndWait выкинуть OperationCanceledException
        // и мгновенно разблокирует все потоки, где бы они ни находились
        _cts.Cancel();

        // Теперь спокойно дожидаемся завершения потоков
        foreach (var thread in _workers)
        {
            if (thread.IsAlive)
            {
                thread.Join();
            }
        }

        _barrier.Dispose();
        _cts.Dispose();
    }
}