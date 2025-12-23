using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hexecs.Benchmarks.Map.Utils;

internal sealed class Camera2D
{
    private readonly GraphicsDevice _graphicsDevice;

    /// <summary>
    /// Позиция камеры в мировых координатах (центр экрана).
    /// </summary>
    public Vector2 Position { get; private set; }

    /// <summary>
    /// Текущий масштаб камеры (1.0 = без изменений).
    /// </summary>
    public float Zoom { get; private set; }

    private int _previousScrollValue;

    public Camera2D(GraphicsDevice graphicsDevice)
    {
        Position = Vector2.Zero;
        Zoom = 1f;

        _graphicsDevice = graphicsDevice;
    }

    /// <summary>
    /// Изменяет текущий зум на множитель и ограничивает его допустимым диапазоном.
    /// </summary>
    /// <param name="factor">Множитель масштаба (больше 1 для приближения, меньше 1 для отдаления).</param>
    public void AdjustZoom(float factor)
    {
        Zoom *= factor;
        Zoom = MathHelper.Clamp(Zoom, 0.1f, 10f);
    }

    /// <summary>
    /// Создает матрицу трансформации для SpriteBatch, учитывая позицию, зум и размер экрана.
    /// </summary>
    public Matrix GetTransformationMatrix()
    {
        var viewport = _graphicsDevice.Viewport;

        return Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
               Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
               Matrix.CreateTranslation(new Vector3(viewport.Width * 0.5f, viewport.Height * 0.5f, 0));
    }

    /// <summary>
    /// Смещает камеру на указанный вектор.
    /// </summary>
    public void Move(Vector2 direction) => Position += direction;

    /// <summary>
    /// Обрабатывает ввод игрока для перемещения и масштабирования камеры.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Базовое управление камерой
        var speed = 500f / Zoom;
        var moveDir = Vector2.Zero;

        if (keyboard.IsKeyDown(Keys.W)) moveDir.Y -= 1;
        if (keyboard.IsKeyDown(Keys.S)) moveDir.Y += 1;
        if (keyboard.IsKeyDown(Keys.A)) moveDir.X -= 1;
        if (keyboard.IsKeyDown(Keys.D)) moveDir.X += 1;

        if (moveDir != Vector2.Zero)
        {
            moveDir.Normalize();
            Move(moveDir * speed * dt);
        }

        if (keyboard.IsKeyDown(Keys.Q)) Zoom += dt * Zoom;
        if (keyboard.IsKeyDown(Keys.E)) Zoom -= dt * Zoom;

        var scrollDelta = mouse.ScrollWheelValue - _previousScrollValue;
        if (scrollDelta != 0)
        {
            AdjustZoom(scrollDelta > 0 ? 1.1f : 0.9f);
        }

        _previousScrollValue = mouse.ScrollWheelValue;

        Zoom = MathHelper.Clamp(Zoom, 0.1f, 10f);
    }

    /// <summary>
    /// Переводит экранные координаты (например, позицию мыши) в мировые координаты игрового поля.
    /// </summary>
    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        return Vector2.Transform(screenPosition, Matrix.Invert(GetTransformationMatrix()));
    }

    /// <summary>
    /// Переводит мировые координаты в координаты экрана (например, для отрисовки UI над объектами).
    /// </summary>
    public Vector2 WorldToScreen(Vector2 worldPosition)
    {
        return Vector2.Transform(worldPosition, GetTransformationMatrix());
    }
}