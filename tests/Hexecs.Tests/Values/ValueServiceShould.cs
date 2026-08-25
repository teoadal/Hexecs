using Hexecs.Values;

namespace Hexecs.Tests.Values;

public sealed class ValueServiceShould
{
    [Fact(DisplayName = "Empty должен возвращать сервис без таблиц")]
    public void Empty_ShouldReturnServiceWithoutTables()
    {
        // Act
        var service = ValueService.Empty;

        // Assert
        service.Should().NotBeNull();
        service.HasTable("anyTable").Should().BeFalse();
    }

    [Fact(DisplayName = "HasTable должен возвращать true для существующей таблицы")]
    public void HasTable_ShouldReturnTrue_ForExistingTable()
    {
        // Arrange
        var tableName = "TestTable";
        var table = new ValueTable<int, int>(tableName);
        var service = new ValueService([table]);

        // Act
        var result = service.HasTable(tableName);

        // Assert
        result.Should().BeTrue();
    }

    [Fact(DisplayName = "HasTable должен возвращать false для несуществующей таблицы")]
    public void HasTable_ShouldReturnFalse_ForNonExistingTable()
    {
        // Arrange
        var service = ValueService.Empty;

        // Act
        var result = service.HasTable("NonExistingTable");

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "GetTable должен возвращать таблицу по имени")]
    public void GetTable_ShouldReturnTableByName()
    {
        // Arrange
        var tableName = "TestTable";
        var table = new ValueTable<int, int>(tableName);
        var service = new ValueService([table]);

        // Act
        var result = service.GetTable(tableName);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(tableName);
        result.KeyType.Should().Be(typeof(int));
        result.ValueType.Should().Be(typeof(int));
    }

    [Fact(DisplayName = "GetTable должен выбрасывать исключение для несуществующей таблицы")]
    public void GetTable_ShouldThrowException_ForNonExistingTable()
    {
        // Arrange
        var service = ValueService.Empty;
        var nonExistingTableName = "NonExistingTable";

        // Act
        Action act = () => service.GetTable(nonExistingTableName);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact(DisplayName = "GetTable<TKey> должен возвращать типизированную таблицу с правильным типом ключа")]
    public void GetTableWithKey_ShouldReturnTypedTable_WithCorrectKeyType()
    {
        // Arrange
        var tableName = "TestTable";
        var table = new ValueTable<int, string>(tableName);
        var service = new ValueService([table]);

        // Act
        var result = service.GetTable<int>(tableName);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IValueTable<int>>();
        result.KeyType.Should().Be(typeof(int));
    }

    [Fact(DisplayName = "GetTable<TKey> должен выбрасывать исключение при неправильном типе ключа")]
    public void GetTableWithKey_ShouldThrowException_WhenKeyTypeIsIncorrect()
    {
        // Arrange
        var tableName = "TestTable";
        var table = new ValueTable<int, int>(tableName);
        var service = new ValueService([table]);

        // Act
        Action act = () => service.GetTable<string>(tableName);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact(DisplayName = "GetTable<TKey, TValue> должен возвращать типизированную таблицу с правильными типами")]
    public void GetTableWithKeyAndValue_ShouldReturnTypedTable_WithCorrectTypes()
    {
        // Arrange
        var tableName = "TestTable";
        var table = new ValueTable<int, double>(tableName);
        var service = new ValueService([table]);

        // Act
        var result = service.GetTable<int, double>(tableName);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IValueTable<int, double>>();
        result.KeyType.Should().Be(typeof(int));
        result.ValueType.Should().Be(typeof(double));
    }

    [Fact(DisplayName = "GetTable<TKey, TValue> должен выбрасывать исключение при неправильных типах")]
    public void GetTableWithKeyAndValue_ShouldThrowException_WhenTypesAreIncorrect()
    {
        // Arrange
        var tableName = "TestTable";
        var table = new ValueTable<int, double>(tableName);
        var service = new ValueService([table]);

        // Act & Assert
        Action act1 = () => service.GetTable<string, double>(tableName);
        act1.Should().Throw<Exception>();

        Action act2 = () => service.GetTable<int, int>(tableName);
        act2.Should().Throw<Exception>();
    }

    [Fact(DisplayName = "SetValue должен устанавливать значение в таблице")]
    public void SetValue_ShouldSetValueInTable()
    {
        // Arrange
        var tableName = "TestTable";
        var table = new ValueTable<int, int>(tableName);
        var service = new ValueService([table]);
        var key = 1;
        var value = 42;

        // Act
        service.SetValue(tableName, key, value);

        // Assert
        table.Has(key).Should().BeTrue();
        table.Get(key).Should().Be(value);
    }

    [Fact(DisplayName = "GetValue должен возвращать значение из таблицы")]
    public void GetValue_ShouldReturnValueFromTable()
    {
        // Arrange
        var tableName = "TestTable";
        var table = new ValueTable<int, int>(tableName);
        var service = new ValueService([table]);
        var key = 1;
        var value = 42;
        table.Set(key, value);

        // Act
        var result = service.GetValue<int, int>(tableName, key);

        // Assert
        result.Should().Be(value);
    }

    [Fact(DisplayName = "HasValue<TKey> должен проверять наличие ключа в таблице")]
    public void HasValueWithKey_ShouldCheckKeyExistence()
    {
        // Arrange
        var tableName = "TestTable";
        var table = new ValueTable<int, int>(tableName);
        var service = new ValueService([table]);
        var key = 1;
        var value = 42;
        table.Set(key, value);

        // Act & Assert
        service.HasValue(tableName, key).Should().BeTrue();
        service.HasValue(tableName, 999).Should().BeFalse();
    }

    [Fact(DisplayName = "HasValue<TKey, TValue> должен проверять совпадение значения по ключу")]
    public void HasValueWithKeyAndValue_ShouldCheckValueMatch()
    {
        // Arrange
        var tableName = "TestTable";
        var table = new ValueTable<int, int>(tableName);
        var service = new ValueService([table]);
        var key = 1;
        var value = 42;
        table.Set(key, value);

        // Act & Assert
        service.HasValue(tableName, key, value).Should().BeTrue();
        service.HasValue(tableName, key, 999).Should().BeFalse();
        service.HasValue(tableName, 999, value).Should().BeFalse();
    }

    [Fact(DisplayName = "RemoveValue<TKey> должен удалять значение из таблицы")]
    public void RemoveValueWithKey_ShouldRemoveValueFromTable()
    {
        // Arrange
        var tableName = "TestTable";
        var table = new ValueTable<int, int>(tableName);
        var service = new ValueService([table]);
        var key = 1;
        var value = 42;
        table.Set(key, value);

        // Act
        var result = service.RemoveValue(tableName, key);

        // Assert
        result.Should().BeTrue();
        table.Has(key).Should().BeFalse();
    }

    [Fact(DisplayName = "RemoveValue<TKey, TValue> должен удалять значение и возвращать его")]
    public void RemoveValueWithKeyAndValue_ShouldRemoveAndReturnValue()
    {
        // Arrange
        var tableName = "TestTable";
        var table = new ValueTable<int, int>(tableName);
        var service = new ValueService([table]);
        var key = 1;
        var value = 42;
        table.Set(key, value);

        // Act
        var result = service.RemoveValue<int, int>(tableName, key, out var removedValue);

        // Assert
        result.Should().BeTrue();
        removedValue.Should().Be(value);
        table.Has(key).Should().BeFalse();
    }

    [Fact(DisplayName = "TryGetValue должен возвращать значение, если оно существует")]
    public void TryGetValue_ShouldReturnValue_WhenExists()
    {
        // Arrange
        var tableName = "TestTable";
        var table = new ValueTable<int, int>(tableName);
        var service = new ValueService([table]);
        var key = 1;
        var value = 42;
        table.Set(key, value);

        // Act
        var result = service.TryGetValue<int, int>(tableName, key, out var retrievedValue);

        // Assert
        result.Should().BeTrue();
        retrievedValue.Should().Be(value);
    }

    [Fact(DisplayName = "TryGetValue должен возвращать false, если ключ не существует")]
    public void TryGetValue_ShouldReturnFalse_WhenKeyDoesNotExist()
    {
        // Arrange
        const string tableName = "TestTable";
        var table = new ValueTable<int, int>(tableName);
        var service = new ValueService([table]);

        // Act
        var result = service.TryGetValue<int, int>(tableName, 999, out var retrievedValue);

        // Assert
        result.Should().BeFalse();
        retrievedValue.Should().Be(0);
    }

    [Fact(DisplayName = "GetValues должен возвращать все пары ключ-значение из таблицы")]
    public void GetValues_ShouldReturnAllKeyValuePairs()
    {
        // Arrange
        var tableName = "TestTable";
        var table = new ValueTable<int, int>(tableName);
        var service = new ValueService([table]);
        var data = new Dictionary<int, int>
        {
            { 1, 10 },
            { 2, 20 },
            { 3, 30 }
        };

        foreach (var kvp in data)
        {
            table.Set(kvp.Key, kvp.Value);
        }

        // Act
        var values = service.GetValues<int, int>(tableName).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Assert
        values.Should().BeEquivalentTo(data);
    }

    [Fact(DisplayName = "Clear должен очищать все таблицы")]
    public void Clear_ShouldClearAllTables()
    {
        // Arrange
        var table1 = new ValueTable<int, int>("Table1");
        var table2 = new ValueTable<string, double>("Table2");
        var service = new ValueService([table1, table2]);
            
        table1.Set(1, 10);
        table2.Set("test", 20.5);

        // Act
        service.Clear();

        // Assert
        table1.Has(1).Should().BeFalse();
        table2.Has("test").Should().BeFalse();
    }

    [Fact(DisplayName = "ClearTable должен очищать указанную таблицу")]
    public void ClearTable_ShouldClearSpecificTable()
    {
        // Arrange
        var table1 = new ValueTable<int, int>("Table1");
        var table2 = new ValueTable<string, double>("Table2");
        var service = new ValueService([table1, table2]);
            
        table1.Set(1, 10);
        table2.Set("test", 20.5);

        // Act
        var result = service.ClearTable("Table1");

        // Assert
        result.Should().BeTrue();
        table1.Has(1).Should().BeFalse();
        table2.Has("test").Should().BeTrue();
    }

    [Fact(DisplayName = "ClearTable должен возвращать false для несуществующей таблицы")]
    public void ClearTable_ShouldReturnFalse_ForNonExistingTable()
    {
        // Arrange
        var service = ValueService.Empty;

        // Act
        var result = service.ClearTable("NonExistingTable");

        // Assert
        result.Should().BeFalse();
    }
}