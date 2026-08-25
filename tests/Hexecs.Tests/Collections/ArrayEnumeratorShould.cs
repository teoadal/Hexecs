using System.Collections;
using Hexecs.Collections;

namespace Hexecs.Tests.Collections;

public sealed class ArrayEnumeratorShould
{
    [Fact(DisplayName = "Пустой перечислитель не должен перемещаться к следующему элементу")]
    public void Empty_Enumerator_Should_Not_MoveNext()
    {
        // Arrange
        var enumerator = ArrayEnumerator<int>.Empty;

        // Act
        var result = enumerator.MoveNext();

        // Assert
        result.Should().BeFalse();
    }

    [AutoData]
    [Theory(DisplayName = "Перечислитель должен корректно перебирать все элементы массива")]
    public void Enumerator_Should_Enumerate_All_Array_Elements(int[] testArray)
    {
        // Arrange
        var enumerator = new ArrayEnumerator<int>(testArray);
        var resultList = new List<int>();

        // Act
        while (enumerator.MoveNext())
        {
            resultList.Add(enumerator.Current);
        }

        // Assert
        resultList.Should().BeEquivalentTo(testArray);
    }

    [AutoData]
    [Theory(DisplayName = "Перечислитель должен корректно работать с заданной длиной")]
    public void Enumerator_Should_Respect_Specified_Length(int[] testArray)
    {
        // Arrange
        var specifiedLength = testArray.Length / 2;
        var enumerator = new ArrayEnumerator<int>(testArray, specifiedLength);
        var resultList = new List<int>();

        // Act
        while (enumerator.MoveNext())
        {
            resultList.Add(enumerator.Current);
        }

        // Assert
        resultList.Should().HaveCount(specifiedLength);
        resultList.Should().BeEquivalentTo(testArray.Take(specifiedLength));
    }

    [AutoData]
    [Theory(DisplayName = "Метод Reset должен сбрасывать индекс перечислителя")]
    public void Reset_Should_Set_Index_To_Initial_Position(int[] testArray)
    {
        // Arrange
        var enumerator = new ArrayEnumerator<int>(testArray);

        // Act - первый проход
        enumerator.MoveNext();
        var firstElement = enumerator.Current;

        // Полный проход
        while (enumerator.MoveNext())
        {
        }

        // Reset и снова первый элемент
        enumerator.Reset();
        var hasMoved = enumerator.MoveNext();
        var firstElementAfterReset = enumerator.Current;

        // Assert
        hasMoved.Should().BeTrue();
        firstElementAfterReset.Should().Be(firstElement);
    }

    [AutoData]
    [Theory(DisplayName = "Доступ к Current до вызова MoveNext должен вызывать исключение")]
    public void Current_Should_Throw_Exception_Before_MoveNext(int[] testArray)
    {
        // Arrange
        var enumerator = new ArrayEnumerator<int>(testArray);

        // Act & Assert
        Action act = () => _ = enumerator.Current;
        act.Should().Throw<IndexOutOfRangeException>();
    }

    [AutoData]
    [Theory(DisplayName = "Реализация IEnumerator.Current должна возвращать то же значение, что и Current")]
    public void IEnumerator_Current_Should_Return_Same_Value_As_Current(int[] testArray)
    {
        // Arrange
        var enumerator = new ArrayEnumerator<int>(testArray);
        IEnumerator genericEnumerator = enumerator;
        enumerator.MoveNext();
        genericEnumerator.MoveNext();
        
        // Act
        var current = enumerator.Current;
        var genericCurrent = genericEnumerator.Current;

        // Assert
        genericCurrent
            .Should()
            .Be(current);
    }

    [Fact(DisplayName = "IDisposable.Dispose не должен вызывать исключений")]
    public void Dispose_Should_Not_Throw()
    {
        // Arrange
        var enumerator = new ArrayEnumerator<int>([1, 2, 3]);
        IDisposable disposable = enumerator;

        // Act & Assert
        Action act = () => disposable.Dispose();
        act.Should().NotThrow();
    }

    [AutoData]
    [Theory(DisplayName = "Свойство Current должно возвращать ссылку на текущий элемент")]
    public void Current_Should_Return_Reference_To_Current_Element(int[] testArray)
    {
        // Arrange
        var enumerator = new ArrayEnumerator<int>(testArray);

        // Act
        enumerator.MoveNext();
        var originalValue = enumerator.Current;
        var newValue = originalValue + 1;

        // Изменяем значение в массиве через ссылку
        testArray[0] = newValue;

        // Assert
        enumerator.Current.Should().Be(newValue);
    }

    [Fact(DisplayName = "Конструктор без параметров должен создать пустой перечислитель")]
    public void Default_Constructor_Should_Create_Empty_Enumerator()
    {
        // Arrange & Act
        var enumerator = new ArrayEnumerator<int>();

        // Assert
        enumerator.MoveNext().Should().BeFalse();
    }

    [AutoData]
    [Theory(DisplayName = "IEnumerator<T>.Current должен возвращать то же значение, что и Current")]
    public void IEnumeratorT_Current_Should_Return_Same_Value_As_Current(int[] testArray)
    {
        // Arrange
        var enumerator = new ArrayEnumerator<int>(testArray);
        IEnumerator<int> genericEnumerator = enumerator;
        enumerator.MoveNext();
        genericEnumerator.MoveNext();
        
        // Act

        var current = enumerator.Current;
        var genericCurrent = genericEnumerator.Current;

        // Assert
        genericCurrent
            .Should()
            .Be(current);
    }
}