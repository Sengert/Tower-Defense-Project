using UnityEngine;
using System.Collections;

public class MapGeneration : MonoBehaviour {

	[SerializeField]
	public GameObject startTile;
	[SerializeField]
	public GameObject endTile;
	[SerializeField]
	private GameObject node;//Add the node game object in the inspector
	[SerializeField]
	private GameObject pathNode;//Add the pathNode game object in the inspector

	//Empty Game Objects used to store each generated node as a child for organization
	private GameObject allPathNodes;
	private GameObject allMapNodes;


	private int nodeNum = 0;
	GameObject[] nodes;

	//--------------------------------------------------------------
	//Variables used by generateHeightMap() and its helper functions
	//--------------------------------------------------------------
	public int mapTotalLength;//Must be odd
	public int mapTotalWidth;//Must be odd
	public int maxHeight = 3; //The height of hills placed on the map.
	public int maxPeaks = 3; //The amount of hills placed on the map.

	//Currently not being used
	//The amounth of tiles the path must at least move before turning.
	//public int minStraightLength = 2;

	//---------------------------------------------------------
	//Variables used by generatePath() and its helper functions
	//---------------------------------------------------------
	private int startID;
	private int endID;
	private int lastTileID;
	private int direction;
	private bool isComplete = false;
	private Vector3 tempV3;
	private int amtPathNodes;


	//Function called on creation of this script
	void Start () {
		//Must have 2 empty Game Objects in the hierarchy. "PathNodes" and "Nodes"
		allPathNodes = GameObject.Find ("PathNodes");
		allMapNodes = GameObject.Find ("Nodes");

		//Creates an array of the size of the map to store each node created
		nodes = new GameObject[(mapTotalLength * mapTotalWidth) + 1];


		generateNodes ();
		generateHeightMap ();
		generatePath ();
	}

	//Generates the Length*Width Rectangle of the map, sets the ID for each individual node,
	//and adds them to an array for reference to every node, also organizes the objects in Hierarchy
	public void generateNodes(){
		for(int i = 0; i < mapTotalLength; i++)
		{
			for(int j = 0; j < mapTotalWidth; j++)
			{
				nodes[nodeNum] = Instantiate (node, new Vector3(i, 0, j), Quaternion.identity) as GameObject;
				nodes[nodeNum].transform.parent = allMapNodes.transform;
				nodes[nodeNum].GetComponent<MapNode>().setNodeID (nodeNum);
				nodes[nodeNum].name = "NodeID" + nodeNum;

				nodeNum++;
			}
		}
		nodeNum = 0;
	}


	//Selects 'x' amount of peaks within the bounds of the nodes.length then places those peaks directly instead of traversing throught the nodes array
	public void generateHeightMap()
	{
		int[] peaks = new int[maxPeaks];

		for (int i = 0; i < maxPeaks; i++) 
		{
			if (i == 0 && maxPeaks > 1) 
			{
				//Ensure at least 1 peak is in the first section of map if more than 1 peak.
				peaks [i] = Random.Range (0,Mathf.FloorToInt(((mapTotalWidth * mapTotalLength)*.25f)));
				nodes [peaks [i]].GetComponent<MapNode> ().setHeightLevel (maxHeight);
			}
			else if (i != maxPeaks - 1 || maxPeaks == 1) {
				peaks [i] = Random.Range (0, (mapTotalWidth * mapTotalLength));
				nodes [peaks [i]].GetComponent<MapNode> ().setHeightLevel (maxHeight);
			} 
			else 
			{
				//Ensure at least 1 peak is in the back section of map if more than 1 peak.
				peaks [i] = Random.Range (Mathf.FloorToInt((mapTotalWidth * mapTotalLength)*.75f),(mapTotalWidth * mapTotalLength));
				nodes [peaks [i]].GetComponent<MapNode> ().setHeightLevel (maxHeight);
			}
		}

		for(int i = 0; i < peaks.Length; i++){
			Debug.Log ("Peak stored at " + peaks[i]);
		}

	}

