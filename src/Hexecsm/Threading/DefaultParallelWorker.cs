namespace Hexecsm.Threading;

public sealed class DefaultParallelWorker : IParallelWorker
{
    public int DegreeOfParallelism
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _degreeOfParallelism;
    }

    public bool Started
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _started;
    }

    private readonly Barrier _barrier;
    private readonly CancellationTokenSource _cts;
    private readonly CancellationToken _cancellationToken;
    private readonly int _degreeOfParallelism;
    private readonly ThreadPriority _threadPriority;
    private readonly Thread?[] _workers;

    private IParallelJob? _job;
    private volatile bool _disposed;
    private bool _started;

    public DefaultParallelWorker(
        int degreeOfParallelism,
        ThreadPriority priority = ThreadPriority.AboveNormal)
    {
        if (degreeOfParallelism < 2)
        {
            throw new ArgumentOutOfRangeException(
                paramName: nameof(degreeOfParallelism),
                message: "Degree of parallelism must be at least 2.");
        }

        _barrier = new Barrier(participantCount: degreeOfParallelism + 1);
        _workers = new Thread?[degreeOfParallelism];

        _cts = new CancellationTokenSource();
        _cancellationToken = _cts.Token;
        _degreeOfParallelism = degreeOfParallelism;
        _threadPriority = priority;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        // Отменяем токен — это заставит методы SignalAndWait выкинуть OperationCanceledException
        // и мгновенно разблокирует все потоки, где бы они ни находились
        _cts.Cancel();

        // Теперь спокойно дожидаемся завершения потоков
        foreach (Thread? thread in _workers)
        {
            if (thread is { IsAlive: true })
            {
                thread.Join();
            }
        }

        _barrier.Dispose();
        _cts.Dispose();
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

    public void Start()
    {
        if (_started)
        {
            return;
        }

        _started = true;

        // +1 для управляющего потока

        // Используем countdown, чтобы главный поток не вышел из конструктора,
        // пока все воркеры гарантированно не дойдут до первого барьера.
        var startupLatch = new CountdownEvent(_degreeOfParallelism);

        for (var i = 0; i < _degreeOfParallelism; i++)
        {
            int workerIndex = i;
            var thread = new Thread(() => ExecuteWorker(workerIndex, startupLatch))
            {
                IsBackground = false, // Потоки foreground, удерживают приложение
                Priority = _threadPriority,
                Name = $"ParallelRunner {workerIndex} of {_degreeOfParallelism}"
            };

            _workers[i] = thread;
            thread.Start();
        }

        // Ждем, пока все потоки инициализируются и встанут у барьера
        startupLatch.Wait(_cancellationToken);
        startupLatch.Dispose();
    }

    private void ExecuteWorker(int workerIndex, CountdownEvent startupLatch)
    {
        int workersCount = _workers.Length;

        // Сигнализируем конструктору, что данный поток готов
        startupLatch.Signal();

        try
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                // Точка ожидания 1: Ждем команду "Старт" от метода Run
                _barrier.SignalAndWait(_cancellationToken);

                // Выполняем работу (проверяем job на null на случай отмены)
                _job?.Execute(workerIndex, workersCount);

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
}
