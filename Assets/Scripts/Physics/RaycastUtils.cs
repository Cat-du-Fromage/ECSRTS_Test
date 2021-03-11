/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Physics;
using Unity.Entities;
using Unity.Mathematics;
using Itinero.Algorithms.Collections;
using Unity.Collections;

public class RaycastUtils
{
    [ReadOnly] public BitArray32 BitArray;
    public static CollisionFilter LayerMaskToFilter(LayerMask mask)
    {
        CollisionFilter filter = new CollisionFilter()
        {
            BelongsTo = (uint)mask.value,
            CollidesWith = (uint)mask.value
        };
        return filter;
    }

    public static CollisionFilter LayerToFilter(int layer)
    {
        if (layer == -1)
        {
            return CollisionFilter.Zero;
        }

        BitArray32 mask = new BitArray32(5);
        mask[layer] = true;

        CollisionFilter filter = new CollisionFilter()
        {
            BelongsTo = mask.Bits,
            CollidesWith = mask.Bits
        };
        return filter;
    }

    private enum CollisionLayer
    {
        Solid = 1 << 0,
        Character = 1 << 1,
        Item = 1 << 2,
        ItemTrigger = 1 << 3
    }

    private static CollisionFilter
        filterSolid = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Solid),
            CollidesWith = (uint)(CollisionLayer.Character | CollisionLayer.Item)
        },
        filterCharacter = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Character),
            CollidesWith = (uint)(CollisionLayer.Solid | CollisionLayer.ItemTrigger)
        },
        filterItem = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.Item),
            CollidesWith = (uint)(CollisionLayer.Solid)
        },
        filterItemTrigger = new CollisionFilter()
        {
            BelongsTo = (uint)(CollisionLayer.ItemTrigger),
            CollidesWith = (uint)(CollisionLayer.Character)
        };
}
*/