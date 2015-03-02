using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Navigation {

	public class NMesh : MonoBehaviour, INode {
		
		public Vector2 position;
		public int id;
		
		private float _pixelsPerUnit;

		private float _width;
		public float width {get {return _width;}}
		private float _height;
		public float height {get {return _height;}}

		private Dictionary<NMesh, NeighbourData> _neighbourtodistance;
		private List<NMesh> _neighbours = new List<NMesh>();
		public List<NMesh> neighbours {get{return _neighbours;}}
		
		public Vector2 upLeft {get {return new Vector2(position.x-_width*0.5f, position.y+_height*0.5f);}}
		public Vector2 upRight {get {return new Vector2(position.x+_width*0.5f, position.y+_height*0.5f);}}
		public Vector2 lowerLeft {get {return new Vector2(position.x-_width*0.5f, position.y-_height*0.5f);}}
		public Vector2 lowerRight {get {return new Vector2(position.x+_width*0.5f, position.y-_height*0.5f);}}
		
		public Rect rect {get {return new Rect(position.x-_width*0.5f, position.y+_height*0.5f, _width, _height);}}

		private Transform _trans;


		void Awake () {
			_trans = transform;

			SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
			Sprite sprite = spriteRenderer.sprite;

			_pixelsPerUnit = NMeshManager.instance.pixelsPerUnit;

			_width = _trans.localScale.x * sprite.rect.width / _pixelsPerUnit / 2;	// The sprite of the mesh may not be 1 pixel wide exactly.
			_height = _trans.localScale.y * sprite.rect.height/ _pixelsPerUnit / 2;	// So the local scale need to consider the sprite size.

			Destroy(spriteRenderer);

			position = new Vector2(_trans.position.x, _trans.position.y);
			
			_neighbourtodistance = new Dictionary<NMesh, NeighbourData>();
		}

		public void AddNeighbour (NMesh neighbour, NeighbourData data = null) {
			if (_neighbourtodistance.ContainsKey(neighbour)) return;
			
			if (data == null) {		
				Vector2 offset = position - neighbour.position;
				float distance = offset.magnitude;
				List<Vector2> gate = GetGateToMesh(neighbour);
				
				Vector2 mid = (gate[0] + gate[1]) / 2;
				Vector2 toMid = mid - position;
				Vector2 midPerpendicular = SMath.GetPerpendicular2(toMid, true);
				
				Vector2 line0 = gate[0] - position;
				
				Vector2 left;
				Vector2 right;
				if (Vector2.Dot(line0, midPerpendicular) > 0) {	// less than 90 deg
					left = gate[0];
					right = gate[1];
				}
				else {
					left = gate[1];
					right = gate[0];
				}
				
				data = new NeighbourData(distance, left, right);
				NeighbourData neighbourData = new NeighbourData(distance, right, left);		// neighbour's left right is reverse
				neighbour.AddNeighbour(this, neighbourData);

				// For Debug
	//			GameObject gate1 = GameObject.CreatePrimitive(PrimitiveType.Quad);
	//			GameObject gate2 = GameObject.CreatePrimitive(PrimitiveType.Quad);
	//			gate1.transform.localScale *= 0.1f * pixelToUnit;
	//			gate2.transform.localScale *= 0.1f * pixelToUnit;
	//			gate1.renderer.material.color = Color.red;
	//			gate2.renderer.material.color = Color.red;
	//			gate1.transform.position = new Vector3(gate[0].x, gate[0].y, -2);
	//			gate2.transform.position = new Vector3(gate[1].x, gate[1].y, -2);
	//			Destroy(gate1.GetComponent<Collider>());
	//			Destroy(gate2.GetComponent<Collider>());

			}
			
			_neighbourtodistance.Add(neighbour, data);
			neighbours.Add(neighbour);
		}

		public void ClearNeighbours () {
			if (_neighbours != null) {
				_neighbours.Clear();
			}
			if (_neighbourtodistance != null) {
				_neighbourtodistance.Clear();
			}
		}
		
		public List<Vector2> GetGateToMesh (NMesh neighbour) {
			if (!TestCollision(neighbour)) return null;
			
			List<Vector2> gate = new List<Vector2>();
			
			if (_neighbourtodistance.ContainsKey(neighbour)) {
				NeighbourData data = _neighbourtodistance[neighbour];
				gate.Add(data.left);
				gate.Add(data.right);
				return gate;
			}
			
			if (neighbour.TestWithinMesh(upLeft)) gate.Add(upLeft);
			if (neighbour.TestWithinMesh(upRight)) gate.Add(upRight);
			if (neighbour.TestWithinMesh(lowerLeft)) gate.Add(lowerLeft);
			if (neighbour.TestWithinMesh(lowerRight)) gate.Add(lowerRight);
			
			if (gate.Count == 0) {	// neigbour has 2 points in me
				gate = neighbour.GetGateToMesh(this);
			}
			else if (gate.Count == 1) {	// neighbour has 1 point in me
				if (TestWithinMesh(neighbour.upLeft)) gate.Add(neighbour.upLeft);
				if (TestWithinMesh(neighbour.upRight)) gate.Add(neighbour.upRight);
				if (TestWithinMesh(neighbour.lowerLeft)) gate.Add(neighbour.lowerLeft);
				if (TestWithinMesh(neighbour.lowerRight)) gate.Add(neighbour.lowerRight);
			}
			else if (gate.Count == 2) {}	// i have 2 points in neighbour

			return gate;
		}
		
		// TODO: Override this method to create other mesh type
		public bool TestCollision (NMesh other) {
			float offsetX = Mathf.Abs(position.x - other.position.x);
			float offsetY = Mathf.Abs(position.y - other.position.y);
			
			if (offsetX <= (_width + other.width)*0.5f && offsetY <= (_height + other.height)*0.5f) {
				return true;	
			}
			return false;
		}
		
		
		public bool TestWithinMesh (Vector2 point) {
			float offsetX = Mathf.Abs(position.x - point.x);
			float offsetY = Mathf.Abs(position.y - point.y);
			if (offsetX <= _width*0.5f && offsetY <= _height*0.5f) {
				return true;	
			}
			return false;	
		}
		
		public float GetDistanceTo (INode destination) {
			NMesh destinationMesh = (NMesh) destination;
			if (_neighbourtodistance.ContainsKey(destinationMesh)) {
				return _neighbourtodistance[destinationMesh].distance;	
			}
			
			Vector2 offset = destinationMesh.position - position;
			return offset.magnitude;
		}
		
		public List<INode> GetNeighbours () {
			return neighbours.Cast<INode>().ToList();
		}
		
		public override bool Equals (object o) {
			if (o.GetType().Equals(typeof(NMesh))) {
				NMesh other = (NMesh) o;
				if (other.position == position && other.width == _width && other.height == _height) {
					return true;	
				}
			}
			return false;
		}
		
		public override int GetHashCode () {
			return (int) (position.x + position.y * 1000 + _width *1000000 + _height * 1000000000);
		}
		
		// Manhattan distance
		public float GetMHDistanceTo (Vector2 point) {
			if (TestWithinMesh(point)) return 0;
			
			float dX = Mathf.Abs(point.x - _trans.position.x);
			float dY = Mathf.Abs(point.y - _trans.position.y);
			
			float distance = 0;
			if (dX < _width/2) distance = dY - _height/2;
			else if (dY < _height/2) distance = dX - _width/2;
			else distance = dY + dX - _width/2 - _height/2;
			
			return distance;
		}
		
		public Vector2 GetNearestPointTo (Vector2 point) {
			if (TestWithinMesh(point)) {
				return point;
			}
			
			float dX = point.x - _trans.position.x;
			float dY = point.y - _trans.position.y;
			
			Vector2 dest = Vector2.zero;
			float tolerance = 0.00001f;
			if (Mathf.Abs(dX) < _width/2) {
				dest.x = point.x;
				dest.y = _trans.position.y + (dY>0? _height/2-tolerance : -_height/2+tolerance);
			}
			else if (Mathf.Abs(dY) < _height/2) {
				dest.x = _trans.position.x + (dX>0? _width/2-tolerance : -_width/2+tolerance);
				dest.y = point.y;
			}
			else {
				dest.x = _trans.position.x + (dX>0? _width/2-tolerance : -_width/2+tolerance);
				dest.y = _trans.position.y + (dY>0? _height/2-tolerance : -_height/2+tolerance);
			}
					
			return dest;
		}

		public override string ToString ()
		{
			return "mesh " + id + " [ position:" + position.ToString() + " _width:" + _width + " _height:" + _height + " ]";
		}
	}


	public class NeighbourData {
		public float distance;
		public Vector2 left;
		public Vector2 right;
			
		public NeighbourData (float distance, Vector2 left, Vector2 right) {
			this.distance = distance;
			this.left = left;
			this.right = right;
		}
	}

}