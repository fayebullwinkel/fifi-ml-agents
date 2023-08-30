using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube: ScriptableObject //TODO: check heredity
{
    private float weight;
    private bool isCarveable;
    private bool isDeletable;
    private GameObject CubeObj;
    private int x;
    private int y;
    private int z;

    public Vector3 GetCubePosition(Vector3 localScale)
    {
        return new Vector3(
                (x + 0.5f - (localScale.x / 2)) / localScale.x,
                (y + 0.5f - (localScale.y / 2)) / localScale.y,
                (z + 0.5f - (localScale.z / 2)) / localScale.z
            );
    }
    public void Generate(GameObject Parent)
    {
        CubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        CubeObj.transform.parent = Parent.transform;
        CubeObj.transform.localPosition = GetCubePosition(Parent.transform.localScale);
    
        if(isCarveable)
        {
            CubeObj.GetComponent<Renderer>().material.color = Color.red;
        }
    }
    public float GetWeight()
    {
        return weight;
    }
    public void SetWeight(float weight)
    {
        this.weight = weight;
    }
    public bool GetIsCarveable()
    {
        return isCarveable;
    }
    public void SetIsCarveable(bool isCarveable)
    {
        this.isCarveable = isCarveable;
    }
    public bool GetIsDeletable()
    {
        return isDeletable;
    }
    public void SetIsDeletable(bool toDelete)
    {
        this.isDeletable = toDelete;
    }
    public int GetX()
    {
        return x;
    }
    public int GetY()
    {
        return y;
    }
    public int GetZ()
    {
        return z;
    }
    public void SetPos(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}
