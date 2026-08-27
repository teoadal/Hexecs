namespace Hexecs.Actors.Nodes;

[DebuggerDisplay("{Id}")]
internal sealed class ActorNode(uint id)
{
    public readonly uint Id = id;

    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    public ActorNode? Parent
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _parent;
    }

    private ActorNode[]? _children;
    private ActorNode? _parent;
    private int _length;

    public void AddChild(ActorNode child)
    {
        ActorNode? existsParent = child._parent;

        if (existsParent == this)
        {
            ActorError.ChildAlreadyAdded(Id, child.Id);
        }

        if (existsParent != null && ArrayUtils.Remove(existsParent._children, this))
        {
            existsParent._length--;
        }

        ArrayUtils.InsertOrCreate(ref _children, ArrayPool<ActorNode>.Shared, _length, child);

        child._parent = this;

        _length++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<ActorNode> AsSpan()
    {
        return _children == null
            ? Span<ActorNode>.Empty
            : _children.AsSpan(0, _length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<ActorNode> AsReadOnlySpan()
    {
        return _children == null
            ? ReadOnlySpan<ActorNode>.Empty
            : _children.AsSpan(0, _length);
    }

    public void Dispose()
    {
        if (_parent != null && ArrayUtils.Remove(_parent._children, this))
        {
            _parent._length--;
        }

        if (_children == null)
        {
            return;
        }

        foreach (ActorNode child in AsSpan())
        {
            child._parent = null;
        }

        ArrayPool<ActorNode>.Shared.Return(_children, true);

        _children = null;
        _length = 0;
    }

    public bool HasChild(uint childId)
    {
        if (_children == null)
        {
            return false;
        }

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (ActorNode childNode in AsReadOnlySpan())
        {
            if (childNode.Id == childId)
            {
                return true;
            }
        }

        return false;
    }

    public bool RemoveChild(uint childId)
    {
        if (_children == null || _length == 0)
        {
            return false;
        }

        Span<ActorNode> children = AsSpan();

        for (var i = 0; i < children.Length; i++)
        {
            ActorNode childNode = _children[i];

            if (childNode.Id != childId)
            {
                continue;
            }

            ArrayUtils.Cut(_children, i);

            childNode._parent = null;

            break;
        }

        _length--;

        // ReSharper disable once InvertIf
        if (_length == 0)
        {
            ArrayPool<ActorNode>.Shared.Return(_children, true);
            _children = null;
        }

        return true;
    }
}
