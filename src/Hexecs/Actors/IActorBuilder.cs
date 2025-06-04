using Hexecs.Assets;

namespace Hexecs.Actors;

/// <summary>
/// Интерфейс, определяющий базовый строитель актёров.
/// Отвечает за конструирование и настройку актёров на основе ассетов.
/// </summary>
public interface IActorBuilder
{
    /// <summary>
    /// Создаёт и настраивает актёра на основе переданного ассета.
    /// </summary>
    /// <param name="actor">Ссылка на актёра, который будет сконструирован.</param>
    /// <param name="asset">Ссылка на ассет, содержащий данные для конструирования актёра.</param>
    /// <param name="args">Дополнительные аргументы для процесса конструирования.</param>
    void Build(in Actor actor, in Asset asset, Args args);
}

/// <summary>
/// Типизированный интерфейс строителя актёров, специализированный для работы 
/// с определённым типом компонента ассета.
/// </summary>
/// <typeparam name="TAsset">Тип компонента ассета, который используется 
/// для построения актёра. Должен быть структурой и реализовывать интерфейс IAssetComponent.</typeparam>
public interface IActorBuilder<TAsset> : IActorBuilder
    where TAsset : struct, IAssetComponent
{
    /// <summary>
    /// Создаёт и настраивает актёра на основе типизированной ссылки на ассет.
    /// </summary>
    /// <param name="actor">Ссылка на актёра, который будет сконструирован.</param>
    /// <param name="asset">Типизированная ссылка на ассет, содержащий данные для конструирования актёра.</param>
    /// <param name="args">Дополнительные аргументы для процесса конструирования.</param>
    void Build(in Actor actor, in AssetRef<TAsset> asset, Args args);

    void IActorBuilder.Build(in Actor actor, in Asset asset, Args args)
    {
        if (asset.IsRef<TAsset>(out var expected))
        {
            Build(in actor, in expected, args);
        }
    }
}