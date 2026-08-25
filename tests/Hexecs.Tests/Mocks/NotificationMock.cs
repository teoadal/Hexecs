using Hexecs.Pipelines;

namespace Hexecs.Tests.Mocks;

public readonly record struct NotificationMock(int Value) : INotification;

public readonly record struct NotificationMockNotRegistered(int Value) : INotification;