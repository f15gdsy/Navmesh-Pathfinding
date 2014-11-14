using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileMap : MonoBehaviour {
	
	public int width;
	public int height;
	
	public float tileWidth;
	public float tileHeight;
	
	public GameObject tilePrefab;
	private List<ASTile> tiles;
	private List<ASVertex> vertices;
	
	private GameObject tilesContainer;
	
	public bool shouldDraw = true;
	
	
	void Start () {
		tiles = new List<ASTile>();
		tilesContainer = new GameObject("Tiles Container");
		vertices = new List<ASVertex>();
		
		SetupTiles();
//		SetupVertices();
	}
	
	private void SetupVertices () {
		for (int x=0; x<width-1; x++) {
			for (int y=0; y<height-1; y++) {
				float posX = x * tileWidth - tileWidth * (width-2) * 0.5f;
				float posY = y * tileHeight - tileHeight * (height-2) * 0.5f;
				
				GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
				go.transform.localScale *= 0.1f;
				go.transform.position = new Vector3(posX, posY, -4);
				go.renderer.material.color = Color.red;
				ASVertex vertex = go.AddComponent<ASVertex>();
				
				vertices.Add(vertex);
			}
		}
	}
	
	private void SetupTiles () {
		for (int x=0; x<width; x++) {
			for (int y=0; y<height; y++) {
				Vector2 tilePosition = GetPositionForTile(x, y);
				Vector3 position = new Vector3(tilePosition.x, tilePosition.y, transform.position.z - 1);
				
				GameObject go = (GameObject) Instantiate(tilePrefab, position, Quaternion.identity);
				go.transform.parent = tilesContainer.transform;
				ASTile tile = go.AddComponent<ASTile>();
				tile.x = x;
				tile.y = y;
				tiles.Add(tile);
			}
		}
	}
	
	public Vector2 GetTilePositionForPoint (Vector2 point) {
		Vector2 tileIndex = GetTileIndexForPoint(point);
		return GetPositionForTile((int) tileIndex.x, (int) tileIndex.y);
	}
	
	public List<ASTile> GetTileNeighbours2 (ASTile center) {
		List<ASTile> neighbours = new List<ASTile>();
		int minX = center.x - 1;
		int minY = center.y - 1;
		int maxX = center.x + 1;
		int maxY = center.y + 1;
		for (int x=minX; x<=maxX; x++) {
			for (int y=minY; y<=maxY; y++) {
				if (center.x == x && center.y == y) continue;
				ASTile tile = GetTile(x, y);
				if (tile != null) {
					neighbours.Add(tile);	
				}
			}
		}
		return neighbours;
	}
	
	public List<ASTile> GetTileNeighbours (ASTile center) {
		List<ASTile> neighbours = new List<ASTile>();
		int x = center.x;
		int y = center.y;
		int minX = center.x - 1;
		int minY = center.y - 1;
		int maxX = center.x + 1;
		int maxY = center.y + 1;
		
		neighbours.Add(GetTile(x, maxY));
		neighbours.Add(GetTile(maxX, maxY));
		neighbours.Add(GetTile(maxX, y));
		neighbours.Add(GetTile(maxX, minY));
		neighbours.Add(GetTile(x, minY));
		neighbours.Add(GetTile(minX, minY));
		neighbours.Add(GetTile(minX, y));
		neighbours.Add(GetTile(minX, maxY));
		
		foreach (ASTile tile in neighbours) {
			if (tile == null) {
				neighbours.Remove(tile);	
			}
		}
		return neighbours;
	}
	
//	public List<ASVertex> GetVertexNeighbours (ASVertex center) {
//		List<ASVertex> neighbours = new List<ASVertex>();
//		int minX = center.x - 1;
//		int minY = center.y - 1;
//		int maxX = center.x + 1;
//		int maxY = center.y + 1;
//		for (int x=minX; x<=maxX; x++) {
//			for (int y=minY; y<=maxY; y++) {
//				if (center.x == x && center.y == y) continue;
//				ASVertex vertex = GetVertex(x, y);
//				if (vertex != null) {
//					neighbours.Add(vertex);	
//				}
//			}
//		}
//		return neighbours;
//	}
	
	public ASTile GetTileForPosition (Vector2 position) {
		Vector2 tileIndex = GetTileIndexForPoint(position);
		return GetTile((int) tileIndex.x, (int) tileIndex.y);
	}
	
	public ASTile GetTile (int x, int y) {
		if (x<0 || x>=width || y<0 || y>=height) return null;
		return tiles[x*height + y];
	}
	
	public ASVertex GetVertex (int x, int y) {
		if (x<0 || x>=width-1 || y<0 || y>=height-1) return null;
		return vertices[x*(height-1) + y];
	}
	
	public Vector2 GetPositionForTile (int x, int y) {
		float posX = x * tileWidth + tileWidth * 0.5f - tileWidth * width * 0.5f;
		float posY = y * tileHeight + tileHeight * 0.5f - tileHeight * height * 0.5f;
		return new Vector2(posX, posY);
	}
	
	private Vector2 GetTileIndexForPoint (Vector2 point) {
		float distFromLeft = point.x + width * tileWidth * 0.5f;
		float distFromBottom = point.y + height * tileHeight * 0.5f;
		
		int numX = (int) (distFromLeft / tileWidth);
		int numY = (int) (distFromBottom / tileHeight);
		return new Vector2(numX, numY);
	}
}
