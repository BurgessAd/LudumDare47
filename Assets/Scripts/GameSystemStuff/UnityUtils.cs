using UnityEngine;

public static class UnityUtils
{
    public static bool IsLayerInMask(in LayerMask mask, in int layer) 
    {
        return (mask == (mask | (1 << layer)));
    }
}
