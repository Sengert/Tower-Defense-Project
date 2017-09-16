using UnityEngine;
using System.Collections;

public class MapNode : MonoBehaviour {

	public int nodeID;
	public bool isBuildable;
	public int heightLevel;
	public bool isPath = false;


	private Renderer tileRender;
	private Color defaultColor;


	void Start()
	{
		tileRender = GetComponent<Renderer> ();
		defaultColor = tileRender.material.color;
	}

	void OnMouseEnter(){
		tileRender.material.color = Color.red;
	}

	void OnMouseExit(){
		tileRender.material.color = defaultColor;
	}

	public void setNodeID(int id){
		this.nodeID = id;
	}

	public void setHeightLevel(int height){
		if (height == 0) {
			transform.position = new Vector3 (transform.position.x, 0, transform.position.z);
		}

		this.heightLevel = height;
		this.gameObject.transform.Translate(Vector3.up * height);
	}

	public void setIsBuildable(bool canBuild){
		this.isBuildable = canBuild;
	}

	public void setIsPath(bool a){
		this.isPath = a;
	}

	public bool getIsPath(){
		return(isPath);
	}
}
