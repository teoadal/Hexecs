# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this project is

**Hexecs** — a high-performance, AOT-friendly Entity Component System framework for .NET, distributed as the [`Hexecs` NuGet package](https://www.nuget.org/packages/Hexecs). In addition to the ECS core, the library bundles a CQRS pipeline and a small DI container so that entity state can be driven through systems/messages rather than scattered mutations.

- Multi-targeted library: `net8.0;net9.0;net10.0` (see `src/Hexecs/Hexecs.csproj`).
- `IsAotCompatible=true`, `AllowUnsafeBlocks=true`, `WarningsAsErrors=true`, nullable enabled, LangVersion 14.
- `InternalsVisibleTo` is granted to `Hexecs.Tests` and `Hexecs.Benchmarks`.
- Package versions are centrally managed via `Directory.Packages.props`.
- The README is in Russian — examples there are the source of truth for public API shape.

## Solution layout

```
Hexecs.sln
src/
  Hexecs/                # main library (the published package)
  Hexecs.Tests/          # xUnit + FluentAssertions + Moq + AutoFixture, net10.0
  Hexecs.Benchmarks/         # BenchmarkDotNet
  Hexecs.Benchmarks.Noise/   # nested under "Benchmarks" solution folder
  Hexecs.Benchmarks.City/    # nested under "Benchmarks" solution folder
```

`Hexecs` is the main project of the solution. The benchmarks projects form a "Benchmarks" solution folder; they share `BenchmarkDotNet` via `Directory.Build.props` and are excluded from code coverage.

## Build, test, lint

All commands are run from the repository root unless noted.

```bash
# Build the library (multi-targeted: net8.0, net9.0, net10.0)
dotnet build src/Hexecs/Hexecs.csproj -c Release

# Build everything in the solution
dotnet build -c Release

# Run the full test suite (xUnit)
dotnet test src/Hexecs.Tests/Hexecs.Tests.csproj -c Release

# Run a single test by fully qualified name
dotnet test src/Hexecs.Tests/Hexecs.Tests.csproj -c Release \
  --filter "FullyQualifiedName~Hexecs.Tests.Collections.BucketShould.Has_WhenEmpty_ShouldReturnFalse"

# Run a whole test class
dotnet test src/Hexecs.Tests/Hexecs.Tests.csproj -c Release \
  --filter "FullyQualifiedName~Hexecs.Tests.Collections.BucketShould"

# Tests with coverage (mirrors CI: opencover, excluding tests/benchmarks/demos)
cd src/Hexecs.Tests
dotnet test -c Release --no-build \
  /p:CollectCoverage=true \
  /p:CoverletOutput=TestResults/ \
  /p:CoverletOutputFormat=opencover \
  /p:Exclude="[*Tests*]*|[*Benchmarks*]*|[*Demo*]*"
```

CI is defined in `.github/workflows/dotnet.yaml` and builds+runs tests on a matrix of `{ubuntu, windows, macos} × {8.0.x, 9.0.x, 10.0.x}`. Codecov upload runs only on `ubuntu-latest` / `10.0.x`. There is no separate linter step — `WarningsAsErrors` in `Directory.Build.props` is the enforced bar.

The test project uses `BaseFixture` (in `src/Hexecs.Tests/BaseFixture.cs`) for shared `Random` / `Fixture` / array helpers; subclasses inherit from it.

## High-level architecture

A `World` (built by `WorldBuilder`, see `src/Hexecs/Worlds/`) is the root container. It owns:

- **`AssetContext`** — immutable, resource-like entities ("assets") with their own `AssetFilter1/2/3`. Created at world-build time from `IAssetSource` registrations; cannot be created at runtime.
- One or more **`ActorContext`** instances (the default one plus any created via `World.CreateActorContext`). Each has its own set of `Actor` entities, filters, systems, and a small DI scope.
- A **`DependencyProvider`** wired by the `WorldBuilder` (Singleton/Scoped/Transient via `UseSingleton` / `UseScoped` / `UseTransient`). The `World` itself registers `World`, `AssetContext`, `ActorContext`, `LogService`, `ConfigurationService`, `ValueService`, `Dice` as services.
- A **`LogService`**, **`ConfigurationService`**, **`ValueService`**, and **`Dice`** RNG — all configured on the `WorldBuilder`.

### Actor/Component model (the ECS core)

- A `Component` is a `struct` implementing `IActorComponent` (or `IAssetComponent` for assets). Using a struct is required for performance — the README states this explicitly.
- An `Actor` (a value type wrapping an `ActorId` + `ActorContext` reference) is created via `world.Actors.CreateActor()`. `Actor<T>` is the strongly-typed wrapper that guarantees a component is present, useful for message handlers.
- Components are added/removed/updated through `ActorContext` (the actual store). Mutations to a component made through a `ref var` are not observed; `actor.Update(component)` is the API to publish a change.
- Children: `ActorContext.AddChild` links actors; each child is associated with a `BuildActor(asset, args)` call.

### Component storage & filters

- Each component type lives in its own `ActorComponentPool` (see `src/Hexecs/Actors/Components/`). The pool is per-`ActorContext`, registered via `ActorContextBuilder` (typically a partial class generated alongside `ActorComponentConfiguration<T>`).
- `ActorFilter1` / `ActorFilter2` / `ActorFilter3` are the three-arity query types (one, two, or three component constraints). They are the only public filter shapes — systems and constraints are written against them. Each has `Dictionary` / `Enumerator` / `Operation` / `SkipTakeEnumerator` partial files. `ActorConstraint` and `AssetConstraint` are builder types for dynamic filters.
- AOT-friendly enumerators live in `src/Hexecs/Collections/` (`ArrayEnumerator`, `Bucket`, `Block`, `InlineBucket`, `ThreadLocalStack`); `ArrayUtils` (`src/Hexecs/Utils/`) handles zero-init array allocation with primitive alignment hints.

### Systems & parallel execution

- `IUpdateSystem` and `IDrawSystem` are the two system interfaces. The concrete `UpdateSystem1/2/3` and `DrawSystem1/2/3` (and `UpdateSystem`) live in `src/Hexecs/Actors/Systems/`.
- `ParallelSystem` runs a list of `IUpdateSystem` over a worker (see `IParallelWorker` / `DefaultParallelWorker` in `src/Hexecs/Threading/`). The default worker is configured on `WorldBuilder.UseDefaultParallelWorker` and resolved through DI inside the actor context.
- The `World.Update` / `World.Draw` cycle swaps `WorldState` atomically (`None` ↔ `Update`/`Draw`) using `Interlocked.CompareExchange`, so re-entering update from draw (or vice versa) throws via `WorldError.InvalidState`.

### CQRS / Pipelines

`src/Hexecs/Pipelines/` implements a small CQRS layer built around `ICommand` / `IQuery` / `IMessage` / `INotification` plus their `*Handler` counterparts and a `Result` value type. Each handler is a method-level generic with a delegate-based registration so it works under AOT. `Actors/Pipelines/` provides the actor-specific convenience handlers (`ActorCommandHandler`, `ActorMessageHandler`, `ActorNotificationHandler`, `ActorQueryHandler`) that bridge the pipeline to a specific actor context.

### Source layout convention: partial classes

Many "big" types are split into partial files across one folder rather than nested folders. Examples: `ActorContext` (≈20 partials under `Actors/`), `AssetContext`, `ActorFilter1/2/3`, `AssetFilter1/2/3`, `ActorComponentPool`, `WorldBuilder`. The `DependentUpon` entries in `src/Hexecs/Hexecs.csproj` keep them grouped under their "main" file in IDEs. **When adding a new method to one of these types, add it as another partial in the same folder — not a new file under a subfolder.**

### Asset loading

Assets are populated at world build via `IAssetSource` (`UseAddAssetSource` / `CreateAssetSource` / `CreateAssetData`). The `AssetContext.Loader.cs` partial is the entry point. Asset sources have an `Order` (`OrderComparer<IAssetSource>`) so they load deterministically. There's an `AssetContext.Sources` folder plus a `Development` folder for dev-mode helpers.

### AOT and unsafe code

- Many hot-path methods are annotated `[MethodImpl(MethodImplOptions.AggressiveInlining)]` and use `Unsafe.As<,>` for `Interlocked` on enums (see `World.Update`/`World.Draw`).
- `Utils/ArrayUtils.Create<T>` prefers `GC.AllocateUninitializedArray<T>` and aligns primitive arrays to 16 bytes on 64-bit — don't replace this with `new T[]` (it zero-initializes).
- No reflection-based assembly scanning; everything (components, asset sources, system registrations, pipeline handlers) is registered explicitly. Adding a new component means registering its pool in `ActorContextBuilder` and its builder if you want `BuildActor(asset, args)` to populate it.

## Common workflows when editing

- **Add a new component type:** declare the `struct : IActorComponent`; register the pool in the `ActorContextBuilder` configuration; add tests under `src/Hexecs.Tests/Actors/Components/` or extend an existing `*Should.cs`.
- **Add a new filter arity:** not supported — the library deliberately stops at 3-arity. Use `ActorConstraint` (dynamic) instead.
- **Add a new system:** implement `IUpdateSystem` (or `IDrawSystem`); register it in `ActorContextBuilder.AddSystem<T>()` or via `ParallelSystemBuilder` for parallel execution.
- **Add a new pipeline handler:** implement the corresponding `*Handler` interface, then register it on the relevant `ActorContext`.
- **Add a new collection type:** place it in `src/Hexecs/Collections/`; mirror the `*Should.cs` test pattern with FluentAssertions.
- **Change public API:** any new public type/member should have an XML doc comment (warnings-as-errors + `<GenerateDocumentationFile>true</GenerateDocumentationFile>` enforce this); the repo's existing comments are in Russian — match the surrounding language unless the change is purely internal.

## Project conventions worth respecting

- Hot paths use `AggressiveInlining`; mirror that style for any small method called from a tight loop.
- `array.Should().BeTrue()` style with `FluentAssertions` is the test assertion standard.
- The test project targets only `net10.0`; library code is what is multi-targeted.
- Don't introduce reflection or `dynamic` — AOT compatibility is a first-class requirement.
- `Hexecs.sln.DotSettings.user` is a Rider/ReSharper local user file; ignore.
