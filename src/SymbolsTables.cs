using UnityEngine;
using System.Collections.Generic;

public class SymbolsTables : MonoBehaviour
{
    public static Dictionary<string, TransformFunction> m_TransformFunctionTable;

    public delegate Transform TransformFunction(int x, int y, int z, Transform transform);

    public Transform MoveTransform(int x, int y, int z, Transform transform)
    {
        transform.Translate(new Vector3(x, y, z));
        return transform;
    }

    public Transform RotateTransform(int x, int y, int z, Transform transform)
    {
        Vector3 localEulerAngles = transform.localEulerAngles;
        localEulerAngles += new Vector3(x, y, z);
        transform.localEulerAngles = localEulerAngles;
        return transform;
    }

    public Transform ScaleTransform(int x, int y, int z, Transform transform)
    {
        transform.localScale += new Vector3(x, y, z);
        return transform;
    }

    void Awake()
    {
        m_TransformFunctionTable = new Dictionary<string, TransformFunction>();
        m_TransformFunctionTable.Add("MOV", MoveTransform);
        m_TransformFunctionTable.Add("ROT", RotateTransform);
        m_TransformFunctionTable.Add("SCL", ScaleTransform);
    }
};