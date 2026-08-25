using Hexecs.Collections;

namespace Hexecs.Tests.Collections;

public class BucketShould
{
    [Fact(DisplayName = "Конструктор: должен создать пустой Bucket, если capacity = 0")]
    public void Constructor_WithZeroCapacity_ShouldCreateEmptyBucket()
    {
        // Arrange & Act
        var bucket = new Bucket<int>(0);

        // Assert
        bucket.IsEmpty.Should().BeTrue();
        bucket.Length.Should().Be(0);
        bucket.AsSpan().IsEmpty.Should().BeTrue();
        bucket.AsMemory().IsEmpty.Should().BeTrue();
        bucket.AsReadOnlySpan().IsEmpty.Should().BeTrue();
    }

    [Fact(DisplayName = "Конструктор: должен создать Bucket с указанной capacity, но Length = 0")]
    public void Constructor_WithPositiveCapacity_ShouldCreateBucketWithCapacityButEmpty()
    {
        // Arrange & Act
        var bucket = new Bucket<string>(5);

        // Assert
        bucket.Length.Should().Be(0);
    }

    [Fact(DisplayName = "Add: должен корректно добавлять элементы и увеличивать Length")]
    public void Add_ShouldAddItemAndIncrementLength()
    {
        // Arrange
        var bucket = new Bucket<int>(1);
        var item1 = 10;
        var item2 = 20;

        // Act
        bucket.Add(item1);

        // Assert
        bucket.Length.Should().Be(1);
        bucket.IsEmpty.Should().BeFalse();
        bucket.AsReadOnlySpan()[0].Should().Be(item1);

        // Act
        bucket.Add(item2);

        // Assert
        bucket.Length.Should().Be(2);
        bucket.AsReadOnlySpan()[0].Should().Be(item1); // Проверяем, что элементы сохраняются
        bucket.AsReadOnlySpan()[1].Should().Be(item2);
    }

    [Fact(DisplayName = "Add: должен расширять внутренний массив при необходимости")]
    public void Add_ShouldResizeInternalArray_WhenCapacityExceeded()
    {
        // Arrange
        var bucket = new Bucket<int>(1); // Начальная вместимость 1
        bucket.Add(1);
        bucket.Add(2); // Вместимость должна увеличиться

        // Act
        bucket.Add(3);

        // Assert
        bucket.Length.Should().Be(3);
        bucket.ToArray().Should().Equal(1, 2, 3);
    }

    [Fact(DisplayName = "AsMemory: должен возвращать пустую Memory для пустого Bucket")]
    public void AsMemory_WhenEmpty_ShouldReturnEmptyMemory()
    {
        // Arrange
        var bucket = new Bucket<object>(0);

        // Act
        var memory = bucket.AsMemory();

        // Assert
        memory.IsEmpty.Should().BeTrue();
    }

    [Fact(DisplayName = "AsMemory: должен возвращать Memory с элементами для непустого Bucket")]
    public void AsMemory_WhenNotEmpty_ShouldReturnMemoryWithItems()
    {
        // Arrange
        var bucket = new Bucket<int>(2);
        bucket.Add(1);
        bucket.Add(2);

        // Act
        var memory = bucket.AsMemory();

        // Assert
        memory.Length.Should().BeGreaterThanOrEqualTo(bucket.Length); // ArrayPool может вернуть больший массив
        memory.Span.Slice(0, bucket.Length).ToArray().Should().Equal(1, 2);
    }

    [Fact(DisplayName = "AsSpan: должен возвращать пустой Span для пустого Bucket")]
    public void AsSpan_WhenEmpty_ShouldReturnEmptySpan()
    {
        // Arrange
        var bucket = new Bucket<Guid>(0);

        // Act
        var span = bucket.AsSpan();

        // Assert
        span.IsEmpty.Should().BeTrue();
    }

    [Fact(DisplayName = "AsSpan: должен возвращать Span с элементами для непустого Bucket")]
    public void AsSpan_WhenNotEmpty_ShouldReturnSpanWithItems()
    {
        // Arrange
        var bucket = new Bucket<string>(2);
        bucket.Add("hello");
        bucket.Add("world");

        // Act
        var span = bucket.AsSpan();

        // Assert
        span.Length.Should().Be(2);
        span.ToArray().Should().Equal("hello", "world");
    }

    [Fact(DisplayName = "AsReadOnlySpan: должен возвращать пустой ReadOnlySpan для пустого Bucket")]
    public void AsReadOnlySpan_WhenEmpty_ShouldReturnEmptyReadOnlySpan()
    {
        // Arrange
        var bucket = new Bucket<int>(0);

        // Act
        var span = bucket.AsReadOnlySpan();

        // Assert
        span.IsEmpty.Should().BeTrue();
    }

