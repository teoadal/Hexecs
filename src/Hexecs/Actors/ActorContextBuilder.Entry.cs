namespace Hexecs.Actors;

public sealed partial class ActorContextBuilder
{
    internal readonly struct Entry<TResult>
        where TResult : class
    {
        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _builder == null && _instance == null;
        }

        private readonly Func<ActorContext, TResult> _builder;
        private readonly TResult? _instance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entry(TResult instance)
        {
            _builder = null!;
            _instance = instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entry(Func<ActorContext, TResult> builder)
        {
            _builder = builder;
            _instance = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TResult Invoke(ActorContext context) => _instance ?? _builder(context);
    }
}