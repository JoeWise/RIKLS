using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class TreePlant_Procedural : Plant
{
	public GameObject treeStructure;
	TreeStructure treeStructureScript;
	//public GameObject rootStructure;
	//public List<GameObject> leaves;
	public TreeSettings currentTreeSettings;
	public TreeSettings nextTreeSettings;

	Vector3 initialPos;

	public float zOffset = 0;

	public bool isPaused = false;

	[System.Serializable]
	public class TreeSettings {
		// how many faces does each vine segment have? 4 = square, 6 = hexagonal, etc.

		public Vector3 treeTrajectory = Vector3.up; // treeShape
		// random
		public float treeDirectionWeight = 0.8f;
		// random
		public float treeMaxHeight = 10f; // treeShape
		public int treeMaxDepth = 3; // treeShape
		public float treeMaxWidth = 0.3f; // treeShape

		public bool trunkExtend = true; // treeShape
		public float trunkPercent = 0.5f; // treeShape
		// random
		public float maxNodeChanceToBranch = 0.35f; // treeShape
		// random
		public int maxNumNodeBranches = 3; // treeShape
		// random
		public float minBranchAngle = 15f; // branchShape
		public float maxBranchAngle = 40f; // branchShape

		public int branchResolution = 5; // general
		public float branchNodeMaxAngle = 40f; // branchShape
		public float branchSegLength = 0.3f; // branchShape
		public float branchMinWidth = 0.00f; // branchShape :: unimportant?
		public float branchMaxWidth = 0.04f; // branchShape
		public float branchTrajectoryNoise = 45f; // branchShape
		public float branchTrajectoryWeight = 0.5f; // branchShape
		public float branchTrajectoryWeightVariation = 0; // branchShape
	}

	void Start () 
	{
		currentTreeSettings = new TreeSettings();
		nextTreeSettings = new TreeSettings();
		newTree();
	}

	public override void Update()
    {
    	if (!isPaused)
    	{
    		base.Update();

    		if (treeStructureScript != null)
    		{
    			treeStructureScript.maturity = maturity;
    			treeStructureScript.isMaturing = isMaturing;
    		}
    	}
    }


    public void pauseTree()
    {
        if (isPaused)
            isPaused = false;
        else
            isPaused = true;
    }

    public void newTree()
    {

    	maturity = 0;
    	currentTreeSettings = nextTreeSettings;
    	nextTreeSettings = new TreeSettings();
    	Destroy(treeStructure);
    	treeStructure = Instantiate(Resources.Load("TreePlant/TreeStructure"), transform.position + new Vector3(0, 0, zOffset), Quaternion.identity) as GameObject;
    	treeStructure.transform.parent = gameObject.transform;
    	treeStructureScript = treeStructure.GetComponent<TreeStructure>();
    	treeStructureScript.loadTreeSettings(currentTreeSettings);

    	initialPos = treeStructure.transform.position;
    }

    public void saveMeshAsset()
    {
    	Mesh m1 = treeStructure.GetComponent<MeshFilter>().mesh;
    	AssetDatabase.CreateAsset(m1, "Assets/" + "RIKLS1" + ".asset"); // saves to "assets/"
    	AssetDatabase.SaveAssets();
    }

    public void updateX(float x)
    {
    	transform.position = new Vector3(
    		initialPos.x + x,
    		initialPos.y,
    		initialPos.z);
    }

    public void updateY(float y)
    {
    	transform.position = new Vector3(
    		initialPos.x,
    		initialPos.y + y,
    		initialPos.z);
    }

    public void updateZ(float z)
    {
    	transform.position = new Vector3(
    		initialPos.x,
    		initialPos.y,
    		initialPos.z + z);
    }


    public void setTreeTrajectory(Vector3 traj){nextTreeSettings.treeTrajectory = traj;}
    public void setTreeDirectionWeight(float w){nextTreeSettings.treeDirectionWeight = w;}
    public void setTreeMaxHeight(float h){nextTreeSettings.treeMaxHeight = h;}
    public void setTreeMaxDepth(float d){nextTreeSettings.treeMaxDepth = (int) d;}
    public void setTreeMaxWidth(float w){nextTreeSettings.treeMaxWidth = w;}
    public void setTrunkExtend(bool e){nextTreeSettings.trunkExtend = e;}
    public void setTrunkPercent(float p){nextTreeSettings.trunkPercent = p;}
    public void setMaxNodeChanceToBranch(float c){nextTreeSettings.maxNodeChanceToBranch = c;}
    public void setMaxNumNodeBranches(float n){nextTreeSettings.maxNumNodeBranches = (int) n;}
    public void setMinBranchAngle(float a){nextTreeSettings.minBranchAngle = a;}
    public void setMaxBranchAngle(float a){nextTreeSettings.maxBranchAngle = a;}
    public void setBranchResolution(float r){nextTreeSettings.branchResolution = (int) r;}
    public void setBranchNodeMaxAngle(float a){nextTreeSettings.branchNodeMaxAngle = a;}
    public void setBranchSegLength(float l){nextTreeSettings.branchSegLength = l;}
    public void setBranchMinWidth(float w){nextTreeSettings.branchMinWidth = w;}
    public void setBranchMaxWidth(float w){nextTreeSettings.branchMaxWidth = w;}
    public void setBranchTrajectoryNoise(float t){nextTreeSettings.branchTrajectoryNoise = t;}
    public void setBranchTrajectoryWeight(float w){nextTreeSettings.branchTrajectoryWeight = w;}
    public void setBranchTrajectoryWeightVariation(float v){nextTreeSettings.branchTrajectoryWeightVariation = v;}

}