	//Generates a path with a starting point and ending point by "carving" it into the generated map
	//All path tiles are set to the height of 0 and only move toward the end point from the start point
	//The terms 'left', 'right', and 'forward' are relative to the view facing from the Starting Tile towards the Ending Tile
	public void generatePath()
	{
		startID = Random.Range (0,mapTotalLength);
		endID = Random.Range (mapTotalWidth * mapTotalLength - mapTotalLength, mapTotalWidth * mapTotalLength);
		lastTileID = startID;

		//Instantiating the starting pathNode
		tempV3 = nodes[startID].transform.position;
		Destroy (nodes [startID]);
		nodes[startID] = Instantiate (startTile, tempV3, Quaternion.identity) as GameObject;
		nodes[startID].name = "Start Tile";
		Debug.Log("Start Tile placed at " + startID);
		nodes[startID].GetComponent<MapNode>().setHeightLevel(0);
		nodes[startID].GetComponent<MapNode>().setIsBuildable(false);
		nodes[startID].GetComponent<MapNode>().setIsPath (true);

	
		//Instantiating the ending pathNode
		tempV3 = nodes[endID].transform.position;
		Destroy (nodes[endID]);
		nodes [endID] = Instantiate (endTile, tempV3, Quaternion.identity) as GameObject;
		nodes[endID].name = "End Tile";
		Debug.Log("End Tile placed at " + endID);
		nodes[endID].GetComponent<MapNode>().setHeightLevel(0);
		nodes[endID].GetComponent<MapNode>().setIsBuildable(false);
		nodes[endID].GetComponent<MapNode>().setIsPath (true);

		//----------------------
		//Begin path generation.
		//----------------------
		while (!isComplete) {
			//----------------------------------------------------------
			//When the starting tile is on the 'right' corner of the map
			//----------------------------------------------------------
			if(isTileEnd())
			{
				//End the while loop for path generation
				isComplete = true;
				Debug.Log ("Path Complete!");
			}
			else if (lastTileID == 0)
			{
				direction = Random.Range (0,2);
				//When direction is 0, place a pathNode to the left.
				if (direction == 0 && !(nodes[lastTileID + 1].GetComponent<MapNode>().getIsPath())) {
					increasePathLength ("left");
				} 
				//When dirction is 1, place a pathNode forward.
				else if (direction == 1) 
				{
					increasePathLength ("forward");
				}
			}
			//----------------------------------------------------------
			//When the starting tile is on the 'left' corner of the map
			//----------------------------------------------------------
			else if (lastTileID == (mapTotalLength - 1))
			{
				direction = Random.Range (0,2);
				//When direction is 0, place a pathNode to the right.
				if (direction == 0 && !(nodes[lastTileID - 1].GetComponent<MapNode>().getIsPath())) {
					increasePathLength ("right");
				} 
				//When direction is 1, place a pathNode forward.
				else if (direction == 1) {
					increasePathLength ("forward");
				}
			}
			//-------------------------------------------------------------------------------
			//If the path reaches the end of the map and it is not adjacent to the endID tile
			//-------------------------------------------------------------------------------
			else if(lastTileID >= (mapTotalWidth*mapTotalLength)-mapTotalLength && lastTileID != endID - 1 && lastTileID != endID + 1 && lastTileID != (endID - mapTotalWidth))
			{
				//If the Ending Tile is left of the path, move left
				if (lastTileID < endID) {
					increasePathLength ("left");
				} 
				else//move right
				{
					increasePathLength ("right");
				}
			}
			//---------------------------------------
			//If the path is on a boundary of the map
			//---------------------------------------
			else if(isTileBoundary(lastTileID))
			{
				//Move either forward or away from the boundary
				//If the path is on the right boundary, move left or forward
				if(lastTileID % mapTotalLength == 0)
				{
					direction = Random.Range(0,2);
					//When direction is 0, and the tile to the left is not a path, move left
					if(direction == 0 && !(nodes[lastTileID + 1].GetComponent<MapNode>().getIsPath()))
					{
						increasePathLength ("left");
					}
					else//move forward
					{
						increasePathLength ("forward");
					}
				}
				//If the path is on the left boundary, move right or forward
				else if(lastTileID % mapTotalLength == (mapTotalLength - 1))
				{
					direction = Random.Range (0,2);
					//When direction is 0, move right.
					if(direction == 0 && !(nodes[lastTileID - 1].GetComponent<MapNode>().getIsPath()))
					{
						increasePathLength ("right");
					}
					else//move forward, can probably condense this forward with the one above.
					{
						increasePathLength ("forward");
					}
				}
			}
			//----------------------------------------------
			//When path is anywhere else (middle) on the map
			//----------------------------------------------
			else if (lastTileID != endID - 1 && lastTileID != endID + 1 && lastTileID != (endID - mapTotalWidth)) {
				direction = Random.Range (0,3);
				//When direction is 0, move right.
				if (direction == 0 && !(nodes[lastTileID - 1].GetComponent<MapNode>().getIsPath())) {
					increasePathLength ("right");
				}
				//When direction is 1, move left.
				else if (direction == 1 && !(nodes[lastTileID + 1].GetComponent<MapNode>().getIsPath())) {
					increasePathLength ("left");
				}
				//When direction is 2, move forward.
				else
				{
					increasePathLength ("forward");
				}
			} 
		}

	}

