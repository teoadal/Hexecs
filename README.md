[![.NET Core](https://github.com/teoadal/Hexecs/actions/workflows/dotnet.yaml/badge.svg?branch=master)](https://github.com/teoadal/Hexecs/actions/workflows/dotnet.yaml)
[![NuGet](https://img.shields.io/nuget/v/Hexecs.svg)](https://www.nuget.org/packages/Hexecs)
[![NuGet](https://img.shields.io/nuget/dt/Hexecs.svg)](https://www.nuget.org/packages/Hexecs)
[![codecov](https://codecov.io/gh/teoadal/Hexecs/branch/master/graph/badge.svg?token=8L4HN9FAIV)](https://codecov.io/gh/teoadal/Hexecs)
[![CodeFactor](https://www.codefactor.io/repository/github/teoadal/Hexecs/badge)](https://www.codefactor.io/repository/github/teoadal/Hexecs)

# HexECS

Привет! Это простой движок игровой логики, имплементирующий ECS. Тут ты можешь делать многое, к чему и так привык.
При этом, ты можешь чуть больше, чем обычные ECS, так как помимо ECS в библиотеке есть встроенный CQRS и DI.
CQRS в связке с ECS представляется мне максимально подходящей штукой для управления состоянием сущностей через системы.
DI необходим для того, чтобы всё это работало удобно.

## Базовые возможности

Так как всё в этом мире завязано на "мир" (`World`), то мы начнём с его создания.
```csharp
var world = new WorldBuilder()
    .DebugWorld() // чтобы получать больше информации в Debug-режиме 
    .CreateLogger(logger => logger.UseConsoleSink()); // чтобы логи выводились в консоль
    .Build();    
```
Этот код создаёт сконфигурированный объект World. 
Так как библиотека AOT-friendly, тут нет сканеров assembly для регистрации отдельных компонентов. 
То есть всё нужно регистрировать вручную.

### Создание компонента

Компонент - это один из базовых кирпичиков ECS.
Для того, чтобы системы могли наполнить сущность жизнью, сущность должна обладать признаками - компоненты и есть эти признаки.

```csharp
public struct FlyComponent(int current, int max) : IActorComponent { // необходимо указать маркерный интерфейс
    public int CurrentSpeed;
    public int MaxSpeed;
}
```
Компоненты должны быть `struct`, чтобы всё в вашем мире могло работать эффективно и быстро.
Например, обрабатывать в системах или обработчиках шины сообщений.
В будущем мы будем получать эти структуры по ссылке `ref struct`, что позволяет неплохо увевличивать скорость работы.

### Создание сущности

В этом мире есть две сущности - Actor'ы и Asset'ы.
- **Актёры** - это динамичные изменяемые сущности, которые могут создаваться и удаляться, и которым можно добавлять и удалять компоненты.
- **Ассеты** - это сущности, которые нельзя изменить, представляющие из себя настройки мира, но которые также имеют компоненты.
Единственное отличие в том, что их нельзя создавать в процессе работы, так как они, по задумке, представляют из себя ресурсы, на основе которых будут создаваться актёры.

Создание ассетов мы рассмотрим отдельно.
Пока просто представим, что они у нас есть.

Создать актёра можно следующим образом:
```csharp
ActorContext actorContext = world.Actors; // контекст актёров по-умолчанию.
Actor actor = actorContext.CreateActor(); // создаём
actor.Add(new FlyComponent(10, 20)); // добавляем компонент
Actor<FlyComponent> typedActor = actor.As<FlyComponent>();
```

В последней строчке был создан т.н. "типизированный актёр" или актёр, в котором точно есть летающий компонент.
Эта щепотка типизации очень важна в случаях, когда нам хотелось бы знать заранее - действительно ли актёр содержит нужный компонент. 
Например, в шине сообщений.

Структуры `Actor` и `Actor<T>` предоставляют удобную обёртку для взаимодействия с актёром. 
Актёр всегда знает к какому `ActorContext` он принадлежит и просто проксирует методы к контексту, чтобы программисту было проще.
Если мы посмотрим на код, то можем увидеть и такие экзотические типы как `ActorRef<T>`, но пока забудем о них, они нужны для высокопроизводительных сценариев.

Кстати, более привычным типом entity для классических ECS будет являться содержимое поля `Actor.Id`, которое содержит в себе простой идентификатор актёра типа `unit`. 
Если вам удобнее работать с ними, можно использовать их, но лучше сделать так:

```csharp
ActorId actorId = typedActor;
ActorId<FlyComponent> typedId = typedActor; // или даже вот так
```

Таким образом у нас будет типизированный идентификатор, который, в случае чего, можно превратить в `Actor` или `Actor<T>`:

```csharp
ActorContext actorContext = world.Actors; // это контекст актёров по-умолчанию 
Actor actor = actorId.Unwrap(actorContext);
Actor<FlyComponent> typedActor = typedId.Unwrap(actorContext);
```
Обратим внимание, что методу `Unwrap` нужен контекст актёров. 
Во-первых, для того, чтобы убедиться, что актёр существует, а во-вторых, чтобы привязаться к конкретному контексту, так как их (контекстов) может быть несколько.

### Добавление компонентов

Созданный актёр может быть создан с использованием метода `world.Actors.Create()`.
В этом случае он не будет обладать компонентом `FlyComponent`. Если мы не добавили его ранее, то добавим его:
```csharp
actor.Add(new FlyComponent { CurrentSpeed = 5, MaxSpeed = 7 });
```
То, что компонент теперь принадлежит актёру можно легко проверить с помощью метода:
```csharp
actor.Has<FlyComponent>(); // it's true!
```

Поздравляю, мы создали компонент и теперь можем его редактировать.
Для этого достаточно получить ссылку на компонент и отредактировать его.
```csharp
ref var component = ref actor.Get<FlyComponent>();
component.Current = 15;
```

Однако, в данном случае никто никогда не узнает о том, что было что-то изменено. 
Иногда это важно, например, чтобы держать кэш позиций некоторых актёров.

Чтобы все точно узнали, что были изменения, нужно использовать специальный метод:

```csharp
var component = actor.Get<FlyComponent>(); // копируем компонент
component.Current = 15; // изменяем значение копии
actor.Update(component); // в этот момент все узнают, что что-то изменилось
```

### Удаление компонентов

Тут всё тоже предельно просто. Представим, что у нас есть компонент `Pilot` (на самом деле структура `PilotComponent : IActorComponent`), который мы уже добавили к актёру-самолёту.
После того как пилот захотел спать, он покинет самолёт следующим способом:
```csharp
actor.Remove<PilotComponent>();
```
Если пилота там не было, метод вернёт `false` и мы сможем обработать это вопиющее нарушение правил полётов.
Если компонент был, то метод вернёт `true`, а значит мы можем быть уверенными, что пассажиры в безопасности.

## Дочерние элементы

Пассажиры! Их тоже можно создать примерно так:

```csharp
ActorContext actorContext = world.Actors;
Asset passengerAsset = world.Assets.GetAsset<PassengerAsset>(); 
Actor passenger = .BuildActor(passengerAsset, Arg.Rent("plane", planeActor))
```

Мы находим ассет пассажира (вспомним, что у него тоже есть компоненты). 
Допустим, что ассет пассажира всего один. 
Если их несколько, можно поискать его по алиасу или идентификатору: 

```csharp
AssetContext assetContext = world.Assets;
assetContext.GetAsset<PassengerAsset>("simle_passenger") // по алиасу
assetContext.GetAsset<PassengerAsset>(123) // по идентификатору
```

На второй строке, с помощью зарегистрированных `IActorBuilder` (имплементации "строителей" актёров по компонентам ассета), буквательно создаём актёра.
Кстати, если нам будет нужно, мы сможем проверить, по какому ассету был построен актёр, запросив `actor.TryGetAsset`.

После создания пассажиров, мы можем в прямом смысле посадить их на самолёт.
Для этого нужно воспользоваться следующим методом:

```csharp
Actor<FlyComponent> plane = ...; 
Actor passenger = ...;
plane.AddChild(passenger);
```

Если повторить это много раз, то в самолёте будут сидеть 2-400 пассажиров (в зависимости от типа самолёта, конечно).
Получить всех пассажиров можно вот так:

```csharp
Actor plane = ...; 
foreach(Actor passanger in plane.Children(passenger)) 
{
    // do something
}
```

## Системы

Системы это важный элемент ECS. 
Системы регистрируются при создании контекста актёров и работают с ним и только с ним. 
Необходимо помнить, что контексты акёров могут быть разными, но контекст ассетов всегда один.

Сейчас мы зарегистрируем систему при создании мира, так как у нас всего один контекст актёров.

```csharp
var world = new WorldBuilder()
    .DebugWorld() // чтобы получать больше информации в Debug-режиме 
    .CreateLogger(logger => logger.UseConsoleSink()); // чтобы логи выводились в консоль
    .DefaultActorContext(defaultContext => defaultContext
        .AddUpdateSystem(new MyUpdateSystem())
        .CreateUpdateSystem(serviceProvider => new FlySystem<FlyComponent>(serviceProvider.GetRequiredService<ActorContext>)))
    .Build(); 
```

Можно обратить внимание на то, что в коде выше есть два похода к регистрации системы: добавление инстанса или созданеие через DI. 
Второй подход предпочтительнее, но сложнее в написании. 
Сложность состоит в том, что мы не можем довериться Reflection для создания системы, так как библиотека является `AOT-ready`, что запрещает некоторые трюки с созданием инстансов объектов по сигнатуре конструктора. Да, это можно обойти, но в настоящий момент этот функционал не реализован.

Код самой системы будет простой. 
Нам нужны все не сломанные самолёты с пилотами, которые умеют летать.
Мы создаём систему, которая обновляет состояние аткёра, а не рисует его (да, ещё есть системы `IDrawSystem`).

```csharp
internal sealed class PlaneSystem: UpdateSystem<FlyComponent, PilotComponent>
{
    public LandSystem(ActorContext context) 
        : base(context, static constraint => constraint.Exclude<BrokenComponent>()) // исключаем сломанные самолёты
    {
    }

    protected override void Update(
        in ActorRef<FlyComponent, PilotComponent> actor,  // ссылка на актёра и его компоненты
        in WorldTime time) // время мира
    {
        ref FlyComponent fly = ref actor.Component1;
        fly.CurrentSpeed++;
        
        ref PilotComponent pilot = ref actor.Component2;
        pilot.Panic--;
    }
}
```

Для обновления мира нужно просто обновить его:
```csharp
world.Update();
```

При вызове этого метода будут вызваны все методы обновления у всех контекстов актёров и, соответственно, у всех зарегистрированных систем обновления (`IUpdateSystem`).

