using Hexecs.Actors;
using Hexecs.Benchmarks.Spawn.Components;
using Hexecs.Benchmarks.Spawn.Systems;
using Hexecs.Dependencies;
using Hexecs.Threading;
using Hexecs.Worlds;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hexecs.Benchmarks.Spawn;

internal sealed class SpawnGame : Game
{
    private BenchmarkCounter _benchmarkCounter = null!;
    private ActorContext _context = null!;
    private readonly GraphicsDeviceManager _graphics;
    private World _world = null!;

    private const int MaxEntityCount = 3_000_000;

    public SpawnGame()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720,
            GraphicsProfile = GraphicsProfile.HiDef,
            PreferMultiSampling = true,
            SynchronizeWithVerticalRetrace = true,
            IsFullScreen = true,
            HardwareModeSwitch = false
        };

        // Включаем поддержку сглаживания для устройства
        _graphics.PreparingDeviceSettings += (_, e) =>
        {
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8; // 8x MSAA
        };

        IsFixedTimeStep = true;
        Content.RootDirectory = "Content";

        _graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;

        int width = _graphics.PreferredBackBufferWidth;
        int height = _graphics.PreferredBackBufferHeight;

        _world = new WorldBuilder()
            .UseDefaultParallelWorker(Math.Min(6, Environment.ProcessorCount))
            .UseDefaultActorContext(builder => builder
                .Capacity(MaxEntityCount)
                .ConfigureComponentPool<CircleColor>(static color => color.Capacity(MaxEntityCount))
                .ConfigureComponentPool<Lifetime>(static lifetime => lifetime.Capacity(MaxEntityCount))
                .ConfigureComponentPool<Position>(static position => position.Capacity(MaxEntityCount))
                .ConfigureComponentPool<Velocity>(static velocity => velocity.Capacity(MaxEntityCount))
                .CreateUpdateSystem(ctx => new MovementSystem(ctx, ctx.GetRequiredService<IParallelWorker>(), width, height))
                .CreateUpdateSystem<LifetimeSystem>()
                .CreateUpdateSystem<DestroySystem>()
                .CreateUpdateSystem(ctx => new SpawnSystem(ctx, ctx.GetRequiredService<Dice>(), width, height))
                .CreateDrawSystem(ctx => new RenderSystem(ctx, GraphicsDevice, MaxEntityCount * 2)))
            .Build();

        _context = _world.Actors;
        _benchmarkCounter = new BenchmarkCounter(static ctx => ctx.Length, _context, Content, GraphicsDevice);

        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();

        if (keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        _benchmarkCounter.Update(gameTime);
        _world.Update(gameTime.ElapsedGameTime, gameTime.TotalGameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _world.Draw(gameTime.ElapsedGameTime, gameTime.TotalGameTime);
        _benchmarkCounter.Draw(gameTime);

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
