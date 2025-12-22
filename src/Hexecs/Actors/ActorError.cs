namespace Hexecs.Actors;

/// <summary>
/// Статический класс, содержащий методы для обработки ошибок, связанных с актёрами.
/// Предоставляет удобные методы для генерации типовых исключений при работе с актёрами.
/// </summary>
internal static class ActorError
{
    /// <summary>
    /// Генерирует исключение, когда подходящий актёр не найден.
    /// </summary>
    [DoesNotReturn]
    public static void ApplicableNotFound()
    {
        throw new Exception("Applicable actor isn't found");
    }

    /// <summary>
    /// Генерирует исключение, когда подходящий актёр с указанным компонентом не найден.
    /// </summary>
    /// <typeparam name="T">Тип компонента, который должен быть у актёра</typeparam>
    [DoesNotReturn]
    public static void ApplicableNotFound<T>()
    {
        throw new Exception($"Applicable actor with component {TypeOf<T>.GetTypeName()} isn't found");
    }

    /// <summary>
    /// Генерирует исключение, когда актёр с указанным идентификатором уже существует.
    /// </summary>
    /// <param name="actorId">Идентификатор актёра</param>
    [DoesNotReturn]
    public static void AlreadyExists(uint actorId)
    {
        throw new Exception($"Actor with id {actorId} already exists");
    }

    /// <summary>
    /// Генерирует исключение, когда актёр не имеет связанного ресурса.
    /// </summary>
    /// <param name="actorId">Идентификатор актёра</param>
    [DoesNotReturn]
    public static void AssetNotFound(uint actorId)
    {
        throw new Exception($"Actor {actorId} don't have associated asset");
    }

    /// <summary>
    /// Генерирует исключение, когда пул компонентов уже сконфигурирован
    /// </summary>
    /// <param name="componentType"></param>
    /// <exception cref="Exception"></exception>
    [DoesNotReturn]
    public static void ComponentPoolAlreadyConfigured(Type componentType)
    {
        throw new Exception($"Actor component pool for {TypeOf.GetTypeName(componentType)} already configured");
    }


    /// <summary>
    /// Генерирует исключение, когда тип компонента актёра с указанным идентификатором не найден.
    /// </summary>
    /// <param name="id">Идентификатор типа компонента</param>
    [DoesNotReturn]
    public static void ComponentTypeNotFound(uint id)
    {
        throw new Exception($"Actor component type with id '{id}' isn't found");
    }

    /// <summary>
    /// Генерирует исключение, когда дочерний актёр уже добавлен к родительскому.
    /// </summary>
    /// <param name="parentId">Идентификатор родительского актёра</param>
    /// <param name="childId">Идентификатор дочернего актёра</param>
    /// <returns>Объект актёра (не используется, так как метод всегда выбрасывает исключение)</returns>
    [DoesNotReturn]
    public static Actor ChildAlreadyAdded(uint parentId, uint childId)
    {
        throw new Exception($"Child {childId} already has parent {parentId}");
    }

    /// <summary>
    /// Генерирует исключение, когда ограничение для указанного типа уже существует.
    /// </summary>
    /// <typeparam name="T">Тип, для которого установлено ограничение</typeparam>
    [DoesNotReturn]
    public static void ConstraintExists<T>()
    {
        throw new Exception($"Constraint for {TypeOf<T>.GetTypeName()} already exists");
    }

    /// <summary>
    /// Генерирует исключение, когда актёр уже имеет компонент указанного типа.
    /// </summary>
    /// <typeparam name="T">Тип компонента</typeparam>
    /// <param name="actorId">Идентификатор актёра</param>
    [DoesNotReturn]
    public static void ComponentExists<T>(uint actorId)
    {
        throw new Exception($"Actor {actorId} already has component {TypeOf<T>.GetTypeName()}");
    }

    /// <summary>
    /// Генерирует исключение, когда у актёра отсутствует компонент указанного типа.
    /// </summary>
    /// <typeparam name="T">Тип компонента</typeparam>
    /// <param name="actorId">Идентификатор актёра</param>
    [DoesNotReturn]
    public static void ComponentNotFound<T>(uint actorId)
    {
        throw new Exception($"Actor {actorId} don't have component {TypeOf<T>.GetTypeName()}");
    }

    /// <summary>
    /// Генерирует исключение при попытке обратиться к уничтоженному объекту.
    /// </summary>
    /// <param name="type">Тип уничтоженного объекта</param>
    [DoesNotReturn]
    public static void Disposed(Type type)
    {
        throw new ObjectDisposedException(TypeOf.GetTypeName(type));
    }

    /// <summary>
    /// Генерирует исключение, когда система отрисовки актёров указанного типа не найдена.
    /// </summary>
    /// <typeparam name="T">Тип системы актёров</typeparam>
    [DoesNotReturn]
    public static void DrawSystemNotFound<T>() where T : class, IDrawSystem
    {
        throw new Exception($"Draw system {TypeOf<T>.GetTypeName()} isn't found");
    }