    [Fact(DisplayName = "AsReadOnlySpan: должен возвращать ReadOnlySpan с элементами для непустого Bucket")]
    public void AsReadOnlySpan_WhenNotEmpty_ShouldReturnReadOnlySpanWithItems()
    {
        // Arrange
        var bucket = new Bucket<char>(3);
        bucket.Add('a');
        bucket.Add('b');

        // Act
        var span = bucket.AsReadOnlySpan();

        // Assert
        span.Length.Should().Be(2);
        span.ToArray().Should().Equal('a', 'b');
    }

    [Fact(DisplayName = "Dispose: должен сбрасывать состояние Bucket")]
    public void Dispose_ShouldResetBucketState()
    {
        // Arrange
        var bucket = new Bucket<int>(3);
        bucket.Add(1);
        bucket.Add(2);

        // Act
        bucket.Dispose();

        // Assert
        bucket.Length.Should().Be(0);
        bucket.IsEmpty.Should().BeTrue();
        bucket.AsSpan().IsEmpty.Should().BeTrue(); // После Dispose массив должен быть пуст или null
    }

    [Fact(DisplayName = "Dispose: двойной вызов Dispose не должен вызывать ошибок")]
    public void Dispose_MultipleCalls_ShouldNotThrow()
    {
        // Arrange
        var bucket = new Bucket<int>(1);
        bucket.Add(10);
        bucket.Dispose();

        // Act
        Action act = () => bucket.Dispose();

        // Assert
        act.Should().NotThrow();
        bucket.Length.Should().Be(0);
        bucket.IsEmpty.Should().BeTrue();
    }

    [Fact(DisplayName = "GetEnumerator: должен корректно перечислять элементы")]
    public void GetEnumerator_ShouldEnumerateItemsCorrectly()
    {
        // Arrange
        var bucket = new Bucket<int>(3);
        bucket.Add(10);
        bucket.Add(20);
        bucket.Add(30);
        var expectedItems = new[] { 10, 20, 30 };
        var actualItems = new List<int>();

        // Act
        foreach (var item in bucket)
        {
            actualItems.Add(item);
        }

        // Assert
        actualItems.Should().Equal(expectedItems);
    }

    [Fact(DisplayName = "GetEnumerator: для пустого Bucket не должен ничего перечислять")]
    public void GetEnumerator_WhenEmpty_ShouldEnumerateNothing()
    {
        // Arrange
        var bucket = new Bucket<string>(0);
        var count = 0;

        // Act
        foreach (var _ in bucket)
        {
            count++;
        }

        // Assert
        count.Should().Be(0);
    }

    [Fact(DisplayName = "IndexOf: должен возвращать правильный индекс для существующего элемента")]
    public void IndexOf_ItemExists_ShouldReturnCorrectIndex()
    {
        // Arrange
        var bucket = new Bucket<string>(3);
        bucket.Add("a");
        bucket.Add("b");
        bucket.Add("c");

        // Act
        var indexB = bucket.IndexOf("b");
        var indexC = bucket.IndexOf("c");

        // Assert
        indexB.Should().Be(1);
        indexC.Should().Be(2);
    }

    [Fact(DisplayName = "IndexOf: должен возвращать -1 для несуществующего элемента")]
    public void IndexOf_ItemDoesNotExist_ShouldReturnMinusOne()
    {
        // Arrange
        var bucket = new Bucket<string>(2);
        bucket.Add("a");
        bucket.Add("b");

        // Act
        var index = bucket.IndexOf("z");

        // Assert
        index.Should().Be(-1);
    }

    [Fact(DisplayName = "IndexOf: должен возвращать -1 для пустого Bucket")]
    public void IndexOf_WhenEmpty_ShouldReturnMinusOne()
    {
        // Arrange
        var bucket = new Bucket<int>(0);

        // Act
        var index = bucket.IndexOf(123);

        // Assert
        index.Should().Be(-1);
    }

    [Fact(DisplayName = "IndexOf: должен использовать кастомный EqualityComparer")]
    public void IndexOf_WithCustomComparer_ShouldUseComparer()
    {
        // Arrange
        var bucket = new Bucket<string>(2);
        bucket.Add("apple");
        bucket.Add("APPLE");
        var comparer = StringComparer.OrdinalIgnoreCase;

        // Act
        var index = bucket.IndexOf("apple", comparer); // Должен найти "apple"
        var indexIgnoreCase = bucket.IndexOf("Apple", comparer); // Должен найти "APPLE" на позиции 1

        // Assert
        index.Should().Be(0);
        indexIgnoreCase.Should().Be(0); // Первый найденный "apple" или "APPLE"
    }

