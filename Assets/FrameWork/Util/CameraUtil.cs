using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Debug = EAFrameWork.Debug;

public class CameraDepth
{
	public const int ClearCamera = -10; 
	public const int WorldCamera = 1;
	public const int UICamera = 5;
    public const int EffectCamera = 100;
}

[System.Serializable]
public struct CameraPose
{
	public Vector3 position;
	public Vector3 rotation;
	public float nearClip;
	public float farClip;
	public float fov;
}
public class SceneViewPose
{
	// Vector3은 fxJson으로 serialize되지 않는다.
	public float x, y, z; // SceneView.pivot
	public float rx,ry,rz,rw; // SceneView.rotation
	public float size; // SceneView.size

	//public Vector3 position { get { return new Vector3(x, y, z); } set { x=value.x; y=value.y; z=value.z; }}
	//public Vector3 rotation { get { return new Vector3(rx, ry, rz); } set { rx = value.x; ry = value.y; rz = value.z; } }
	public Vector3 GetPosition() { return new Vector3(x, y, z); }
	public void SetPosition(Vector3 value) { x = value.x; y = value.y; z = value.z; }
	public Quaternion GetRotation() { return new Quaternion(rx, ry, rz, rw); }
	public void SetRotation(Quaternion value) { rx = value.x; ry = value.y; rz = value.z; rw = value.w;  }

	
	//public float nearClip;
	//public float farClip;
	//public float fov;
}
public class CameraUtil
{

	public static Camera GetCamera(int depth)
	{
		for (int i = 0; i < Camera.allCameras.Length; i++)
		{
			Camera cam = Camera.allCameras[i];
			if (cam.depth == depth)
				return cam;
		}

        Debug.Assert(false, "No Camera for depth: " +  depth);
        
		return null;
	}

	public static Camera FindMainCamera()
	{
		for (int i = 0; i < Camera.allCameras.Length; i++)
		{
			Camera cam = Camera.allCameras[i];
			if(cam.tag == Tags.MainCamera)
				return cam;
		}
        Debug.Assert(false, "No MainCamera");
        return null;
	}

	public static CameraPose GetCameraPose(Camera cam)
	{
		CameraPose pose = new CameraPose();
		pose.position = cam.transform.position;
		pose.rotation = cam.transform.localRotation.eulerAngles;
		pose.nearClip = cam.nearClipPlane;
		pose.farClip = cam.farClipPlane;
		pose.fov = cam.fieldOfView;
		return pose;
	}
	public static void SetCameraPose(Camera cam, CameraPose pose)
	{
		cam.transform.position = pose.position;
		cam.transform.localRotation = Quaternion.Euler(pose.rotation);
		cam.nearClipPlane = pose.nearClip;
		cam.farClipPlane = pose.farClip;
		cam.fieldOfView = pose.fov;
	}

#if UNITY_EDITOR
	public static SceneViewPose GetSceneViewPose(SceneView sceneView)
	{
		SceneViewPose pose = new SceneViewPose();
		pose.SetPosition(sceneView.pivot);
		pose.SetRotation(sceneView.rotation);
		pose.size = sceneView.size;
		return pose;
	}
	public static void SetSceneViewPose(SceneView sceneView, SceneViewPose pose)
	{
		sceneView.pivot = pose.GetPosition();
		sceneView.rotation = pose.GetRotation();
		sceneView.size = pose.size;
	}
#endif
}