    /// <summary>
    /// Генерирует исключение, когда система указанного типа не найдена.
    /// </summary>
    /// <typeparam name="T">Тип системы</typeparam>
    /// <returns>Объект типа T (не используется, так как метод всегда выбрасывает исключение)</returns>
    [DoesNotReturn]
    public static T QueueNotFound<T>()
    {
        throw new Exception($"System {TypeOf<T>.GetTypeName()} isn't found");
    }

    [DoesNotReturn]
    public static void InvalidId()
    {
        throw new Exception("Invalid actor id");
    }
    
    /// <summary>
    /// Генерирует исключение, когда актёр с указанным ключом не найден.
    /// </summary>
    [DoesNotReturn]
    public static void KeyNotFound()
    {
        throw new KeyNotFoundException("Actor with given key isn't found");
    }

    /// <summary>
    /// Генерирует исключение, когда актёр с указанным идентификатором не найден.
    /// </summary>
    /// <param name="actorId">Идентификатор актёра</param>
    [DoesNotReturn]
    public static void NotFound(uint actorId)
    {
        throw new Exception($"Actor with id {actorId} isn't found");
    }

    /// <summary>
    /// Генерирует исключение, когда актёр с указанным компонентом не найден.
    /// </summary>
    /// <typeparam name="T">Тип компонента</typeparam>
    [DoesNotReturn]
    public static void NotFound<T>()
    {
        throw new Exception($"Actor with component {TypeOf<T>.GetTypeName()} isn't found");
    }

    /// <summary>
    /// Генерирует исключение, когда система актёров указанного типа не найдена.
    /// </summary>
    /// <param name="componentType">Тип, который не является компонентом актёра</param>
    [DoesNotReturn]
    public static void NotComponentType(Type componentType)
    {
        throw new ArgumentException($"Type {TypeOf.GetTypeName(componentType)} isn't a actor component type");
    }

    /// <summary>
    /// Генерирует исключение, когда актёр с указанным компонентом не единственный.
    /// </summary>
    /// <typeparam name="T">Тип компонента</typeparam>
    [DoesNotReturn]
    public static void NotSingle<T>()
    {
        throw new Exception($"Actor with {TypeOf<T>.GetTypeName()} isn't single");
    }

    [DoesNotReturn]
    public static void ParallelWorkerNotRegistered()
    {
        throw new Exception("Parallel worker isn't registered");
    }

    [DoesNotReturn]
    public static void RelationAlreadyExists<T>(uint subject, uint relative)
    {
        throw new Exception($"Actor '{subject}' already has relation {TypeOf<T>.GetTypeName()} with '{relative}'");
    }

    /// <summary>
    /// Генерирует исключение, когда у актёра отсутствует связь указанного типа с другим актёром.
    /// </summary>
    /// <typeparam name="T">Тип связи</typeparam>
    /// <param name="subject">Идентификатор субъекта связи</param>
    /// <param name="relative">Идентификатор связанного актёра</param>
    [DoesNotReturn]
    public static void RelationNotFound<T>(uint subject, uint relative)
    {
        throw new Exception($"Actor {subject} don't have relation {TypeOf<T>.GetTypeName()} with {relative}");
    }

    /// <summary>
    /// Генерирует исключение, когда тип связи с указанным идентификатором не найден.
    /// </summary>
    /// <param name="id">Идентификатор типа связи</param>
    [DoesNotReturn]
    public static void RelationTypeNotFound(uint id)
    {
        throw new Exception($"Relation type with {id} isn't found");
    }

    /// <summary>
    /// Генерирует исключение, когда единственный актёр с указанным компонентом не найден.
    /// </summary>
    /// <typeparam name="T">Тип компонента</typeparam>
    [DoesNotReturn]
    public static void SingleNotFound<T>()
    {
        throw new Exception($"Single actor with component {TypeOf<T>.GetTypeName()} isn't found");
    }

    /// <summary>
    /// Генерирует исключение, когда система обновления актёров указанного типа не найдена.
    /// </summary>
    /// <typeparam name="T">Тип системы актёров</typeparam>
    [DoesNotReturn]
    public static void UpdateSystemNotFound<T>() where T : class, IUpdateSystem
    {
        throw new Exception($"Update system {TypeOf<T>.GetTypeName()} isn't found");
    }

    /// <summary>
    /// Генерирует исключение, когда значение с указанным именем и типом не найдено.
    /// </summary>
    /// <param name="name">Имя значения</param>
    /// <param name="type">Тип значения</param>
    [DoesNotReturn]
    public static void ValueNotFound(string name, Type type)
    {
        throw new Exception($"Value '{name}' (type {TypeOf.GetTypeName(type)}) isn't found");
    }
}