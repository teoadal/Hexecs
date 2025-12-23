using Microsoft.Xna.Framework;

namespace Hexecs.Benchmarks.Noise;

internal sealed class FpsCounter
{
    private readonly Func<int> _countResolver;
    private readonly int[] _fpsHistory;
    private readonly GameWindow _window;

    // Поля для статистики
    private double _frameTime;
    private int _fps;
    private int _frameCount;
    private double _fpsTimer;
    private int _secondsCounter;

    // Для среднего значения за минуту (Rolling Average)
    private int _historyIndex;
    private bool _historyFull;
    private double _avgFps;

    public FpsCounter(Func<int> countResolver, GameWindow window)
    {
        _countResolver = countResolver;
        _window = window;
        _fpsHistory = new int[60];
    }

    public void Draw(GameTime gameTime)
    {
        var elapsedSeconds = gameTime.ElapsedGameTime.TotalSeconds;
        _frameTime = gameTime.ElapsedGameTime.TotalMilliseconds;
        _fpsTimer += elapsedSeconds;
        _frameCount++;

        // Считаем FPS каждую секунду для точности истории
        if (_fpsTimer >= 1.0)
        {
            _fps = _frameCount;

            // Обновляем историю для Avg
            _fpsHistory[_historyIndex] = _fps;
            _historyIndex = (_historyIndex + 1) % 60;
            if (_historyIndex == 0) _historyFull = true;

            // Считаем среднее за минуту
            var historyCount = _historyFull ? 60 : _historyIndex;
            var sum = 0;
            for (var i = 0; i < historyCount; i++) sum += _fpsHistory[i];
            _avgFps = (double)sum / historyCount;

            _frameCount = 0;
            _fpsTimer -= 1.0;
            _secondsCounter++;

            if (_secondsCounter >= 1)
            {
                var alloc = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
                var count = _countResolver();
                _window.Title = $"FPS: {_fps} | Avg FPS: {_avgFps:F1} | Entities: {count:N0} | Frame Time: {_frameTime:F2}ms | Alloc: {alloc:F2}Mb";

                _secondsCounter = 0;
            }
        }
    }
}