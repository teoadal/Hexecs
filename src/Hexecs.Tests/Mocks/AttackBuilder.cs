﻿using Hexecs.Actors;
using Hexecs.Assets;
using Hexecs.Utils;

namespace Hexecs.Tests.Mocks;

internal sealed class AttackBuilder : IActorBuilder<UnitAsset>
{
    public void Build(in Actor actor, in AssetRef<UnitAsset> asset, Args args)
    {
        actor.Add(new Attack { Value = asset.Component1.Attack });
    }
}