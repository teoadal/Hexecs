using Hexecs.Benchmarks.Map.Common;
using Hexecs.Benchmarks.Map.Common.Visibles;
using Hexecs.Benchmarks.Map.Terrains;
using Hexecs.Benchmarks.Map.Terrains.Commands.Generate;
using Hexecs.Benchmarks.Map.Utils;
using Hexecs.Worlds;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hexecs.Benchmarks.Map;

internal sealed class CityGame : Game
{
    private BenchmarkCounter _benchmarkCounter = null!;
    private Camera _camera = null!;
    private readonly GraphicsDeviceManager _graphics;
    private World _world = null!;

    public CityGame()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720,
            GraphicsProfile = GraphicsProfile.HiDef,
            PreferMultiSampling = true,
            SynchronizeWithVerticalRetrace = true,
            IsFullScreen = false,
            HardwareModeSwitch = false
        };

        // Включаем поддержку сглаживания для устройства
        _graphics.PreparingDeviceSettings += (_, e) =>
        {
            e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8; // 8x MSAA
        };

        _graphics.ApplyChanges();

        IsFixedTimeStep = false;
        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;

        _camera = new Camera(GraphicsDevice);
        _world = new WorldBuilder()
            .UseDefaultParallelWorker(Math.Min(6, Environment.ProcessorCount))
            .UseSingleton(Content)
            .UseSingleton(GraphicsDevice)
            .UseSingleton(_camera)
            .UseTerrain()
            .UseDefaultActorContext(context => context
                .Capacity(3_000_000)
                .AddCommon()
                .AddTerrain()
                .AddVisible())
            .Build();

        _world.Actors.Execute(new GenerateTerrainCommand());

        _benchmarkCounter = new BenchmarkCounter(() => _world.Actors.Length, Content, GraphicsDevice);

        base.Initialize();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _world.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.White);

        _world.Draw(gameTime.ElapsedGameTime, gameTime.TotalGameTime);
        _benchmarkCounter.Draw(gameTime);

        base.Draw(gameTime);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.Space))
        {
        }

        _camera.Update(gameTime);
        _benchmarkCounter.Update(gameTime);
        _world.Update(gameTime.ElapsedGameTime, gameTime.TotalGameTime);

        base.Update(gameTime);
    }
}