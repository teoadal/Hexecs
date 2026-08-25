using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hexecs.Benchmarks.Map.Utils;

internal sealed class Camera(GraphicsDevice graphicsDevice)
{
    /// <summary>
    /// Позиция камеры в мировых координатах (центр экрана).
    /// </summary>
    public ref readonly Vector2 Position
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _currentPosition;
    }

    /// <summary>
    /// Матрица трансформации, учитывая позицию, зум и размер экрана.
    /// </summary>
    public ref readonly Matrix TransformationMatrix
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _currentTransform;
    }

    /// <summary>
    /// Viewport of world boundary
    /// </summary>
    public ref readonly CameraViewport Viewport
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref _currentViewport;
    }

    /// <summary>
    /// Текущий масштаб камеры (1.0 = без изменений).
    /// </summary>
    public float Zoom
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _currentZoom;
    }

    private Vector2 _currentPosition;
    private Matrix _currentTransform;
    private CameraViewport _currentViewport;
    private float _currentZoom = 1f;

    private Vector2 _previousPosition;
    private float _previousZoom;
    private int _previousScrollValue;

    /// <summary>
    /// Изменяет текущий зум на множитель и ограничивает его допустимым диапазоном.
    /// </summary>
    /// <param name="factor">Множитель масштаба (больше 1 для приближения, меньше 1 для отдаления).</param>
    public void AdjustZoom(float factor)
    {
        if (factor > 0) _currentZoom += 1f;
        else _currentZoom -= 1f;

        _currentZoom = MathHelper.Clamp(_currentZoom, 1f, 10f);
    }

    /// <summary>
    /// Смещает камеру на указанный вектор.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Move(Vector2 direction) => _currentPosition += direction;

    /// <summary>
    /// Обрабатывает ввод игрока для перемещения и масштабирования камеры.
    /// </summary>
    public void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Базовое управление камерой
        var speed = 500f / _currentZoom;
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

        var scrollDelta = mouse.ScrollWheelValue - _previousScrollValue;
        if (scrollDelta != 0)
        {
            AdjustZoom(scrollDelta);
        }

        _previousScrollValue = mouse.ScrollWheelValue;

        if (_currentPosition != _previousPosition || Math.Abs(_currentZoom - _previousZoom) > float.Epsilon)
        {
            UpdateTransformationMatrix();
            UpdateViewportBoundary();

            _previousPosition = _currentPosition;
            _previousZoom = _currentZoom;
        }

        UpdateTransformationMatrix();
        UpdateViewportBoundary();
    }

    /// <summary>
    /// Переводит экранные координаты (например, позицию мыши) в мировые координаты игрового поля.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        return Vector2.Transform(screenPosition, Matrix.Invert(_currentTransform));
    }

    /// <summary>
    /// Переводит мировые координаты в координаты экрана (например, для отрисовки UI над объектами).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 WorldToScreen(Vector2 worldPosition)
    {
        return Vector2.Transform(worldPosition, _currentTransform);
    }

    private void UpdateTransformationMatrix()
    {
        var viewport = graphicsDevice.Viewport;
        var zoom = MathF.Round(_currentZoom);

        var roundedPosition = new Vector2(
            MathF.Round(_currentPosition.X),
            MathF.Round(_currentPosition.Y)
        );

        _currentTransform = Matrix.CreateTranslation(new Vector3(-roundedPosition.X, -roundedPosition.Y, 0)) *
                            Matrix.CreateScale(new Vector3(zoom, zoom, 1)) *
                            Matrix.CreateTranslation(new Vector3(viewport.Width * 0.5f, viewport.Height * 0.5f, 0));
    }

    private void UpdateViewportBoundary()
    {
        var deviceViewport = graphicsDevice.Viewport;
        var topLeft = ScreenToWorld(Vector2.Zero);
        var bottomRight = ScreenToWorld(new Vector2(deviceViewport.Width, deviceViewport.Height));

        var width = (int)MathF.Ceiling(bottomRight.X - topLeft.X);
        var height = (int)MathF.Ceiling(bottomRight.Y - topLeft.Y);
        var x = (int)topLeft.X;
        var y = (int)topLeft.Y;

        ref var viewport = ref _currentViewport;
        viewport.Left = x;
        viewport.Right = x + width;
        viewport.Top = y;
        viewport.Bottom = y + height;
    }
}