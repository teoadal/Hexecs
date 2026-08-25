using System.Reflection;

namespace Hexecs.Tests.Actors;

public class ActorMethodsConsistencyTests
{
    /// <summary>
    /// Все типы, которые должны иметь общие методы
    /// </summary>
    private static readonly Type[] ActorTypes =
    [
        typeof(Actor),
        typeof(Actor<>),
        typeof(ActorRef<>),
        typeof(ActorRef<,>),
        typeof(ActorRef<,,>)
    ];

    /// <summary>
    /// Эталонный тип — источник правды для списка методов
    /// </summary>
    private static readonly Type ReferenceType = typeof(Actor);

    /// <summary>
    /// Методы, которые исключаем из проверки (специфичные для типа или системные)
    /// </summary>
    private static readonly HashSet<string> ExcludedMethods =
    [
        "AsRef",
        "GetType",
        "ToString",
        "Equals",
        "GetHashCode"
    ];

    private static IEnumerable<string> CommonMethodNames =>
        ReferenceType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(m => m.Name)
            .Where(name => !ExcludedMethods.Contains(name))
            .Distinct();

    [Fact(DisplayName = "Все типы акторов должны иметь общие методы")]
    public void AllActorTypes_ShouldHaveCommonMethods()
    {
        var expectedMethods = CommonMethodNames.ToList();

        foreach (var type in ActorTypes.Where(t => t != ReferenceType))
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var methodNames = methods.Select(m => m.Name).Distinct().ToHashSet();

            foreach (var expectedMethod in expectedMethods)
            {
                methodNames.Should().Contain(expectedMethod,
                    because: $"тип {type.Name} должен содержать метод {expectedMethod} (как в {ReferenceType.Name})");
            }
        }
    }

    [Fact(DisplayName = "Все общие методы должны иметь одинаковые сигнатуры")]
    public void CommonMethods_ShouldHaveIdenticalSignatures()
    {
        foreach (var methodName in CommonMethodNames)
        {
            var referenceMethods = GetMethodSignatures(ReferenceType, methodName);

            foreach (var type in ActorTypes.Where(t => t != ReferenceType))
            {
                var typeMethods = GetMethodSignatures(type, methodName);

                typeMethods.Should().BeEquivalentTo(referenceMethods,
                    because:
                    $"метод {methodName} в {type.Name} должен иметь такую же сигнатуру как в {ReferenceType.Name}");
            }
        }
    }

    [Fact(DisplayName = "Все общие методы должны иметь AggressiveInlining-атрибут")]
    public void CommonMethods_ShouldHaveAggressiveInlining()
    {
        foreach (var type in ActorTypes)
        {
            var methods = type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => CommonMethodNames.Contains(m.Name));

            foreach (var method in methods)
            {
                var implFlags = method.MethodImplementationFlags;

                implFlags.Should().HaveFlag(MethodImplAttributes.AggressiveInlining,
                    because: $"метод {type.Name}.{method.Name} должен иметь атрибут AggressiveInlining");
            }
        }
    }

    private static List<MethodSignature> GetMethodSignatures(Type type, string methodName)
    {
        return type
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.Name == methodName)
            .Select(m => new MethodSignature(m))
            .ToList();
    }

    private sealed class MethodSignature
    {
        public string Name { get; }
        public string ReturnType { get; }
        public string[] ParameterTypes { get; }
        public string[] GenericConstraints { get; }
        public int GenericArgumentCount { get; }

        public MethodSignature(MethodInfo method)
        {
            Name = method.Name;
            ReturnType = NormalizeTypeName(method.ReturnType);
            ParameterTypes = method.GetParameters()
                .Select(p => NormalizeTypeName(p.ParameterType))
                .ToArray();
            GenericArgumentCount = method.GetGenericArguments().Length;
            GenericConstraints = method.GetGenericArguments()
                .SelectMany(g => g.GetGenericParameterConstraints())
                .Select(NormalizeTypeName)
                .OrderBy(x => x)
                .ToArray();
        }

        private static string NormalizeTypeName(Type type)
        {
            if (type.IsGenericParameter)
                return "T";

            if (type.IsGenericType)
            {
                var baseName = type.GetGenericTypeDefinition().Name;
                var args = string.Join(",", type.GetGenericArguments().Select(NormalizeTypeName));
                return $"{baseName}[{args}]";
            }

            if (type.IsByRef)
                return $"ref {NormalizeTypeName(type.GetElementType()!)}";

            return type.Name;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not MethodSignature other) return false;
            return Name == other.Name
                   && ReturnType == other.ReturnType
                   && GenericArgumentCount == other.GenericArgumentCount
                   && ParameterTypes.SequenceEqual(other.ParameterTypes)
                   && GenericConstraints.SequenceEqual(other.GenericConstraints);
        }

        public override int GetHashCode() => HashCode.Combine(Name, ReturnType, GenericArgumentCount);

        public override string ToString() =>
            $"{ReturnType} {Name}<{GenericArgumentCount}>({string.Join(", ", ParameterTypes)})";
    }
}