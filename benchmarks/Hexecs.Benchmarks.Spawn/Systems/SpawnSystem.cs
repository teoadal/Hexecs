using Hexecs.Benchmarks.Spawn.Components;

using Hexecsm;
using Hexecsm.Systems;
using Hexecsm.Utils;
using Hexecsm.Worlds;

using Microsoft.Xna.Framework.Input;

namespace Hexecs.Benchmarks.Spawn.Systems;

internal sealed class SpawnSystem(
    World context,
    int screenWidth,
    int screenHeight) : IUpdateSystem
{
    private const int SpawnRatePerFrame = 2500;

    public bool Enabled { get; set; } = true;

    private readonly Dice _dice = context.Dice;
    private readonly Position _spawnPosition = Position.Create(screenWidth / 2, screenHeight);
    private KeyboardState _previousKeyboardState = Keyboard.GetState();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SkipLocalsInit]
    public void Update(in WorldTime time)
    {
        // 1. Получаем состояние клавиатуры в ТЕКУЩЕМ кадре
        KeyboardState currentKeyboardState = Keyboard.GetState();

        // 2. ПРОВЕРКА НА ВЗРЫВ (Клик): Нажата СЕЙЧАС, но была отпущена РАНЬШЕ
        if (currentKeyboardState.IsKeyDown(Keys.Space) && _previousKeyboardState.IsKeyUp(Keys.Space))
        {
            // Спавним огромную пачку частиц, разлетающихся во все стороны по кругу
            for (var i = 0; i < 20000; i++)
            {
                ActorId actor = context.CreateActor();
                context.AddComponent(actor, in _spawnPosition);

                // Математика кругового взрыва: случайный угол и случайная скорость
                var angle = (float)(_dice.GetNextDouble() * Math.PI * 2);
                var speed = (float)(_dice.GetNextDouble() * 400.0 + 100.0); // скорость от 100 до 500

                float vx = (float)Math.Cos(angle) * speed;
                float vy = (float)Math.Sin(angle) * speed;

                context.AddComponent(actor, Velocity.Create(vx, vy));

                var lifetimeSeconds = (float)(_dice.GetNextDouble() * 2.0 + 2.5);
                context.AddComponent(actor, Lifetime.Create(lifetimeSeconds));
                context.AddComponent(actor, CircleColor.CreateFromVelocity(new Vector2(vx, vy)));
            }
        }

        // 3. ПРОВЕРКА НА УДЕРЖАНИЕ (Поток): Нажата и СЕЙЧАС, и в ПРОШЛОМ кадре
        else if (currentKeyboardState.IsKeyDown(Keys.Space))
        {
            // Обычный спавн стандартного фонтана (ваш текущий код)
            for (var i = 0; i < SpawnRatePerFrame; i++)
            {
                ActorId actor = context.CreateActor();
                context.AddComponent(actor, in _spawnPosition);

                var vx = (float)(_dice.GetNextDouble() * 500.0 - 250.0);
                var vy = (float)(_dice.GetNextDouble() * -600.0 - 150.0);

                context.AddComponent(actor, Velocity.Create(vx, vy));

                var lifetimeSeconds = (float)(_dice.GetNextDouble() * 2.0 + 2.5);
                context.AddComponent(actor, Lifetime.Create(lifetimeSeconds));
                context.AddComponent(actor, CircleColor.CreateFromVelocity(new Vector2(vx, vy)));
            }
        }

        // 4. КРИТИЧЕСКИ ВАЖНО: В самом конце метода сохраняем текущее состояние.
        // В следующем кадре оно станет "предыдущим".
        _previousKeyboardState = currentKeyboardState;
    }
}
