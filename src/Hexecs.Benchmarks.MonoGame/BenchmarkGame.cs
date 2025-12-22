using Hexecs.Actors;
using Hexecs.Benchmarks.MonoGame.Components;
using Hexecs.Benchmarks.MonoGame.Systems;
using Hexecs.Dependencies;
using Hexecs.Threading;
using Hexecs.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hexecs.Benchmarks.MonoGame;

public class BenchmarkGame : Game
{
    private ActorFilter<Position>? _entitiesCountFilter;
    private readonly GraphicsDeviceManager _graphics;
    private World _world = null!;
    private ActorContext _context = null!;
    private readonly Random _random = new();

    // Поля для статистики
    private double _frameTime;
    private int _fps;
    private int _frameCount;
    private double _fpsTimer;
    private int _secondsCounter;

    // Для среднего значения за минуту (Rolling Average)
    private readonly int[] _fpsHistory = new int[60];
    private int _historyIndex;
    private bool _historyFull;
    private double _avgFps;

    private const int InitialEntityCount = 2_000_000;
    private const int MaxEntityCount = 3_000_000;

    public BenchmarkGame()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720,
            GraphicsProfile = GraphicsProfile.HiDef, // Используем профиль HiDef для поддержки расширенных возможностей
            PreferMultiSampling = true,
            SynchronizeWithVerticalRetrace = false,
            IsFullScreen = false,
            HardwareModeSwitch = false // Используем borderless fullscreen для удобства
        };

        // Включаем поддержку сглаживания для устройства
        _graphics.PreparingDeviceSettings += (_, e) =>
        {
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8; // 8x MSAA
        };

        _graphics.ApplyChanges();

        IsFixedTimeStep = false;
    }

    protected override void Initialize()
    {
        GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;

        var width = _graphics.PreferredBackBufferWidth;
        var height = _graphics.PreferredBackBufferHeight;

        _world = new WorldBuilder()
            .DefaultParallelWorker(Math.Min(6, Environment.ProcessorCount))
            .DefaultActorContext(builder => builder
                .Capacity(InitialEntityCount)
                .ConfigureComponentPool<CircleColor>(color => color.Capacity(InitialEntityCount))
                .ConfigureComponentPool<Position>(position => position.Capacity(InitialEntityCount))
                .ConfigureComponentPool<Velocity>(velocity => velocity.Capacity(InitialEntityCount))
                .CreateUpdateSystem(ctx =>
                    new MovementSystem(ctx, ctx.GetRequiredService<IParallelWorker>(), width, height))
                .CreateDrawSystem(ctx => new RenderSystem(ctx, GraphicsDevice, MaxEntityCount * 2)))
            .Build();

        _context = _world.Actors;
        _entitiesCountFilter = _context.Filter<Position>();

        for (var i = 0; i < InitialEntityCount; i++)
        {
            SpawnEntity();
        }

        base.Initialize();
    }


    private void SpawnEntity(CircleColor? color = null)
    {
        var actor = _context.CreateActor();
        actor.Add(Position.Create(
            x: _graphics.PreferredBackBufferWidth / 2, 
            y: _graphics.PreferredBackBufferHeight / 2));
        
        actor.Add(Velocity.Create(
            x: (float)(_random.NextDouble() * 200 - 100),
            y: (float)(_random.NextDouble() * 200 - 100)));
        
        actor.Add(color ?? CircleColor.CreateRgba(_random));
    }

    protected override void Update(GameTime gameTime)
    {
        var count = _entitiesCountFilter?.Length ?? 0;

        var keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.Space))
        {
            var color = CircleColor.CreateRgba(_random);
            for (var i = 0; i < 50; i++)
            {
                if (count >= MaxEntityCount)
                {
                    break;
                }

                SpawnEntity(color);
            }
        }

        _world.Update(gameTime.ElapsedGameTime, gameTime.TotalGameTime);

        // Сбор статистики
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
                count = _entitiesCountFilter?.Length ?? 0;
                Window.Title =
                    $"FPS: {_fps} | Avg FPS: {_avgFps:F1} | Entities: {count:N0} | Frame Time: {_frameTime:F2}ms | Alloc: {alloc:F2}Mb";

                _secondsCounter = 0;
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);

        _world.Draw(gameTime.ElapsedGameTime, gameTime.TotalGameTime);

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _world.Dispose();
        }

        base.Dispose(disposing);
    }
}