	//Helper function to simplify generatePath()
	public void increasePathLength(string direction)
	{
			if (direction == "left") 
			{
				tempV3 = nodes [lastTileID + 1].transform.position;
				Destroy (nodes[lastTileID + 1]);
				createPathNode ((lastTileID + 1), tempV3);
				lastTileID = (lastTileID + 1);
			} 
			else if (direction == "right") 
			{
				tempV3 = nodes [lastTileID - 1].transform.position;
				Destroy (nodes[lastTileID - 1]);
				createPathNode ((lastTileID - 1), tempV3);
				lastTileID = (lastTileID - 1);
			} 
			else if (direction == "forward") 
			{
				tempV3 = nodes [lastTileID + mapTotalWidth].transform.position;
				Destroy (nodes[lastTileID + mapTotalWidth]);
				createPathNode ((lastTileID + mapTotalWidth), tempV3);
				lastTileID = (lastTileID + mapTotalWidth);

			if (lastTileID + mapTotalWidth != endID && (lastTileID + mapTotalWidth) < (mapTotalWidth*mapTotalLength)) //Ensure path doesn't replace Ending Tile, or go forward off the end off the map
				{
					tempV3 = nodes [lastTileID + mapTotalWidth].transform.position;
					Destroy (nodes[lastTileID + mapTotalWidth]);
					createPathNode ((lastTileID + mapTotalWidth), tempV3);
					lastTileID = (lastTileID + mapTotalWidth);
				}
			}
	}

	//Function used to replace a mapNode with a pathNode
	//Called from generatePath()
	//could also be used if feature to build more paths is ever implemented
	public void createPathNode(int nodeToReplace, Vector3 pos){
		amtPathNodes++;

		nodes[nodeToReplace] = Instantiate (pathNode, pos, Quaternion.identity) as GameObject;
		//Set reference to the MapNode script attached to this tile
		MapNode mapNode = nodes [nodeToReplace].GetComponent<MapNode> ();
		nodes[nodeToReplace].transform.parent = allPathNodes.transform;//Add this tile as a child to the PathNodes Game Object in hierarchy
		mapNode.setNodeID(nodeToReplace);
		nodes[nodeToReplace].name = "Waypoint " + amtPathNodes;//Rename this tile to represent a waypoint for AI
		//Set properties to that of a path
		mapNode.setHeightLevel (0);
		mapNode.setIsBuildable(false);
		mapNode.setIsPath (true);


	}

	//Small function that returns true if the last path placed was on a boundary or edge of
	//the map. Used to constrain the path from going through the edge of the map and starting
	//again at the other end.
	public bool isTileBoundary(int IDtoCheck){

		if (IDtoCheck % mapTotalLength == 0 || IDtoCheck % mapTotalLength == mapTotalLength - 1) {
			return true;
		} else {
			return false;
		}
	}

	public bool isTileEnd(){
		if (lastTileID == endID - 1 || lastTileID == endID + 1 || lastTileID == (endID - mapTotalWidth)) {
			return true;
		} else {
			return false;
		}
	}
}