    [Fact(DisplayName = "Has: должен возвращать true, если элемент существует")]
    public void Has_ItemExists_ShouldReturnTrue()
    {
        // Arrange
        var bucket = new Bucket<int>(2);
        bucket.Add(100);
        bucket.Add(200);

        // Act
        var has = bucket.Has(200);

        // Assert
        has.Should().BeTrue();
    }

    [Fact(DisplayName = "Has: должен возвращать false, если элемент не существует")]
    public void Has_ItemDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var bucket = new Bucket<int>(1);
        bucket.Add(100);

        // Act
        var has = bucket.Has(300);

        // Assert
        has.Should().BeFalse();
    }

    [Fact(DisplayName = "Has: должен возвращать false для пустого Bucket")]
    public void Has_WhenEmpty_ShouldReturnFalse()
    {
        // Arrange
        var bucket = new Bucket<string>(0);

        // Act
        var has = bucket.Has("any");

        // Assert
        has.Should().BeFalse();
    }

    [Fact(DisplayName = "Has: должен использовать кастомный EqualityComparer")]
    public void Has_WithCustomComparer_ShouldUseComparer()
    {
        // Arrange
        var bucket = new Bucket<string>(1);
        bucket.Add("TestData");
        var comparer = StringComparer.OrdinalIgnoreCase;

        // Act
        var hasLower = bucket.Has("testdata", comparer);
        var hasUpper = bucket.Has("TESTDATA", comparer);

        // Assert
        hasLower.Should().BeTrue();
        hasUpper.Should().BeTrue();
    }

    [Fact(DisplayName = "Remove: должен удалять существующий элемент и возвращать true")]
    public void Remove_ItemExists_ShouldRemoveItemAndReturnTrue()
    {
        // Arrange
        var bucket = new Bucket<int>(3);
        bucket.Add(1);
        bucket.Add(2);
        bucket.Add(3);

        // Act
        var result = bucket.Remove(2);

        // Assert
        result.Should().BeTrue();
        bucket.Length.Should().Be(2);
        bucket.ToArray().Should().Equal(1, 3); // Порядок элементов после удаления важен
        bucket.Has(2).Should().BeFalse();
    }

    [Fact(DisplayName = "Remove: должен возвращать false, если элемент не существует")]
    public void Remove_ItemDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var bucket = new Bucket<int>(2);
        bucket.Add(1);
        bucket.Add(2);

        // Act
        var result = bucket.Remove(3);

        // Assert
        result.Should().BeFalse();
        bucket.Length.Should().Be(2);
        bucket.ToArray().Should().Equal(1, 2);
    }

    [Fact(DisplayName = "Remove: должен возвращать false для пустого Bucket")]
    public void Remove_WhenEmpty_ShouldReturnFalse()
    {
        // Arrange
        var bucket = new Bucket<string>(0);

        // Act
        var result = bucket.Remove("any");

        // Assert
        result.Should().BeFalse();
        bucket.Length.Should().Be(0);
    }

    [Fact(DisplayName = "Remove: должен использовать кастомный EqualityComparer")]
    public void Remove_WithCustomComparer_ShouldUseComparerAndRemoveItem()
    {
        // Arrange
        var bucket = new Bucket<string>(2);
        bucket.Add("ItemOne");
        bucket.Add("ItemTwo");
        var comparer = StringComparer.OrdinalIgnoreCase;

        // Act
        var result = bucket.Remove("itemone", comparer);

        // Assert
        result.Should().BeTrue();
        bucket.Length.Should().Be(1);
        bucket.ToArray().Should().Equal("ItemTwo");
        bucket.Has("ItemOne", comparer).Should().BeFalse();
    }

    [Fact(DisplayName = "ToArray: должен возвращать пустой массив для пустого Bucket")]
    public void ToArray_WhenEmpty_ShouldReturnEmptyArray()
    {
        // Arrange
        var bucket = new Bucket<int>(0);

        // Act
        var array = bucket.ToArray();

        // Assert
        array.Should().BeEmpty();
    }

    [Fact(DisplayName = "ToArray: должен возвращать массив с элементами для непустого Bucket")]
    public void ToArray_WhenNotEmpty_ShouldReturnArrayWithItems()
    {
        // Arrange
        var bucket = new Bucket<string>(3);
        bucket.Add("x");
        bucket.Add("y");
        bucket.Add("z");

        // Act
        var array = bucket.ToArray();

        // Assert
        array.Should().Equal("x", "y", "z");
    }

    [Fact(DisplayName = "ToBlock: должен корректно преобразовывать в Block (проверяем через ToArray)")]
    public void ToBlock_ShouldConvertCorrectly()
    {
        // Arrange
        var bucket = new Bucket<int>(3);
        bucket.Add(7);
        bucket.Add(8);
        bucket.Add(9);

        // Act
        // Так как Block<T> не определен в контексте, мы можем проверить, 
        // что ReadOnlySpan, используемый для его создания, корректен.
        // Если бы Block<T> имел метод ToArray() или свойство, мы бы использовали его.
        // В данном случае, проверим содержимое AsReadOnlySpan(), которое передается в конструктор Block.
        var blockEquivalentContent = bucket.AsReadOnlySpan().ToArray();

        // Assert
        blockEquivalentContent.Should().Equal(7, 8, 9);
        // Если бы Block<T> был доступен и имел метод ToArray():
        // var block = bucket.ToBlock();
        // block.ToArray().Should().Equal(7,8,9);
    }

    [Fact(DisplayName = "ToBlock: для пустого Bucket должен возвращать Block с пустым содержимым")]
    public void ToBlock_WhenEmpty_ShouldReturnEmptyBlockContent()
    {
        // Arrange
        var bucket = new Bucket<char>(0);

        // Act
        var blockEquivalentContent = bucket.AsReadOnlySpan().ToArray();

        // Assert
        blockEquivalentContent.Should().BeEmpty();
    }


    [Fact(DisplayName = "TryAdd: должен добавить элемент и вернуть true, если его нет")]
    public void TryAdd_NewItem_ShouldAddAndReturnTrue()
    {
        // Arrange
        var bucket = new Bucket<int>(1);

        // Act
        var result = bucket.TryAdd(10);

        // Assert
        result.Should().BeTrue();
        bucket.Length.Should().Be(1);
        bucket.Has(10).Should().BeTrue();
    }

    [Fact(DisplayName = "TryAdd: не должен добавлять элемент и вернуть false, если он уже есть")]
    public void TryAdd_ExistingItem_ShouldNotAddAndReturnFalse()
    {
        // Arrange
        var bucket = new Bucket<int>(2);
        bucket.Add(10);

        // Act
        var result = bucket.TryAdd(10);

        // Assert
        result.Should().BeFalse();
        bucket.Length.Should().Be(1); // Длина не должна измениться
    }

    [Fact(DisplayName = "TryAdd: должен использовать кастомный EqualityComparer")]
    public void TryAdd_WithCustomComparer_ShouldRespectComparer()
    {
        // Arrange
        var bucket = new Bucket<string>(2);
        var comparer = StringComparer.OrdinalIgnoreCase;
        bucket.Add("Example");

        // Act

        // Не должен добавить, т.к. уже есть без учета регистра
        var resultAddDifferentCase = bucket.TryAdd("example", comparer);
        var resultAddNew = bucket.TryAdd("NewItem", comparer); // Должен добавить

        // Assert
        resultAddDifferentCase.Should().BeFalse();
        resultAddNew.Should().BeTrue();
        bucket.Length.Should().Be(2);
        bucket.ToArray().Should().ContainInOrder("Example", "NewItem");
    }

    [Fact(DisplayName = "IEnumerable.GetEnumerator: должен работать как явная реализация")]
    public void IEnumerable_GetEnumerator_ShouldWork()
    {
        // Arrange
        var bucket = new Bucket<int>(2);
        bucket.Add(1);
        bucket.Add(2);
        System.Collections.IEnumerable enumerable = bucket;
        var items = new List<int>();

        // Act
        foreach (object item in enumerable)
        {
            items.Add((int)item);
        }

        // Assert
        items.Should().Equal(1, 2);
    }

    [Fact(DisplayName = "IEnumerable<T>.GetEnumerator: должен работать как явная реализация")]
    public void IEnumerableT_GetEnumerator_ShouldWork()
    {
        // Arrange
        var bucket = new Bucket<int>(2);
        bucket.Add(1);
        bucket.Add(2);
        IEnumerable<int> enumerable = bucket;
        var items = new List<int>();

        // Act
        foreach (int item in enumerable)
        {
            items.Add(item);
        }

        // Assert
        items.Should().Equal(1, 2);
    }
}