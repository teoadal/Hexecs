using Hexecs.Collections;

namespace Hexecs.Tests.Collections;

public sealed class BlockShould
{
    [Fact(DisplayName = "Конструктор по умолчанию должен создавать пустой блок")]
    public void DefaultConstructor_ShouldCreateEmptyBlock()
    {
        // Arrange & Act
        var block = new Block<int>();

        // Assert
        block.IsEmpty.Should().BeTrue();
        block.Length.Should().Be(0);
        block.ToArray().Should().BeEmpty(); // Используем ToArray() из LINQ для проверки содержимого
    }

    [Fact(DisplayName = "Конструктор с одним элементом должен создавать блок с одним элементом")]
    public void SingleItemConstructor_ShouldCreateBlockWithOneItem()
    {
        // Arrange
        var item = 42;

        // Act
        var block = new Block<int>(item);

        // Assert
        block.IsEmpty.Should().BeFalse();
        block.Length.Should().Be(1);
        block[0].Should().Be(item);
        block.ToArray().Should().ContainSingle().Which.Should().Be(item);
    }

    [Fact(DisplayName = "Конструктор с массивом должен создавать блок с элементами массива")]
    public void ArrayConstructor_ShouldCreateBlockWithArrayElements()
    {
        // Arrange
        var array = new[] { 1, 2, 3 };

        // Act
        var block = new Block<int>(array);

        // Assert
        block.Length.Should().Be(array.Length);
        block.ToArray().Should().Equal(array);
    }

    [Fact(DisplayName = "Конструктор с null-массивом должен приводить к эквиваленту пустого блока")]
    public void ArrayConstructor_WithNullArray_ShouldResultInEmptyBlockEquivalent()
    {
        // Arrange
        int[]? nullArray = null;

        // Act
        // Конструктор public Block(T[] array) => _array = array;
        // Если nullArray передать, то _array станет null.
        // Length будет 0, IsEmpty будет true.
        var block = new Block<int>(nullArray!); // Подавляем предупреждение о null

        // Assert
        block.IsEmpty.Should().BeTrue();
        block.Length.Should().Be(0);
        block.ToArray().Should().BeEmpty();
    }

    [Fact(DisplayName = "Конструктор с массивом и длиной должен создавать блок указанной длины")]
    public void ArrayAndLengthConstructor_ShouldCreateBlockWithSpecifiedLength()
    {
        // Arrange
        var array = new[] { 1, 2, 3, 4, 5 };
        var length = 3;

        // Act
        var block = new Block<int>(array, length);

        // Assert
        block.Length.Should().Be(length);
        block.ToArray().Should().Equal(1, 2, 3);
    }

    [Fact(DisplayName = "Конструктор с массивом и нулевой длиной должен создавать пустой блок")]
    public void ArrayAndLengthConstructor_WithZeroLength_ShouldCreateEmptyBlock()
    {
        // Arrange
        var array = new[] { 1, 2, 3 };
        var length = 0;

        // Act
        var block = new Block<int>(array, length);

        // Assert
        block.IsEmpty.Should().BeTrue();
        block.Length.Should().Be(0);
        block.ToArray().Should().BeEmpty();
    }

    [Fact(DisplayName = "Конструктор с массивом и отрицательной длиной должен создавать пустой блок")]
    public void ArrayAndLengthConstructor_WithNegativeLength_ShouldCreateEmptyBlock()
    {
        // Arrange
        var array = new[] { 1, 2, 3 };
        var length = -1;

        // Act
        var block = new Block<int>(array, length);

        // Assert
        block.IsEmpty.Should().BeTrue();
        block.Length.Should().Be(0);
        block.ToArray().Should().BeEmpty();
    }

    [Fact(DisplayName = "Конструктор с массивом и длиной больше массива должен выбрасывать ArgumentOutOfRangeException")]
    public void ArrayAndLengthConstructor_WithLengthGreaterThanArray_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var array = new[] { 1, 2, 3 };
        var length = 5;

