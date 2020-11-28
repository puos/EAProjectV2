using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EACharacterInfo : MonoBehaviour
{
	public string[] BoneNames = null;
    public Transform[] Bones  = null;
    public Renderer[] renderers = null;
}
