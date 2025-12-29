using System.Globalization;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Hexecs.Benchmarks.Map;

internal sealed class BenchmarkCounter
{
    private readonly Func<int> _countResolver;
    private readonly int[] _fpsHistory;

    private double _frameTime;
    private int _fps;
    private int _frameCount;
    private double _fpsTimer;

    private int _historyIndex;
    private bool _historyFull;
    private double _avgFps;
    private long _historySum;

    private readonly SpriteFont _font;
    private readonly SpriteBatch _spriteBatch;

    // Используем StringBuilder как буфер
    private readonly StringBuilder _stringBuilder = new(128);
    private readonly Vector2 _textPos = new(10, 10);
    private readonly Vector2 _shadowPos = new(11, 11);

    public BenchmarkCounter(Func<int> countResolver, ContentManager contentManager, GraphicsDevice graphicsDevice)
    {
        _countResolver = countResolver;
        _fpsHistory = new int[60];
        _font = contentManager.Load<SpriteFont>("DebugFont");
        _spriteBatch = new SpriteBatch(graphicsDevice);
    }

    public void Draw(GameTime gameTime)
    {
        _frameCount++;

        _spriteBatch.Begin();

        _spriteBatch.DrawString(_font, _stringBuilder, _shadowPos, Color.Black);
        _spriteBatch.DrawString(_font, _stringBuilder, _textPos, Color.Yellow);

        _spriteBatch.End();
    }

    public void Update(GameTime gameTime)
    {
        var elapsedSeconds = gameTime.ElapsedGameTime.TotalSeconds;
        _frameTime = gameTime.ElapsedGameTime.TotalMilliseconds;
        _fpsTimer += elapsedSeconds;

        if (_fpsTimer >= 1.0)
        {
            _fps = _frameCount;

            _historySum -= _fpsHistory[_historyIndex];
            _fpsHistory[_historyIndex] = _fps;
            _historySum += _fps;

            _historyIndex = (_historyIndex + 1) % 60;
            if (_historyIndex == 0) _historyFull = true;

            var historyCount = _historyFull ? 60 : _historyIndex;
            _avgFps = (double)_historySum / historyCount;

            var alloc = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
            var count = _countResolver();

            // Очищаем буфер и записываем новые данные без создания строк
            var culture = CultureInfo.InvariantCulture;

            _stringBuilder.Clear();
            _stringBuilder
                .Append($"{_fps} FPS")
                .Append(culture, $" | Avg:{_avgFps:F1} fps")
                .Append(culture, $" | Entities:{count:N0}")
                .Append(culture, $" | Frame time:{_frameTime:F1}ms")
                .Append(culture, $" | Alloc:{alloc:F3}mb");

            _frameCount = 0;
            _fpsTimer = 0;
        }
    }
}