using Hexecs.Actors.Components;

namespace Hexecs.Actors;

public sealed partial class ActorConstraint
{
    /// <summary>
    /// Внутренняя структура, представляющая подписку на события пула компонентов.
    /// Используется для отслеживания изменений в компонентах актёров 
    /// и применения ограничений в реальном времени.
    /// </summary>
    private readonly struct Subscription : IComparable<Subscription>, IEquatable<Subscription>
    {
        /// <summary>
        /// Идентификатор типа компонента, связанного с подпиской.
        /// </summary>
        public ushort ComponentId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _pool.Id;
        }

        /// <summary>
        /// Функция проверки актёра на соответствие ограничению.
        /// </summary>
        public readonly Func<uint, bool> Check;

        /// <summary>
        /// Флаг, указывающий, является ли подписка включающей (true) или исключающей (false).
        /// </summary>
        private readonly bool _include;
        
        /// <summary>
        /// Пул компонентов, на события которого подписывается ограничение.
        /// </summary>
        private readonly IActorComponentPool _pool;

        /// <summary>
        /// Создает новую подписку для ограничения актёров.
        /// </summary>
        /// <param name="include">Флаг включения/исключения компонента</param>
        /// <param name="pool">Пул компонентов для подписки</param>
        /// <param name="check">Функция проверки актёра</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Subscription(bool include, IActorComponentPool pool, Func<uint, bool> check)
        {
            Check = check;

            _include = include;
            _pool = pool;
        }

        /// <summary>
        /// Подписывает ограничение на события пула компонентов.
        /// Настраивает обработчики событий в зависимости от типа ограничения (включение/исключение).
        /// </summary>
        /// <param name="constraint">Ограничение актёров для подписки</param>
        public void Subscribe(ActorConstraint constraint)
        {
            if (_include)
            {
                _pool.Added += constraint.OnInclude;
                _pool.Removing += constraint.OnExclude;
            }
            else
            {
                _pool.Added += constraint.OnExclude;
                _pool.Removing += constraint.OnInclude;
            }
        }

        /// <summary>
        /// Отписывает ограничение от событий пула компонентов.
        /// </summary>
        /// <param name="constraint">Ограничение актёров для отписки</param>
        public void Unsubscribe(ActorConstraint constraint)
        {
            if (_include)
            {
                _pool.Added -= constraint.OnInclude;
                _pool.Removing -= constraint.OnExclude;
            }
            else
            {
                _pool.Added -= constraint.OnExclude;
                _pool.Removing -= constraint.OnInclude;
            }
        }

        #region Equality

        /// <summary>
        /// Сравнивает текущую подписку с другой подпиской для сортировки.
        /// Сначала сравниваются идентификаторы компонентов, затем типы ограничений.
        /// </summary>
        /// <param name="other">Подписка для сравнения</param>
        /// <returns>Результат сравнения</returns>
        public int CompareTo(Subscription other)
        {
            var componentIdComparison = _pool.Id.CompareTo(other._pool.Id);
            return componentIdComparison != 0
                ? componentIdComparison
                : _include.CompareTo(other._include);
        }

        /// <summary>
        /// Определяет, равна ли текущая подписка другой подписке.
        /// </summary>
        /// <param name="other">Подписка для сравнения</param>
        /// <returns>Возвращает true, если подписки равны; иначе false</returns>
        public bool Equals(Subscription other) => _include == other._include && _pool.Id.Equals(other._pool.Id);

        /// <summary>
        /// Определяет, равен ли указанный объект текущей подписке.
        /// </summary>
        /// <param name="obj">Объект для сравнения</param>
        /// <returns>Возвращает true, если объекты равны; иначе false</returns>
        public override bool Equals(object? obj) => obj is Subscription other && Equals(other);

        /// <summary>
        /// Возвращает хеш-код для текущей подписки.
        /// </summary>
        /// <returns>Хеш-код</returns>
        public override int GetHashCode() => HashCode.Combine(Check, _include ? 2 : 3);

        #endregion
    }
}