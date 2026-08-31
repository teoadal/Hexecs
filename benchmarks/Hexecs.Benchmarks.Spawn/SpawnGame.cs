using Hexecs.Benchmarks.Spawn.Systems;

                                 using Hexecsm.Worlds;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hexecs.Benchmarks.Spawn;

internal sealed class SpawnGame : Game
{
    private BenchmarkCounter _benchmarkCounter = null!;
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
            SynchronizeWithVerticalRetrace = false,
            IsFullScreen = false,
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
            .WithDegreeOfParallelism(Math.Min(6, Environment.ProcessorCount))
            .AddUpdateSystem(ctx => new MovementSystem(ctx, width, height))
            .AddUpdateSystem(ctx => new LifetimeSystem(ctx))
            .AddUpdateSystem(ctx => new DestroySystem(ctx))
            .AddUpdateSystem(ctx => new SpawnSystem(ctx, width, height))
            .AddDrawSystem(ctx => new RenderSystem(ctx, GraphicsDevice, MaxEntityCount * 2))
            .Build();

        _benchmarkCounter = new BenchmarkCounter(static ctx => ctx.Length, _world, Content, GraphicsDevice);

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
