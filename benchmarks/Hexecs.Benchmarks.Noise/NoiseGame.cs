using Hexecs.Benchmarks.Noise.Components;
using Hexecs.Benchmarks.Noise.Systems;

using Hexecsm;
using Hexecsm.Worlds;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hexecs.Benchmarks.Noise;

internal sealed class NoiseGame : Game
{
    private BenchmarkCounter _benchmarkCounter = null!;
    private readonly GraphicsDeviceManager _graphics;
    private readonly Random _random = new Random();
    private World _world = null!;

    private const int InitialEntityCount = 2_000_000;
    private const int MaxEntityCount = 3_000_000;

    public NoiseGame()
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
            .WithInitialCapacity(InitialEntityCount)
            .ConfigureComponent<CircleColor>(color => color.WithInitialCapacity(MaxEntityCount))
            .ConfigureComponent<Position>(position => position.WithInitialCapacity(MaxEntityCount))
            .ConfigureComponent<Velocity>(velocity => velocity.WithInitialCapacity(MaxEntityCount))
            .AddUpdateSystem(ctx => new MovementSystem(ctx, width, height))
            .AddDrawSystem(ctx => new RenderSystem(ctx, GraphicsDevice, MaxEntityCount * 2))
            .Build();

        _benchmarkCounter = new BenchmarkCounter(static ctx => ctx.Length, _world, Content, GraphicsDevice);

        for (var i = 0; i < InitialEntityCount; i++)
        {
            SpawnEntity();

            if (i % 5_000 == 0)
            {
                _world.Update(); // avoid too big internal buffers
            }
        }

        base.Initialize();
    }

    private void SpawnEntity(CircleColor? color = null)
    {
        ActorId actor = _world.CreateActor();

        _world.AddComponent(
            actor,
            Position.Create(
                x: _graphics.PreferredBackBufferWidth / 2,
                y: _graphics.PreferredBackBufferHeight / 2));

        _world.AddComponent(
            actor,
            Velocity.Create(
                x: (float)(_random.NextDouble() * 200 - 100),
                y: (float)(_random.NextDouble() * 200 - 100)));

        _world.AddComponent(actor, color ?? CircleColor.CreateRgba(_random));
    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();

        if (keyboard.IsKeyDown(Keys.Space))
        {
            int count = _world.Length;
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
        GraphicsDevice.Clear(Color.White);

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