        // Act
        Action act = () => new Block<int>(array, length);

        // Assert
        // array.AsSpan(0, length) вызовет ArgumentOutOfRangeException
        act.Should().Throw<ArgumentOutOfRangeException>();
    }


    [Fact(DisplayName = "Конструктор с ReadOnlySpan должен создавать блок с элементами из Span")]
    public void ReadOnlySpanConstructor_ShouldCreateBlockWithSpanElements()
    {
        // Arrange
        var array = new[] { 10, 20, 30 };
        var span = new ReadOnlySpan<int>(array);

        // Act
        var block = new Block<int>(span);

        // Assert
        block.Length.Should().Be(span.Length);
        block.ToArray().Should().Equal(array);
    }

    [Fact(DisplayName = "Конструктор с пустым ReadOnlySpan должен создавать пустой блок")]
    public void ReadOnlySpanConstructor_WithEmptySpan_ShouldCreateEmptyBlock()
    {
        // Arrange
        var span = ReadOnlySpan<int>.Empty;

        // Act
        var block = new Block<int>(span);

        // Assert
        block.IsEmpty.Should().BeTrue();
        block.Length.Should().Be(0);
    }

    [Fact(DisplayName = "Конструктор со списком должен создавать блок с элементами списка")]
    public void ListConstructor_ShouldCreateBlockWithListElements()
    {
        // Arrange
        var list = new List<string> { "a", "b", "c" };

        // Act
        var block = new Block<string>(list);

        // Assert
        block.Length.Should().Be(list.Count);
        block.ToArray().Should().Equal(list);
    }

    [Fact(DisplayName = "Конструктор с пустым списком должен создавать пустой блок")]
    public void ListConstructor_WithEmptyList_ShouldCreateEmptyBlock()
    {
        // Arrange
        var list = new List<string>();

        // Act
        var block = new Block<string>(list);

        // Assert
        block.IsEmpty.Should().BeTrue();
        block.Length.Should().Be(0);
    }

    [Fact(DisplayName = "Конструктор с IEnumerable должен создавать блок с элементами IEnumerable")]
    public void EnumerableConstructor_ShouldCreateBlockWithEnumerableElements()
    {
        // Arrange
        var enumerable = Enumerable.Range(1, 3);

        // Act
        var block = new Block<int>(enumerable);

        // Assert
        block.Length.Should().Be(3);
        block.ToArray().Should().Equal(1, 2, 3);
    }

    [Fact(DisplayName = "Конструктор с пустым IEnumerable должен создавать пустой блок")]
    public void EnumerableConstructor_WithEmptyEnumerable_ShouldCreateEmptyBlock()
    {
        // Arrange
        var enumerable = Enumerable.Empty<int>();

        // Act
        var block = new Block<int>(enumerable);

        // Assert
        block.IsEmpty.Should().BeTrue();
        block.Length.Should().Be(0);
    }

    [Fact(DisplayName = "Конструктор с IEnumerable и длиной должен создавать блок указанной длины")]
    public void EnumerableAndLengthConstructor_ShouldCreateBlockWithSpecifiedLength()
    {
        // Arrange
        var enumerable = Enumerable.Range(1, 5); // 1, 2, 3, 4, 5
        var length = 3;

        // Act
        var block = new Block<int>(enumerable, length);

        // Assert
        block.Length.Should().Be(length);
        block.ToArray().Should().Equal(1, 2, 3);
    }

    [Fact(DisplayName = "Конструктор с IEnumerable и нулевой длиной должен создавать пустой блок")]
    public void EnumerableAndLengthConstructor_WithZeroLength_ShouldCreateEmptyBlock()
    {
        // Arrange
        var enumerable = Enumerable.Range(1, 5);
        var length = 0;

        // Act
        var block = new Block<int>(enumerable, length);

        // Assert
        block.IsEmpty.Should().BeTrue();
        block.Length.Should().Be(0);
    }

    [Fact(DisplayName = "IsEmpty должно быть true для конструктора по умолчанию")]
    public void IsEmpty_ShouldBeTrue_ForDefaultConstructor()
    {
        new Block<char>().IsEmpty.Should().BeTrue();
    }

    [Fact(DisplayName = "IsEmpty должно быть false для непустого блока")]
    public void IsEmpty_ShouldBeFalse_ForNonEmptyBlock()
    {
        new Block<int>(1).IsEmpty.Should().BeFalse();
    }

    [Fact(DisplayName = "Length должно быть 0 для конструктора по умолчанию")]
    public void Length_ShouldBeZero_ForDefaultConstructor()
    {
        new Block<object>().Length.Should().Be(0);
    }

    [Fact(DisplayName = "Length должно возвращать корректное количество")]
    public void Length_ShouldReturnCorrectCount()
    {
        new Block<int>(new[] { 1, 2, 3 }).Length.Should().Be(3);
    }

    [Fact(DisplayName = "AsMemory для пустого блока должен возвращать пустую память")]
    public void AsMemory_ForEmptyBlock_ShouldReturnEmptyMemory()
    {
        // Arrange
        var block = new Block<int>();

        // Act
        var memory = block.AsMemory();

        // Assert
        memory.IsEmpty.Should().BeTrue();
    }

    [Fact(DisplayName = "AsMemory для непустого блока должен возвращать память с элементами")]
    public void AsMemory_ForNonEmptyBlock_ShouldReturnMemoryWithElements()
    {
        // Arrange
        var array = new[] { 1, 2, 3 };
        var block = new Block<int>(array);

        // Act
        var memory = block.AsMemory();

        // Assert
        memory.Length.Should().Be(array.Length);
        memory.ToArray().Should().Equal(array);
    }

    [Fact(DisplayName = "AsSpan для пустого блока должен возвращать пустой Span")]
    public void AsSpan_ForEmptyBlock_ShouldReturnEmptySpan()
    {
        // Arrange
        var block = new Block<double>();

        // Act
        var span = block.AsSpan();

        // Assert
        span.IsEmpty.Should().BeTrue();
    }

    [Fact(DisplayName = "AsSpan для непустого блока должен возвращать Span с элементами")]
    public void AsSpan_ForNonEmptyBlock_ShouldReturnSpanWithElements()
    {
        // Arrange
        var array = new[] { "x", "y" };
        var block = new Block<string>(array);

        // Act
        var span = block.AsSpan();

        // Assert
        span.Length.Should().Be(array.Length);
        span.ToArray().Should().Equal(array);
    }

    [Fact(DisplayName = "Contains должен возвращать true, если элемент существует")]
    public void Contains_ShouldReturnTrue_IfItemExists()
    {
        var block = new Block<int>(new[] { 1, 2, 3 });
        block.Contains(2).Should().BeTrue();
    }

    [Fact(DisplayName = "Contains должен возвращать false, если элемент не существует")]
    public void Contains_ShouldReturnFalse_IfItemDoesNotExist()
    {
        var block = new Block<int>(new[] { 1, 2, 3 });
        block.Contains(4).Should().BeFalse();
    }

    [Fact(DisplayName = "Contains для пустого блока должен возвращать false")]
    public void Contains_ForEmptyBlock_ShouldReturnFalse()
    {
        var block = new Block<string>();
        block.Contains("a").Should().BeFalse();
    }

    private class CaseInsensitiveComparer : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y) => string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        public int GetHashCode(string obj) => obj.ToLowerInvariant().GetHashCode();
    }

    [Fact(DisplayName = "Contains с пользовательским компаратором должен использовать компаратор")]
    public void Contains_WithCustomComparer_ShouldUseComparer()
    {
        var block = new Block<string>(new[] { "Apple", "Banana" });
        block.Contains("apple", new CaseInsensitiveComparer()).Should().BeTrue();
        block.Contains("APPLE", EqualityComparer<string>.Default).Should().BeFalse(); // Default is case-sensitive
    }

    [Fact(DisplayName = "GetRef должен возвращать ссылку на элемент")]
    public void GetRef_ShouldReturnRefToElement()
    {
        // Arrange
        var array = new[] { 10, 20, 30 };
        var block = new Block<int>(array); // Block uses this array instance

        // Act
        ref var itemRef = ref block.GetRef(1);

        // Assert
        itemRef.Should().Be(20);

        // Modify through ref
        itemRef = 25;

        // Assert modification is reflected if block._array is the same instance
        block[1].Should().Be(25); // Check via indexer
        array[1].Should().Be(25); // Original array is also modified
    }

    [Fact(DisplayName = "GetRef на блоке от конструктора по умолчанию должен выбрасывать IndexOutOfRangeException при доступе к элементу")]
    public void GetRef_OnBlockFromDefaultConstructor_ShouldReturnNullRef_AndNotThrow()
    {
        // Arrange
        var block = new Block<int>();

        // Act
        // Accessing ref Unsafe.NullRef<T>() is unsafe and can lead to crashes if dereferenced.
        // We can't directly assert it's Unsafe.NullRef<T> easily in a safe way.
        // We can check that it doesn't throw for valid index (0) if Length is 0.
        // However, GetRef directly accesses _array[index], so if _array is null, it will throw NullReferenceException.
        // The check `if (_array == null) return ref Unsafe.NullRef<T>();` means for a default ctor, this path is taken.
        // The behavior of *using* Unsafe.NullRef is undefined if not careful.
        // Let's test the scenario where _array is null.

        // For an empty block (default constructor), _array is an empty array `[]`, not null.
        // `public Block() { _array = []; }`
        // So `_array == null` will be false. It will try `_array[index]`.
        // If index is 0 for an empty array, it will throw IndexOutOfRangeException.

        // Let's test with _array explicitly null using the constructor that takes T[]
        Block<int> blockWithNullArray = new Block<int>((int[]?)null!);
        Action act = () =>
        {
            ref var _ = ref blockWithNullArray.GetRef(0);
            // Попытка использования ref Unsafe.NullRef<T>() приведет к NullReferenceException
            // Console.WriteLine(_); // Это вызовет исключение
        };

        // In this specific case, if _array is null, it returns Unsafe.NullRef<T>.
        // Calling a method on this ref would typically cause a NullReferenceException.
        // The test should be cautious not to dereference it.
        // A "safer" check might be to see if it behaves as expected in a controlled way,
        // or simply trust the implementation if `_array == null` branch.
        // For now, let's ensure it does not throw on *getting* the ref if _array is null.
        // No, GetRef itself will not throw if _array is null because of the check.
        // The danger is in *using* the returned NullRef.
        act.Should().NotThrow(); // Получение ссылки не должно вызывать исключение


        // The original code for GetRef:
        // if (_array == null) return ref Unsafe.NullRef<T>();
        // return ref _array[index];

        // Let's re-evaluate. If `new Block<int>((int[])null)` is created, `_array` is null.
        // `GetRef(0)` would return `Unsafe.NullRef<T>`.
        // This is hard to assert directly.
        // What if we check that accessing it (which is unsafe) causes a NullReferenceException?
        // This is testing unsafe behavior, which might not be ideal.

        // A default constructed Block has _array = []. So _array is not null.
        // GetRef(0) on new Block<int>() will throw IndexOutOfRangeException.
        var defaultBlock = new Block<int>();
        Action actDefault = () =>
        {
            ref var _ = ref defaultBlock.GetRef(0);
        };
        actDefault.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact(DisplayName = "GetRef с индексом вне диапазона должен выбрасывать исключение")]
    public void GetRef_IndexOutOfRange_ShouldThrow()
    {
        var block = new Block<int>(new[] { 1 });
        Action act = () =>
        {
            ref var _ = ref block.GetRef(1);
        };
        act.Should().Throw<IndexOutOfRangeException>();

        Action actNegative = () =>
        {
            ref var _ = ref block.GetRef(-1);
        };
        actNegative.Should().Throw<IndexOutOfRangeException>();
    }


    [Fact(DisplayName = "GetEnumerator для пустого блока не должен возвращать элементы")]
    public void GetEnumerator_ForEmptyBlock_ShouldNotYieldAnyItems()
    {
        var block = new Block<decimal>();
        var count = 0;
        foreach (var _ in block)
        {
            count++;
        }

        count.Should().Be(0);
    }

    [Fact(DisplayName = "GetEnumerator должен перебирать все элементы")]
    public void GetEnumerator_ShouldIterateOverAllItems()
    {
        var array = new[] { "one", "two", "three" };
        var block = new Block<string>(array);
        var iteratedItems = new List<string>();

        foreach (var item in block)
        {
            iteratedItems.Add(item);
        }

        iteratedItems.Should().Equal(array);
    }

    [Fact(DisplayName = "GetUnderlyingArray для блока по умолчанию должен возвращать пустой массив")]
    public void GetUnderlyingArray_ForDefaultBlock_ShouldReturnEmptyArray()
    {
        var block = new Block<int>();
        block.GetUnderlyingArray().Should().NotBeNull().And.BeEmpty();
    }

    [Fact(DisplayName = "GetUnderlyingArray для блока с внутренним null-массивом должен возвращать пустой массив")]
    public void GetUnderlyingArray_ForBlockWithNullInternalArray_ShouldReturnEmptyArray()
    {
        // This constructor sets _array to the passed array directly
        var block = new Block<int>((int[]?)null!);
        block.GetUnderlyingArray().Should().NotBeNull().And.BeEmpty();
    }

    [Fact(DisplayName = "GetUnderlyingArray должен возвращать внутренний массив")]
    public void GetUnderlyingArray_ShouldReturnInternalArray()
    {
        var array = new[] { 1, 2 };
        var block = new Block<int>(array); // This constructor assigns the reference
        var underlyingArray = block.GetUnderlyingArray();

        underlyingArray.Should().BeSameAs(array); // Checks if it's the exact same instance
    }

    [Fact(DisplayName = "GetUnderlyingArray из конструктора IEnumerable может возвращать другой экземпляр массива")]
    public void GetUnderlyingArray_FromIEnumerableConstructor_MayReturnDifferentArrayInstance()
    {
        var sourceArray = new[] { 1, 2 };
        IEnumerable<int> enumerable = sourceArray;
        var block = new Block<int>(enumerable); // Uses CollectionUtils.ToArray

        var underlyingArray = block.GetUnderlyingArray();
        underlyingArray.Should().NotBeSameAs(sourceArray); // CollectionUtils.ToArray likely creates a new array
        underlyingArray.Should().Equal(sourceArray);
    }

    [Fact(DisplayName = "IndexOf должен возвращать правильный индекс, если элемент существует")]
    public void IndexOf_ShouldReturnCorrectIndex_IfItemExists()
    {
        var block = new Block<string>(new[] { "a", "b", "c", "b" });
        block.IndexOf("b").Should().Be(1); // First occurrence
    }

    [Fact(DisplayName = "IndexOf должен возвращать -1, если элемент не существует")]
    public void IndexOf_ShouldReturnNegativeOne_IfItemDoesNotExist()
    {
        var block = new Block<string>(new[] { "a", "b", "c" });
        block.IndexOf("d").Should().Be(-1);
    }

    [Fact(DisplayName = "IndexOf для пустого блока должен возвращать -1")]
    public void IndexOf_ForEmptyBlock_ShouldReturnNegativeOne()
    {
        var block = new Block<int>();
        block.IndexOf(100).Should().Be(-1);
    }

    [Fact(DisplayName = "IndexOf с пользовательским компаратором должен использовать компаратор")]
    public void IndexOf_WithCustomComparer_ShouldUseComparer()
    {
        var block = new Block<string>(new[] { "Cat", "Dog" });
        block.IndexOf("cat", new CaseInsensitiveComparer()).Should().Be(0);
        block.IndexOf("CAT", EqualityComparer<string>.Default).Should().Be(-1);
    }

    [Fact(DisplayName = "IndexOf для блока с внутренним null-массивом должен возвращать -1")]
    public void IndexOf_OnBlockWithNullInternalArray_ShouldReturnNegativeOne()
    {
        var block = new Block<int>((int[]?)null!);
        block.IndexOf(1).Should().Be(-1);
    }

    [Fact(DisplayName = "Индексатор Get должен возвращать элемент по индексу")]
    public void Indexer_Get_ShouldReturnElementAtIndex()
    {
        var block = new Block<double>(new[] { 1.1, 2.2, 3.3 });
        block[1].Should().Be(2.2);
    }

    [Fact(DisplayName = "Индексатор Get с индексом вне диапазона должен выбрасывать исключение")]
    public void Indexer_Get_IndexOutOfRange_ShouldThrow()
    {
        var block = new Block<char>(new[] { 'a' });
        Action act = () =>
        {
            var _ = block[1];
        };
        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact(DisplayName = "Индексатор Set должен изменять элемент в базовом массиве")]
    public void Indexer_Set_ShouldModifyElementInUnderlyingArray()
    {
        // Arrange
        var originalArray = new[] { 10, 20, 30 };
        // This constructor Block(T[] array) makes _array refer to originalArray
        var block = new Block<int>(originalArray);

        // Act
        block[1] = 25;

        // Assert
        // The block itself is a readonly struct, so a copy might be modified if not careful.
        // However, the _array field is readonly, pointing to originalArray.
        // The setter `_array![index] = value;` modifies the content of originalArray.
        block[1].Should().Be(25);
        originalArray[1].Should().Be(25); // Verify original array is changed
        block.GetUnderlyingArray()[1].Should().Be(25); // Underlying array reflects change
    }

    [Fact(DisplayName = "Индексатор Set на копии не должен влиять на массив оригинальной структуры, если он скопирован (но тут массив общий)")]
    public void Indexer_Set_OnCopy_ShouldNotAffectOriginalStructsArrayIfCopied()
    {
        // This test demonstrates behavior of value types (structs)
        var array = new[] { 1, 2, 3 };
        var block1 = new Block<int>(array);
        var block2 = block1; // Value type copy

        block2[0] = 100; // Modifies the array referenced by block2._array

        // Since _array is a reference type and both block1._array and block2._array
        // point to the *same* array instance after the copy,
        // modifying through block2 will be visible through block1.
        block1[0].Should().Be(100);
        array[0].Should().Be(100);
    }


    [Fact(DisplayName = "Индексатор Set с индексом вне диапазона должен выбрасывать исключение")]
    public void Indexer_Set_IndexOutOfRange_ShouldThrow()
    {
        var block = new Block<bool>(new[] { true });
        Action act = () => { block[1] = false; };
        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact(DisplayName = "IEnumerable<T>.GetEnumerator должен работать корректно")]
    public void IEnumerable_T_GetEnumerator_ShouldWorkCorrectly()
    {
        var array = new[] { 1, 2, 3 };
        var block = new Block<int>(array);
        IEnumerable<int> enumerable = block; // Explicitly use IEnumerable<T>

        var iteratedItems = new List<int>();
        foreach (var item in enumerable)
        {
            iteratedItems.Add(item);
        }

        iteratedItems.Should().Equal(array);
    }

    [Fact(DisplayName = "IEnumerable.GetEnumerator должен работать корректно")]
    public void IEnumerable_GetEnumerator_ShouldWorkCorrectly()
    {
        var array = new[] { 1, 2, 3 };
        var block = new Block<int>(array);
        System.Collections.IEnumerable enumerable = block; // Explicitly use IEnumerable

        var iteratedItems = new List<object>(); // IEnumerator returns object
        foreach (var item in enumerable)
        {
            iteratedItems.Add(item);
        }

        // Convert object list to int list for comparison
        iteratedItems.Cast<int>().Should().Equal(array);
    }
}