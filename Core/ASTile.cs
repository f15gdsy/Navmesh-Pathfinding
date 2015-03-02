using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Navigation {

	public class ASTile : MonoBehaviour, INode {
		
		private TileMap tileMap;
		public int x;
		public int y;
		private bool _isBlocked = false;
		public bool isBlocked {
			get {return _isBlocked;}
			set {
				_isBlocked = value;
				if (tileMap.shouldDraw) {
					renderer.material.color = Color.black;	
				}
			}
		}

		void Start () {
			tileMap = GameObject.Find("Ground").GetComponent<TileMap>();
		}
		
		public float GetDistanceTo (INode destination) {
			ASTile destinationTile = (ASTile) destination;
			Vector2 currentPos = SMath.Vec3ToVec2(transform.position);
			if (destinationTile == null) Debug.Log("????");
			Vector2 destinationPos = SMath.Vec3ToVec2(destinationTile.transform.position);
			
			Vector2 offset = currentPos - destinationPos;
	//		return Mathf.Abs(offset.x) + Mathf.Abs(offset.y);	// only up, right, down, left direction
	//		return offset.magnitude;	// free direction
			
			float offsetX = Mathf.Abs(offset.x);
			float offsetY = Mathf.Abs(offset.y);
			float max = Mathf.Max(offsetX, offsetY);
			float min = Mathf.Min(offsetX, offsetY);
			
			return (max - min) + Mathf.Sqrt(2) * min;
		}
		
		public List<INode> GetNeighbours () {
			List<ASTile> neighbours = tileMap.GetTileNeighbours(this);
			List<INode> nodes = new List<INode>();
			
			foreach (ASTile neighbour in neighbours) {
				if (neighbour.x==0 || neighbour.x==tileMap.width-1 || neighbour.y==0 || neighbour.y==tileMap.height-1) {
					continue;	
				}
				
				if (neighbour.isBlocked) {
					return new List<INode>();
				}
				else {
					nodes.Add((INode) neighbour);	
				}
			}
			return nodes;
		}
		
	}

}