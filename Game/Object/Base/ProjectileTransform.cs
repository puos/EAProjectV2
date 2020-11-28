using UnityEngine;

public abstract class ProjectileTransform : MonoBehaviour
{
    public static ProjectileTransform instance = null;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
}
