using Hexecs.Pipelines;

namespace Hexecs.Tests.Mocks;

public readonly record struct MessageMock(int Value) : IMessage;

public readonly record struct MessageMockNotRegistered(int Value) : IMessage;