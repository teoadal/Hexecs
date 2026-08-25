using Hexecs.Utils;

namespace Hexecs.Tests.Utils;

public sealed class TypeOfTests
{
    [Fact(DisplayName = "Возвращает имя типа для не-обобщенного типа")]
    public void GetTypeName_ForNonGenericType_ShouldReturnTypeName()
    {
        // Arrange
        var type = typeof(int);

        // Act
        var result = TypeOf.GetTypeName(type);

        // Assert
        result.Should().Be("Int32");
    }

    [Fact(DisplayName = "Возвращает форматированное имя типа для обобщенного типа")]
    public void GetTypeName_ForGenericType_ShouldReturnFormattedTypeName()
    {
        // Arrange
        var type = typeof(List<int>);

        // Act
        var result = TypeOf.GetTypeName(type);

        // Assert
        result.Should().Be("List<Int32>");
    }

    [Fact(DisplayName = "Возвращает форматированное имя типа для вложенного обобщенного типа")]
    public void GetTypeName_ForNestedGenericType_ShouldReturnFormattedTypeName()
    {
        // Arrange
        var type = typeof(Dictionary<string, List<int>>);

        // Act
        var result = TypeOf.GetTypeName(type);

        // Assert
        result.Should().Be("Dictionary<String, List<Int32>>");
    }

    [Fact(DisplayName = "Генерирует уникальные идентификаторы для разных типов")]
    public void TypeOf_Generic_Id_ShouldBeUnique()
    {
        // Act
        var id1 = TypeOf<int>.Id;
        var id2 = TypeOf<string>.Id;
        var id3 = TypeOf<List<int>>.Id;

        // Assert
        id1.Should().NotBe(id2);
        id1.Should().NotBe(id3);
        id2.Should().NotBe(id3);
    }

    [Fact(DisplayName = "Возвращает корректное имя типа при использовании обобщенного класса TypeOf")]
    public void TypeOf_Generic_GetTypeName_ShouldReturnCorrectName()
    {
        // Act & Assert
        TypeOf<int>.GetTypeName().Should().Be("Int32");
        TypeOf<string>.GetTypeName().Should().Be("String");
        TypeOf<List<int>>.GetTypeName().Should().Be("List<Int32>");
    }
}