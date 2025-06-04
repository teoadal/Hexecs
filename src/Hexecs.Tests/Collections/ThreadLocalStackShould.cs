using Hexecs.Collections;

namespace Hexecs.Tests.Collections;

public sealed class ThreadLocalStackShould
{
    [Theory, AutoData]
    [Trait("Category", "Unit")]
    public void Store_And_Retrieve_Single_Element(uint value)
    {
        // Arrange
        using var stack = new ThreadLocalStack<uint>();

        // Act
        stack.Push(value);
        var success = stack.TryPop(out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().Be(value);
    }

    [Fact(DisplayName = "Стек должен корректно обрабатывать несколько элементов в порядке LIFO внутри одного потока")]
    [Trait("Category", "Unit")]
    public void Handle_Push_And_Pop_Of_Multiple_Elements_In_LIFO_Order()
    {
        // Arrange
        using var stack = new ThreadLocalStack<uint>();
        var values = new uint[] { 10, 20, 30, 40 };

        // Act
        foreach (var v in values) stack.Push(v);

        // Assert
        var results = new List<uint>();
        while (stack.TryPop(out var r)) results.Add(r);

        results.Should().Equal(40, 30, 20, 10);
    }

    [Fact(DisplayName = "Метод TryPop должен возвращать false при попытке извлечения из пустого стека")]
    [Trait("Category", "Unit")]
    public void Return_False_On_Empty_Stack()
    {
        // Arrange
        using var stack = new ThreadLocalStack<uint>();

        // Act
        var success = stack.TryPop(out _);

        // Assert
        success.Should().BeFalse();
    }

    [Fact(DisplayName = "Стек должен корректно работать с большим объемом данных в однопоточном режиме, гарантируя отсутствие потерь при миграции данных в глобальный пул")]
    [Trait("Category", "Unit")]
    public void Correctly_Handle_Large_Volume_In_Single_Thread()
    {
        // Arrange
        const int count = 5000;
        using var stack = new ThreadLocalStack<uint>(count);
        var results = new List<uint>(count);

        // Act
        for (uint i = 0; i < count; i++) stack.Push(i);
        
        while (stack.TryPop(out var result))
        {
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(count);
        
        // Вместо тяжелого BeEquivalentTo используем быструю проверку набора данных
        var resultHash = results.Aggregate(0u, (s, v) => s ^ v);
        var expectedHash = 0u;
        for (uint i = 0; i < count; i++) expectedHash ^= i;

        resultHash.Should().Be(expectedHash, "контрольная сумма всех ID должна совпадать");
        results.Should().OnlyHaveUniqueItems("ID не должны дублироваться");
    }

    [Fact(DisplayName = "Стек не должен терять или дублировать данные при интенсивной параллельной нагрузке из нескольких потоков")]
    [Trait("Category", "Concurrency")]
    public void Ensure_No_Data_Loss_Under_Parallel_Load()
    {
        // Arrange
        const int threadsCount = 4;
        const int opsPerThread = 25_000;
        const int totalCount = threadsCount * opsPerThread;
        
        using var stack = new ThreadLocalStack<uint>(totalCount);
        var counts = new int[totalCount];

        // Act
        Parallel.For(0, threadsCount, t =>
        {
            for (var i = 0; i < opsPerThread; i++)
            {
                var val = (uint)(t * opsPerThread + i);
                stack.Push(val);
                
                if (stack.TryPop(out var popped))
                {
                    Interlocked.Increment(ref counts[popped]);
                }
            }
        });

        // Добираем "осадок" из локальных буферов, чтобы убедиться, что всё на месте
        while (stack.TryPop(out var remaining))
        {
            Interlocked.Increment(ref counts[remaining]);
        }

        // Assert
        counts.Should().OnlyContain(x => x == 1, "каждый вставленный ID должен быть обработан ровно один раз");
    }

    [Fact(DisplayName = "Данные, добавленные в одном потоке, должны становиться доступными для извлечения в другом потоке через глобальный пул")]
    [Trait("Category", "Concurrency")]
    public async Task Allow_Threads_To_Exchange_Data_Through_Global_Pool()
    {
        // Arrange
        using var stack = new ThreadLocalStack<uint>();
        const uint elementCount = 300; // Больше чем LocalCapacity (128), гарантирует перелив в глобал

        // Act
        // Поток A: Заполняет и завершается
        await Task.Run(() =>
        {
            for (uint i = 0; i < elementCount; i++) stack.Push(i);
            stack.Flush();
        });

        // Поток B: Пытается выкачать всё
        var results = await Task.Run(() =>
        {
            var list = new List<uint>();
            var timeout = DateTime.Now.AddSeconds(5);
            while (list.Count < elementCount && DateTime.Now < timeout)
            {
                if (stack.TryPop(out var val)) list.Add(val);
                else Thread.Yield(); 
            }
            return list;
        });

        // Assert
        results.Should().HaveCount((int)elementCount, "все элементы должны быть переданы между потоками");
        results.Should().OnlyHaveUniqueItems();
    }
}