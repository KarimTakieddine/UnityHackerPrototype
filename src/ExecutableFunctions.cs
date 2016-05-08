using UnityEngine;
using System.Collections;

public static class ExecutableFunctions
{
    public static Transform move(int x, int y, int z, Transform target)
    {
        target.Translate(new Vector3(x, y, z));
        return target;
    }

    public static Transform rotate(int x, int y, int z, Transform target)
    {
        target.localRotation = Quaternion.Euler(new Vector3(x, y, z));
        return target;
    }

    public delegate Transform TransformVectorFunc(int x, int y, int z, Transform target);
};