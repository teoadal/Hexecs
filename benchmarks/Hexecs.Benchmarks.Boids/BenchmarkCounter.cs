using System.Globalization;
using System.Text;

using Hexecs.Actors;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Hexecs.Benchmarks.Boids;

internal sealed class BenchmarkCounter
{
    private readonly Func<ActorContext, int> _countResolver;
    private readonly ActorContext _context;
    private readonly int[] _fpsHistory;

    private double _frameTime;
    private double _frameTimeSum;
    private int _fps;
    private int _frameCount;
    private double _fpsTimer;

    private int _historyIndex;
    private bool _historyFull;
    private double _avgFps;
    private long _historySum;

    private readonly SpriteFont _font;
    private readonly SpriteBatch _spriteBatch;

    private readonly StringBuilder _stringBuilder = new StringBuilder(128);
    private readonly Vector2 _textPos = new Vector2(10, 10);
    private readonly Vector2 _shadowPos = new Vector2(11, 11);

    public BenchmarkCounter(
        Func<ActorContext, int> countResolver,
        ActorContext context,
        ContentManager contentManager,
        GraphicsDevice graphicsDevice)
    {
        _countResolver = countResolver;
        _context = context;
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
        TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
        double elapsedSeconds = elapsedGameTime.TotalSeconds;

        // Накапливаем время всех кадров для честного подсчета среднего за секунду
        _frameTimeSum += elapsedGameTime.TotalMilliseconds;
        _fpsTimer += elapsedSeconds;

        if (_fpsTimer < 1.0)
        {
            return;
        }

        _fps = _frameCount;

        // Обновление скользящего окна FPS
        ref int historySlot = ref _fpsHistory[_historyIndex];
        _historySum -= historySlot;
        historySlot = _fps;
        _historySum += _fps;

        _historyIndex++;

        if (_historyIndex >= 60)
        {
            _historyIndex = 0;
            _historyFull = true;
        }

        int historyCount = _historyFull ? 60 : _historyIndex;
        _avgFps = historyCount > 0 ? (double)_historySum / historyCount : _fps;

        // Расчет среднего времени кадра за прошедшую секунду
        _frameTime = _frameTimeSum / _frameCount;

        double alloc = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
        int count = _countResolver(_context);

        CultureInfo culture = CultureInfo.InvariantCulture;

        _stringBuilder.Clear();
        _stringBuilder.Append(culture, $"{_fps} FPS | Avg:{_avgFps:F1} fps | Entities:{count:N0} | Frame time:{_frameTime:F1}ms | Alloc:{alloc:F3}mb");

        _frameCount = 0;
        _fpsTimer = 0;
        _frameTimeSum = 0;
    }
}
