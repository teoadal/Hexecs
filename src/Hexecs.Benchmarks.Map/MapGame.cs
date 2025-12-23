using Hexecs.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hexecs.Benchmarks.Map;

internal sealed class MapGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private World _world = null!;

    public MapGame()
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

        _world = new WorldBuilder()
            .Singleton<GraphicsDevice>(_ => GraphicsDevice)
            .DefaultParallelWorker(Math.Min(6, Environment.ProcessorCount))
            .DefaultActorContext(context => context
                .Capacity(500_000))
            .Build();


        base.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.Space))
        {
        }

        _world.Update(gameTime.ElapsedGameTime, gameTime.TotalGameTime);

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