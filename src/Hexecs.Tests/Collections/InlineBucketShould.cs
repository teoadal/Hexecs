using Hexecs.Collections;

namespace Hexecs.Tests.Collections;

public sealed class InlineBucketShould
{
    [Fact(DisplayName = "Добавление элемента в пустую коллекцию должно увеличивать длину")]
    public void Add_ToEmptyBucket_ShouldIncreaseLength()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        var item = 42;
            
        // Act
        bucket.Add(item);
            
        // Assert
        bucket.Length.Should().Be(1);
        bucket[0].Should().Be(item);
    }
        
    [Theory(DisplayName = "Добавление элемента в непустую коллекцию должно увеличивать длину")]
    [AutoData]
    public void Add_ToNonEmptyBucket_ShouldIncreaseLength(int item1, int item2)
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        bucket.Add(item1);
            
        // Act
        bucket.Add(item2);
            
        // Assert
        bucket.Length.Should().Be(2);
        bucket[0].Should().Be(item1);
        bucket[1].Should().Be(item2);
    }
        
    [Fact(DisplayName = "Добавление элементов сверх размера встроенного массива должно работать корректно")]
    public void Add_MoreItemsThanInlineArraySize_ShouldWorkCorrectly()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        var items = Enumerable
            .Range(0, InlineBucket<int>.InlineArraySize + 1)
            .ToArray();
        
        // Act
        foreach (var item in items)
        {
            bucket.Add(item);
        }
            
        // Assert
        bucket.Length
            .Should()
            .Be(items.Length);
            
        for (var i = 0; i < items.Length; i++)
        {
            bucket[i]
                .Should()
                .Be(items[i]);
        }
    }
        
    [Fact(DisplayName = "Contains должен возвращать true для существующего элемента")]
    public void Contains_ExistingItem_ShouldReturnTrue()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        bucket.Add(1);
        bucket.Add(2);
        bucket.Add(3);
            
        // Act
        var result = bucket.Contains(2);
            
        // Assert
        result.Should().BeTrue();
    }
        
    [Fact(DisplayName = "Contains должен возвращать false для несуществующего элемента")]
    public void Contains_NonExistingItem_ShouldReturnFalse()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        bucket.Add(1);
        bucket.Add(2);
        bucket.Add(3);
            
        // Act
        var result = bucket.Contains(4);
            
        // Assert
        result.Should().BeFalse();
    }
        
    [Fact(DisplayName = "Contains должен использовать переданный компаратор")]
    public void Contains_WithCustomEqualityComparer_ShouldUseIt()
    {
        // Arrange
        var bucket = new InlineBucket<TestStruct>();
        var item1 = new TestStruct { Value = 10 };
        var item2 = new TestStruct { Value = 20 };
        bucket.Add(item1);
        bucket.Add(item2);
            
        var comparer = EqualityComparer<TestStruct>.Create((x, y) => x.Value == y.Value, x => x.Value.GetHashCode());
        var searchItem = new TestStruct { Value = 10 }; // Не та же ссылка, но то же значение
            
        // Act
        var result = bucket.Contains(searchItem, comparer);
            
        // Assert
        result.Should().BeTrue();
    }
        
    [Fact(DisplayName = "CopyTo должен копировать элементы в буфер")]
    public void CopyTo_ShouldCopyItemsToBuffer()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        bucket.Add(1);
        bucket.Add(2);
        bucket.Add(3);
            
        var buffer = new int[5];
        var span = new Span<int>(buffer);
            
        // Act
        bucket.CopyTo(ref span);
            
        // Assert
        buffer[0].Should().Be(1);
        buffer[1].Should().Be(2);
        buffer[2].Should().Be(3);
    }
        
    [Fact(DisplayName = "CopyTo не должен изменять буфер при пустой коллекции")]
    public void CopyTo_EmptyBucket_ShouldNotModifyBuffer()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        var buffer = new int[] { 1, 2, 3 };
        var expectedBuffer = new int[] { 1, 2, 3 };
        var span = new Span<int>(buffer);
            
        // Act
        bucket.CopyTo(ref span);
            
        // Assert
        buffer.Should().BeEquivalentTo(expectedBuffer);
    }
        
    [Fact(DisplayName = "Dispose должен очищать внутренние структуры")]
    public void Dispose_ShouldClearInternalStructures()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        for (var i = 0; i < 15; i++) // Добавляем больше, чем InlineArraySize
        {
            bucket.Add(i);
        }
            
        // Act
        bucket.Dispose();
            
        // Assert
        bucket.Length.Should().Be(0);
    }
        
    [Fact(DisplayName = "GetEnumerator должен позволять перебирать элементы")]
    public void GetEnumerator_ShouldEnumerateItems()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        var expectedItems = new[] { 1, 2, 3, 4, 5 };
            
        foreach (var item in expectedItems)
        {
            bucket.Add(item);
        }
            
        // Act
        var actualItems = new List<int>();
        foreach (var item in bucket)
        {
            actualItems.Add(item);
        }
            
        // Assert
        actualItems.Should().BeEquivalentTo(expectedItems);
    }
        
    [Theory(DisplayName = "GetRef должен возвращать ссылку на элемент по индексу")]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(15)]
    public void GetRef_ShouldReturnReferenceToItem(int index)
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        for (var i = 0; i < 20; i++)
        {
            bucket.Add(i);
        }
            
        // Act
        ref var itemRef = ref bucket.GetRef(index);
        var originalValue = itemRef;
        itemRef = 100;
            
        // Assert
        bucket[index].Should().Be(100);
            
        // Cleanup
        itemRef = originalValue;
    }
        
    [Fact(DisplayName = "IndexOf должен возвращать индекс существующего элемента")]
    public void IndexOf_ExistingItem_ShouldReturnCorrectIndex()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        bucket.Add(10);
        bucket.Add(20);
        bucket.Add(30);
            
        // Act
        var index = bucket.IndexOf(20);
            
        // Assert
        index.Should().Be(1);
    }
        
    [Fact(DisplayName = "IndexOf должен возвращать -1 для несуществующего элемента")]
    public void IndexOf_NonExistingItem_ShouldReturnMinusOne()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        bucket.Add(10);
        bucket.Add(20);
        bucket.Add(30);
            
        // Act
        var index = bucket.IndexOf(40);
            
        // Assert
        index.Should().Be(-1);
    }
        
    [Fact(DisplayName = "IndexOf должен возвращать -1 для пустой коллекции")]
    public void IndexOf_EmptyBucket_ShouldReturnMinusOne()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
            
        // Act
        var index = bucket.IndexOf(10);
            
        // Assert
        index.Should().Be(-1);
    }
        
    [Fact(DisplayName = "IndexOf должен использовать переданный компаратор")]
    public void IndexOf_WithCustomEqualityComparer_ShouldUseIt()
    {
        // Arrange
        var bucket = new InlineBucket<TestStruct>();
        bucket.Add(new TestStruct { Value = 10 });
        bucket.Add(new TestStruct { Value = 20 });
            
        var comparer = EqualityComparer<TestStruct>.Create(
            (x, y) => x.Value == y.Value, 
            x => x.Value.GetHashCode());
            
        // Act
        var index = bucket.IndexOf(new TestStruct { Value = 20 }, comparer);
            
        // Assert
        index.Should().Be(1);
    }
        
    [Fact(DisplayName = "Remove должен удалять существующий элемент из начала коллекции")]
    public void Remove_ExistingItemFromBeginning_ShouldRemoveAndReturnTrue()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        bucket.Add(10);
        bucket.Add(20);
        bucket.Add(30);
        var initialLength = bucket.Length;
            
        // Act
        var result = bucket.Remove(10);
            
        // Assert
        result.Should().BeTrue();
        bucket.Length.Should().Be(initialLength - 1);
        bucket[0].Should().Be(20);
        bucket[1].Should().Be(30);
    }
        
    [Fact(DisplayName = "Remove должен удалять существующий элемент из середины коллекции")]
    public void Remove_ExistingItemFromMiddle_ShouldRemoveAndReturnTrue()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        bucket.Add(10);
        bucket.Add(20);
        bucket.Add(30);
        var initialLength = bucket.Length;
            
        // Act
        var result = bucket.Remove(20);
            
        // Assert
        result.Should().BeTrue();
        bucket.Length.Should().Be(initialLength - 1);
        bucket[0].Should().Be(10);
        bucket[1].Should().Be(30);
    }
        
    [Fact(DisplayName = "Remove должен удалять существующий элемент из конца коллекции")]
    public void Remove_ExistingItemFromEnd_ShouldRemoveAndReturnTrue()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        bucket.Add(10);
        bucket.Add(20);
        bucket.Add(30);
        var initialLength = bucket.Length;
            
        // Act
        var result = bucket.Remove(30);
            
        // Assert
        result.Should().BeTrue();
        bucket.Length.Should().Be(initialLength - 1);
        bucket[0].Should().Be(10);
        bucket[1].Should().Be(20);
    }
        
    [Fact(DisplayName = "Remove должен возвращать false для несуществующего элемента")]
    public void Remove_NonExistingItem_ShouldReturnFalse()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        bucket.Add(10);
        bucket.Add(20);
        bucket.Add(30);
        var initialLength = bucket.Length;
            
        // Act
        var result = bucket.Remove(40);
            
        // Assert
        result.Should().BeFalse();
        bucket.Length.Should().Be(initialLength);
    }
        
    [Fact(DisplayName = "Remove должен возвращать false для пустой коллекции")]
    public void Remove_EmptyBucket_ShouldReturnFalse()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
            
        // Act
        var result = bucket.Remove(10);
            
        // Assert
        result.Should().BeFalse();
    }
        
    [Fact(DisplayName = "Remove должен использовать переданный компаратор")]
    public void Remove_WithCustomEqualityComparer_ShouldUseIt()
    {
        // Arrange
        var bucket = new InlineBucket<TestStruct>();
        bucket.Add(new TestStruct { Value = 10 });
        bucket.Add(new TestStruct { Value = 20 });
            
        var comparer = EqualityComparer<TestStruct>.Create(
            (x, y) => x.Value == y.Value, 
            x => x.Value.GetHashCode());
            
        // Act
        var result = bucket.Remove(new TestStruct { Value = 20 }, comparer);
            
        // Assert
        result.Should().BeTrue();
        bucket.Length.Should().Be(1);
    }
        
    [Fact(DisplayName = "Remove должен корректно обрабатывать элементы за пределами встроенного массива")]
    public void Remove_ItemFromBeyondInlineArray_ShouldWorkCorrectly()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
            
        // Заполняем больше элементов, чем размер встроенного массива
        for (var i = 0; i < 15; i++)
        {
            bucket.Add(i);
        }
            
        // Act
        var result = bucket.Remove(12);
            
        // Assert
        result.Should().BeTrue();
        bucket.Length.Should().Be(14);
            
        // Проверяем, что все элементы на своих местах
        for (var i = 0; i < 12; i++)
        {
            bucket[i].Should().Be(i);
        }
            
        bucket[12].Should().Be(13);
        bucket[13].Should().Be(14);
    }
        
    [Fact(DisplayName = "ToArray должен возвращать массив со всеми элементами")]
    public void ToArray_ShouldReturnArrayWithAllItems()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        bucket.Add(10);
        bucket.Add(20);
        bucket.Add(30);
            
        // Act
        var array = bucket.ToArray();
            
        // Assert
        array.Should().BeEquivalentTo(new[] { 10, 20, 30 });
    }
        
    [Fact(DisplayName = "ToArray должен возвращать пустой массив для пустой коллекции")]
    public void ToArray_EmptyBucket_ShouldReturnEmptyArray()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
            
        // Act
        var array = bucket.ToArray();
            
        // Assert
        array.Should().BeEmpty();
    }
        
    [Fact(DisplayName = "TryAdd должен добавлять элемент, если его нет в коллекции")]
    public void TryAdd_NonExistingItem_ShouldAddAndReturnTrue()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        bucket.Add(10);
        bucket.Add(20);
            
        // Act
        var result = bucket.TryAdd(30);
            
        // Assert
        result.Should().BeTrue();
        bucket.Length.Should().Be(3);
        bucket[2].Should().Be(30);
    }
        
    [Fact(DisplayName = "Индексатор должен правильно получать элементы из встроенного массива")]
    public void Indexer_Get_ShouldRetrieveItemsFromInlineArray()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        bucket.Add(10);
        bucket.Add(20);
        bucket.Add(30);
            
        // Act & Assert
        bucket[0].Should().Be(10);
        bucket[1].Should().Be(20);
        bucket[2].Should().Be(30);
    }
        
    [Fact(DisplayName = "Индексатор должен правильно получать элементы за пределами встроенного массива")]
    public void Indexer_Get_ShouldRetrieveItemsBeyondInlineArray()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
            
        // Добавляем больше элементов, чем размер встроенного массива
        for (var i = 0; i < 15; i++)
        {
            bucket.Add(i);
        }
            
        // Act & Assert
        for (var i = 0; i < 15; i++)
        {
            bucket[i].Should().Be(i);
        }
    }
        
    [Fact(DisplayName = "Индексатор должен правильно устанавливать элементы во встроенном массиве")]
    public void Indexer_Set_ShouldSetItemsInInlineArray()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
        bucket.Add(10);
        bucket.Add(20);
        bucket.Add(30);
            
        // Act
        bucket[1] = 25;
            
        // Assert
        bucket[0].Should().Be(10);
        bucket[1].Should().Be(25);
        bucket[2].Should().Be(30);
    }
        
    [Fact(DisplayName = "Индексатор должен правильно устанавливать элементы за пределами встроенного массива")]
    public void Indexer_Set_ShouldSetItemsBeyondInlineArray()
    {
        // Arrange
        var bucket = new InlineBucket<int>();
            
        // Добавляем больше элементов, чем размер встроенного массива
        for (var i = 0; i < 15; i++)
        {
            bucket.Add(i);
        }
            
        // Act
        bucket[12] = 120;
            
        // Assert
        bucket[12].Should().Be(120);
    }
        
    public struct TestStruct
    {
        public int Value;
    }
}