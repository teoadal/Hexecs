using Hexecs.Actors;
using Hexecs.Actors.Systems;
using Hexecs.Benchmarks.Spawn.Components;
using Hexecs.Worlds;

using Microsoft.Xna.Framework.Input;

namespace Hexecs.Benchmarks.Spawn.Systems;

internal sealed class SpawnSystem : UpdateSystem
{
    private readonly ActorContext _context;
    private readonly Dice _dice;

    private readonly Position _spawnPosition;
    private const int SpawnRatePerFrame = 2500;

    private KeyboardState _previousKeyboardState;

    public SpawnSystem(
        ActorContext context,
        Dice dice,
        int screenWidth,
        int screenHeight) : base(context)
    {
        _context = context;
        _dice = dice;
        _spawnPosition = Position.Create(screenWidth / 2, screenHeight);
        _previousKeyboardState = Keyboard.GetState();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public override void Update(in WorldTime time)
    {
        // 1. Получаем состояние клавиатуры в ТЕКУЩЕМ кадре
        KeyboardState currentKeyboardState = Keyboard.GetState();

        // 2. ПРОВЕРКА НА ВЗРЫВ (Клик): Нажата СЕЙЧАС, но была отпущена РАНЬШЕ
        if (currentKeyboardState.IsKeyDown(Keys.Space) && _previousKeyboardState.IsKeyUp(Keys.Space))
        {
            // Спавним огромную пачку частиц, разлетающихся во все стороны по кругу
            for (var i = 0; i < 20000; i++)
            {
                Actor actor = _context.CreateActor();
                actor.Add(_spawnPosition);

                // Математика кругового взрыва: случайный угол и случайная скорость
                var angle = (float)(_dice.GetNextDouble() * Math.PI * 2);
                var speed = (float)(_dice.GetNextDouble() * 400.0 + 100.0); // скорость от 100 до 500

                float vx = (float)Math.Cos(angle) * speed;
                float vy = (float)Math.Sin(angle) * speed;

                actor.Add(Velocity.Create(vx, vy));

                var lifetimeSeconds = (float)(_dice.GetNextDouble() * 2.0 + 2.5);
                actor.Add(Lifetime.Create(lifetimeSeconds));
                actor.Add(CircleColor.CreateFromVelocity(new Vector2(vx, vy)));
            }
        }

        // 3. ПРОВЕРКА НА УДЕРЖАНИЕ (Поток): Нажата и СЕЙЧАС, и в ПРОШЛОМ кадре
        else if (currentKeyboardState.IsKeyDown(Keys.Space))
        {
            // Обычный спавн стандартного фонтана (ваш текущий код)
            for (var i = 0; i < SpawnRatePerFrame; i++)
            {
                Actor actor = _context.CreateActor();
                actor.Add(_spawnPosition);

                var vx = (float)(_dice.GetNextDouble() * 500.0 - 250.0);
                var vy = (float)(_dice.GetNextDouble() * -600.0 - 150.0);

                actor.Add(Velocity.Create(vx, vy));

                var lifetimeSeconds = (float)(_dice.GetNextDouble() * 2.0 + 2.5);
                actor.Add(Lifetime.Create(lifetimeSeconds));
                actor.Add(CircleColor.CreateFromVelocity(new Vector2(vx, vy)));
            }
        }

        // 4. КРИТИЧЕСКИ ВАЖНО: В самом конце метода сохраняем текущее состояние.
        // В следующем кадре оно станет "предыдущим".
        _previousKeyboardState = currentKeyboardState;
    }
}
