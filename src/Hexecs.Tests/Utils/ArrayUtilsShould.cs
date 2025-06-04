using Hexecs.Utils;

namespace Hexecs.Tests.Utils;

public sealed class ArrayUtilsShould 
{
    /// <summary>
    /// Проверяет, что метод Clear не вызывает ошибок при очистке пустого массива
    /// </summary>
    [Fact(DisplayName = "Очистка пустого массива не должна вызывать ошибок")]
    public void Clear_EmptyArray_ShouldDoNothing()
    {
        // Arrange
        var array = Array.Empty<int>();
        
        // Act 
        ArrayUtils.Clear(array);
        
        // Assert
        array
            .Should()
            .BeEmpty();
    }

    /// <summary>
    /// Проверяет, что метод Clear корректно очищает все элементы заполненного массива
    /// </summary>
    [Fact(DisplayName = "Очистка заполненного массива должна обнулить все элементы")] 
    public void Clear_FilledArray_ShouldClearAllElements()
    {
        // Arrange
        var array = new[] { 1, 2, 3, 4, 5 };
        
        // Act
        ArrayUtils.Clear(array);
        
        // Assert
        array
            .Should()
            .OnlyContain(x => x == 0);
    }

    /// <summary>
    /// Проверяет, что метод Clear с указанием длины очищает только заданное количество элементов
    /// </summary>
    [Fact(DisplayName = "Очистка массива с указанной длиной должна очистить только указанное количество элементов")]
    public void Clear_WithLength_ShouldClearSpecifiedNumberOfElements()
    {
        // Arrange
        var array = new[] { 1, 2, 3, 4, 5 };
        
        // Act
        ArrayUtils.Clear(array, 3);
        
        // Assert
        array.Take(3).Should().OnlyContain(x => x == 0);
        array.Skip(3).Should().BeEquivalentTo([4, 5]);
    }

    /// <summary>
    /// Проверяет корректность удаления элемента из середины массива методом Cut
    /// </summary>
    [Fact(DisplayName = "Вырезание элемента из середины массива должно сдвинуть последующие элементы")]
    public void Cut_ValidIndex_ShouldRemoveElementAndShiftArray()
    {
        // Arrange
        var array = new[] { 1, 2, 3, 4, 5 };
        
        // Act
        ArrayUtils.Cut(array, 1);
        
        // Assert
        array.Take(4).Should().BeEquivalentTo([1, 3, 4, 5]);
        array[4].Should().Be(0);
    }

    /// <summary>
    /// Проверяет корректность удаления последнего элемента массива методом Cut
    /// </summary>
    [Fact(DisplayName = "Вырезание последнего элемента массива должно его обнулить")]
    public void Cut_LastIndex_ShouldClearLastElement()
    {
        // Arrange
        var array = new[] { 1, 2, 3, 4, 5 };
        
        // Act
        ArrayUtils.Cut(array, 4);
        
        // Assert
        array.Take(4).Should().BeEquivalentTo([1, 2, 3, 4]);
        array[4].Should().Be(0);
    }

    /// <summary>
    /// Проверяет, что метод EnsureCapacity не изменяет массив, если запрошенная емкость меньше текущей
    /// </summary>
    [Fact(DisplayName = "Проверка емкости не должна изменять массив, если требуемый размер меньше текущего")]
    public void EnsureCapacity_SmallerCapacity_ShouldNotResize()
    {
        // Arrange
        var array = new int[5];
        var originalReference = array;
        
        // Act
        ArrayUtils.EnsureCapacity(ref array, 3);
        
        // Assert
        array.Should().BeSameAs(originalReference);
        array.Length.Should().Be(5);
    }

    /// <summary>
    /// Проверяет корректность увеличения емкости массива методом EnsureCapacity
    /// </summary>
    [Fact(DisplayName = "Увеличение емкости массива должно сохранить существующие элементы")]
    public void EnsureCapacity_LargerCapacity_ShouldResize()
    {
        // Arrange
        var array = new[] { 1, 2, 3 };
        
        // Act
        ArrayUtils.EnsureCapacity(ref array, 5);
        
        // Assert
        array.Length.Should().Be(5);
        array.Take(3).Should().BeEquivalentTo([1, 2, 3]);
    }

    /// <summary>
    /// Проверяет корректность вставки элемента внутри границ массива
    /// </summary>
    [Fact(DisplayName = "Вставка элемента в пределах массива должна корректно его разместить")]
    public void Insert_WithinBounds_ShouldInsertElement()
    {
        // Arrange
        var array = new[] { 1, 2, 3 };
        
        // Act
        ArrayUtils.Insert(ref array, 1, 10);
        
        // Assert
        array[1].Should().Be(10);
    }

    /// <summary>
    /// Проверяет корректность вставки элемента за пределами текущих границ массива
    /// </summary>
    [Fact(DisplayName = "Вставка элемента за пределами массива должна увеличить его размер")]
    public void Insert_BeyondBounds_ShouldResizeAndInsert()
    {
        // Arrange
        var array = new[] { 1, 2, 3 };
        
        // Act
        ArrayUtils.Insert(ref array, 5, 10);
        
        // Assert
        array.Length.Should().BeGreaterThan(5);
        array[5].Should().Be(10);
    }

    /// <summary>
    /// Проверяет корректность удаления существующего элемента из массива
    /// </summary>
    [Fact(DisplayName = "Удаление существующего элемента должно вернуть true и удалить элемент")]
    public void Remove_ExistingItem_ShouldRemoveAndReturnTrue()
    {
        // Arrange
        var array = new[] { 1, 2, 3, 4, 5 };
        
        // Act
        var result = ArrayUtils.Remove(array, 3);
        
        // Assert
        result.Should().BeTrue();
        array.Take(4).Should().BeEquivalentTo([1, 2, 4, 5]);
        array[4].Should().Be(0);
    }

    /// <summary>
    /// Проверяет поведение метода Remove при попытке удаления несуществующего элемента
    /// </summary>
    [Fact(DisplayName = "Удаление несуществующего элемента должно вернуть false")]
    public void Remove_NonExistingItem_ShouldReturnFalse()
    {
        // Arrange
        var array = new[] { 1, 2, 3, 4, 5 };
        
        // Act
        var result = ArrayUtils.Remove(array, 10);
        
        // Assert
        result.Should().BeFalse();
        array.Should().BeEquivalentTo([1, 2, 3, 4, 5]);
    }
}