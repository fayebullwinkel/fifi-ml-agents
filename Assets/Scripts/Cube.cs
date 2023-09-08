using UnityEngine;

public struct Cube
{
    private float _weight;
    private bool _isWall;
    private bool _isDeletable;
    private GameObject _cubeObj;
    private int _x;
    private int _y;
    private int _z;

    public Vector3 GetCubePosition(Vector3 localScale)
    {
        return new Vector3(
                (_x + 0.5f - (localScale.x / 2)) / localScale.x,
                (_y + 0.5f - (localScale.y / 2)) / localScale.y,
                (_z + 0.5f - (localScale.z / 2)) / localScale.z
            );
    }
    public void Generate(GameObject parent)
    {
        _cubeObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _cubeObj.transform.parent = parent.transform;
        _cubeObj.transform.localPosition = GetCubePosition(parent.transform.localScale);
        
        _cubeObj.GetComponent<Renderer>().material = Resources.Load("Materials/Brick_Wall", typeof(Material)) as Material;
    }
    public float GetWeight()
    {
        return _weight;
    }
    public void SetWeight(float weight)
    {
        this._weight = weight;
    }
    public bool GetIsWall()
    {
        return _isWall;
    }
    public void SetIsWall(bool isWall)
    {
        _isWall = isWall;
    }
    public bool GetIsDeletable()
    {
        return _isDeletable;
    }
    public void SetIsDeletable(bool toDelete)
    {
        _isDeletable = toDelete;
    }
    public int GetX()
    {
        return _x;
    }
    public int GetY()
    {
        return _y;
    }
    public int GetZ()
    {
        return _z;
    }
    public void SetPos(int x, int y, int z)
    {
        _x = x;
        _y = y;
        _z = z;
    }
    
    public Vector3 GetPos()
    {
        return new Vector3(_x, _y, _z);
    }
}
