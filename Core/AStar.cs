using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Navigation {

	public static class AStar {
		
		private static List<INode> openSet;	// the nodes to be explored
		private static List<INode> closeSet;	// the nodes explored
		private static Dictionary<INode, float> gScore;	// dist from start
		private static Dictionary<INode, float> fScore;	// dist from start + heuristic estimate to dest
		private static Dictionary<INode, INode> pathLink;	// <to, from>

		public static List<INode> SearchPath (INode start, INode destination) {
			openSet = new List<INode>();	
			closeSet = new List<INode>();	
			
			pathLink = new Dictionary<INode, INode>();	
			
			gScore = new Dictionary<INode, float>();	
			fScore = new Dictionary<INode, float>();	
			
			openSet.Add(start);
			gScore.Add(start, 0);
			fScore.Add(start, start.GetDistanceTo(destination));
			
			while (openSet.Count > 0) {
				INode current = GetnodeWithMinFFromOpenSet();
				
				if (current.Equals(destination)) {
					return ReconstructPath(pathLink, destination);
				}
				
				openSet.Remove(current);
				closeSet.Add(current);
				
				List<INode> neighbours = current.GetNeighbours();
				foreach (INode neighbour in neighbours) {
					float neighbourG = gScore[current] + current.GetDistanceTo(neighbour);
					if (closeSet.Contains(neighbour) && neighbourG >= gScore[neighbour]) {
						continue;	
					}
					
					if (!openSet.Contains(neighbour) || neighbourG < gScore[neighbour]) {
						pathLink[neighbour] = current;	// from current to neighbour
						gScore[neighbour] = neighbourG;
						fScore[neighbour] = neighbourG + neighbour.GetDistanceTo(destination);
						if (!openSet.Contains(neighbour)) {
							openSet.Insert(0, neighbour);	
						}
					}
				}
			}
			return new List<INode>();
		}
		
		private static List<INode> ReconstructPath (Dictionary<INode, INode> pathLink, INode destination) {
			INode current = destination;
			List<INode> path = new List<INode>();
			
			while (true) {
				path.Insert(0, current);
				if (pathLink.ContainsKey(current)) {
					current = pathLink[current];
					path.Remove(current);
				}
				else {
					break;	
				}
			}
			return path;
		}
		
		private static INode GetnodeWithMinFFromOpenSet () {
			bool isFirstLoop = true;
			float minF = -1;
			INode nodeWithMinF = null;
			
			foreach (INode node in openSet) {
				float f = fScore[node];
				if (!isFirstLoop) {
					if (f >= minF) {
						continue;	
					}
				}
				minF = f;
				nodeWithMinF = node;
				isFirstLoop = false;
			}
			return nodeWithMinF;
		}
	}



	public interface INode {
		float GetDistanceTo (INode destination);	
		List<INode> GetNeighbours();
	}

}