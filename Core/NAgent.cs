using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NAgent : MonoBehaviour {
	
	public Vector2 position {get {return new Vector2(_trans.position.x, _trans.position.y);}}
	public bool isDebug = false;
	
	private NMeshManager _meshManager;
	private List<NMesh> _meshLink;
	private List<GameObject> _marks;	// for test
	
	private Transform _trans;
	
	
	void Start () {
		GameObject go = GameObject.Find("Navigation Manager");
		if (go != null) {
			_meshManager = go.GetComponent<NMeshManager>();
		}
		else {
			Debug.Log("Navigation Manager cannot be found!");
		}
		if (isDebug) {
			_marks = new List<GameObject>();
		}
		_trans = transform;
	}
	
	public List<Vector2> GetPath (Vector3 destination3) {
		return GetPath(SMath.Vec3ToVec2(destination3));
	}


	public List<Vector2> GetPath (Vector2 destination) {
		List<Vector2> path = new List<Vector2>();
		
		NMesh destinationMesh = _meshManager.GetMeshForPoint(destination);
		NMesh currentMesh = GetCurrentMesh();
		
		if (destinationMesh.Equals(GetCurrentMesh())) {	// if destination is in the same nav mesh
			path.Add(destination);
		}
		else {
			_meshLink = (AStar.SearchPath(currentMesh, destinationMesh)).Cast<NMesh>().ToList();

			if (isDebug) {
				for (int j=_marks.Count-1; j>=0; j--) {
					GameObject mark = _marks[j];
					_marks.Remove(mark);
					Destroy(mark);
				}
			}
			
			Vector2 turningPoint = position;
			int i=1;
			while (true) {
				turningPoint = GetTurningPoint(turningPoint, destination);

				if (isDebug) {
					GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Quad);
					mark.transform.localScale *= 0.3f + 0.1f * i++;
					mark.transform.position = new Vector3(turningPoint.x, turningPoint.y, -3);
					mark.renderer.material.color = Color.black;
					Destroy(mark.GetComponent<Collider>());
					_marks.Add(mark);
				}
				
				path.Add(turningPoint);
				
				if (turningPoint.Equals(destination)) {
					break;	
				}
//				while (true) {
//					if (_meshLink.Count <= 0 || _meshLink[0].TestWithinMesh(turningPoint)) break;
//					_meshLink.RemoveAt(0);
//				}
			}
		}
		
		return path;
	}

	// Will smartly get the nearest mesh if destination is not a point within any meshes.
	public List<Vector2> GetPathSmart (Vector2 destination) {
		Vector2 newDestination = _meshManager.GetNearestPointInMesh(destination);
		return GetPath(newDestination);
	}

	// NOTE: start should be within the _meshLink[0] or is the gate of _meshLink[0]
	private Vector2 GetTurningPoint (Vector2 start, Vector2 destination) {
		Vector2 leftLimit = Vector2.zero;
		Vector2 rightLimit = Vector2.zero;
		Vector2 turningPoint = destination;

		int lastLeftMeshIndex = -1;
		int lastRightMeshIndex = -1;

		NMesh startMesh = _meshLink[0];

		bool isFirstLoop = true;
		bool shouldBreak = false;
		bool isLeft = false;
		for (int i=0; i<_meshLink.Count; i++) {
			if (_meshLink.Count == 1) {
				_meshLink.RemoveAt(0);
				return destination;	
			}
			
			if (i == _meshLink.Count - 1) {
				Vector2 line = destination - start;

				LineSituation situation = SMath.IsBetween(leftLimit, line, rightLimit);
				switch (situation) {
				case LineSituation.Between:
					break;
				case LineSituation.OutLeft:
					turningPoint = leftLimit + start;
					isLeft = true;
					break;
				case LineSituation.OutRight:
					turningPoint = rightLimit + start;
					isLeft = false;
					break;
				case LineSituation.Same:
					break;
				}
				break;
			}
			NMesh current = _meshLink[i];
			NMesh neighbour = _meshLink[i+1];
			
			List<Vector2> gate = current.GetGateToMesh(neighbour);
			Vector2 leftGate = gate[0];
			Vector2 rightGate = gate[1];
			
			Vector2 leftLine = leftGate - start;
			Vector2 rightLine = rightGate - start;
			
			if (isFirstLoop) {
				leftLimit = leftLine;
				rightLimit = rightLine;
				startMesh = current;
				isFirstLoop = false;
			}
			else {
				LineSituation situation = SMath.IsBetween(leftLimit, leftLine, rightLimit);
				switch (situation) {
				case LineSituation.Between:
					leftLimit = leftLine;
					lastLeftMeshIndex = i;
					break;
				case LineSituation.OutLeft:
					break;
				case LineSituation.OutRight:
					turningPoint = rightLimit + start;
					isLeft = false;
					shouldBreak = true;
					break;
				case LineSituation.Same:
					break;
				}
				
				if(shouldBreak) break;
				
				situation = SMath.IsBetween(leftLimit, rightLine, rightLimit);
				switch (situation) {
				case LineSituation.Between:
					rightLimit = rightLine;
					lastRightMeshIndex = i;
					break;
				case LineSituation.OutLeft:
					turningPoint = leftLimit + start;
					isLeft = true;
					shouldBreak = true;
					break;
				case LineSituation.OutRight:
					break;
				case LineSituation.Same:
					break;
				}

				if (shouldBreak) break;
			}
		}

		// NOTE: The mesh links members have to be neighbours, 
		// so if a mesh in the middle of the link is removed, the previous meshes need to be removed too.
		if (isLeft) {
			for (int i=0; i<=lastLeftMeshIndex; i++) {
				_meshLink.RemoveAt(0);
			}	
		}
		else {
			for (int i=0; i<=lastRightMeshIndex; i++) {
				_meshLink.RemoveAt(0);
			}
		}	
	
		_meshLink.Remove(startMesh);

		return turningPoint;
	}
	

	private NMesh GetCurrentMesh () {
		NMesh mesh = _meshManager.GetMeshForPoint(position);
		if (mesh == null) {
			mesh = _meshManager.GetNearestMeshForPoint(position);
			
		}
		return mesh;
	}
}


