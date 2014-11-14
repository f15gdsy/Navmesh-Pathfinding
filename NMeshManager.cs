using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/**
 * Note:
 * 1. Attach one and only one NMeshManager to an GameObject.
 * 2. Create "Navmesh Container" GameObject to store all the navmeshes.
 * 3. Create Navmesh
 */
public class NMeshManager : SingletonBase<NMeshManager> {

	public List<NMesh> _meshes;
	public List<NMesh> meshes {get{return _meshes;}}

	public int pixelsPerUnit = 100;

//	private static NMeshManager _instance;
//	public static NMeshManager instance {
//		get {
//			if (_instance == null) {
//				Debug.Log("NMeshManager: instance is null.");
//			}
//			return _instance;
//		}
//	}
	
	void Start () {
		RefreshMeshes();
	}

	public void RefreshMeshes () {
		_meshes = new List<NMesh>();	

		Transform container =GameObject.Find("[Navmesh Container]").transform;
		int i = 0;
		foreach (Transform child in container) {
			NMesh mesh = child.GetComponent<NMesh>(); 
			mesh.id = i++;
			meshes.Add(mesh);
		}
		
		foreach (NMesh mesh1 in meshes) {
			foreach (NMesh mesh2 in meshes) {
				if (mesh1.Equals(mesh2)) continue;
				
				if (mesh1.TestCollision(mesh2)) {
					mesh1.AddNeighbour(mesh2);
				}
			}
		}
	}
	
	public NMesh GetMeshForPoint (Vector2 point) {	
		foreach (NMesh mesh in meshes) {
			if (mesh.TestWithinMesh(point)) {
				return mesh;			
			}
			
		}
		
		return null;
	}

	public NMesh GetNearestMeshForPoint (Vector2 point) {
		float minDist = float.MaxValue;
		NMesh nearestMesh = meshes[0];
		foreach (NMesh mesh in meshes) {
			if (mesh.TestWithinMesh(point)) {
				return mesh;	
			}
			
			float dist = mesh.GetMHDistanceTo(point);
			if (dist < minDist) {
				minDist = dist;
				nearestMesh = mesh;
			}
		}
		return nearestMesh;
	}
	
	public Vector2 GetNearestPointInMesh (Vector2 point) {
		NMesh nearestMesh = GetNearestMeshForPoint(point);

//		Debug.Log("point: "+point.ToString());
//		Debug.Log("mesh top left: "+nearestMesh.upLeft);
//		Debug.Log("mesh lower right: "+nearestMesh.lowerRight);
//		Debug.Log("new point "+nearestMesh.GetNearestPointTo(point).ToString());
		
		return nearestMesh.GetNearestPointTo(point);
	}
}
