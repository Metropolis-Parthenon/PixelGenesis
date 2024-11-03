﻿using PixelGenesis.ECS;

namespace PixelGenesis._3D.Common;

public sealed class PhysicMaterial : IWritableAsset, IReadableAsset
{
    public string Reference { get; }

    public float DynamicFriction { get; }
    public float StaticFriction { get; }
    public float Bounciness { get; }
    public CombineMode FrictionCombine { get; }
    public CombineMode BounceCombine { get; }

    private PhysicMaterial(string reference, float dynamicFriction, float staticFriction, float bounciness, CombineMode frictionCombine, CombineMode bounceCombine)
    {
        Reference = reference;
        DynamicFriction = dynamicFriction;
        StaticFriction = staticFriction;
        Bounciness = bounciness;
        FrictionCombine = frictionCombine;
        BounceCombine = bounceCombine;
    }

    public void WriteToStream(Stream stream)
    {
        using var bw = new BinaryWriter(stream);
        bw.Write(DynamicFriction);
        bw.Write(StaticFriction);
        bw.Write(Bounciness);
        bw.Write((int)FrictionCombine);
        bw.Write((int)BounceCombine);
    }

    public class Factory : IReadableAssetFactory<PhysicMaterial>
    {
        public PhysicMaterial ReadAsset(string reference, Stream stream)
        {
            using var br = new BinaryReader(stream);
            var dynamicFriction = br.ReadSingle();
            var staticFriction = br.ReadSingle();
            var bounciness = br.ReadSingle();
            var frictionCombine = (CombineMode)br.ReadInt32();
            var bounceCombine = (CombineMode)br.ReadInt32();

            return new PhysicMaterial(reference, dynamicFriction, staticFriction, bounciness, frictionCombine, bounceCombine);
        }
    }
}

public enum CombineMode
{
    Average,
    Minimum,
    Maximum,
    Multiply
}