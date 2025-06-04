using Hexecs.Pipelines;

namespace Hexecs.Tests.Mocks;

public readonly record struct QueryMock(int Value) : IQuery<PipelineResult>;

public readonly record struct QueryMockNotRegistered(int Value) : IQuery<PipelineResult>;