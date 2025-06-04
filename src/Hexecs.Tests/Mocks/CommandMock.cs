using Hexecs.Pipelines;

namespace Hexecs.Tests.Mocks;

public readonly record struct CommandMock(int Value) : ICommand;

public readonly record struct CommandMockWithResult(int Value) : ICommand<PipelineResult>;