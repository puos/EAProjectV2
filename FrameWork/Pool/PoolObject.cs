using UnityEngine;

public class PoolObject : MonoBehaviour
{
    public CObjectPoolWrapper<GameObject> obj = null;
    public int hashKey;
}