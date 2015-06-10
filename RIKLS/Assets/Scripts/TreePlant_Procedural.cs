using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreePlant_Procedural : Plant
{
	public GameObject treeStructure;
	//public GameObject rootStructure;
	//public List<GameObject> leaves;
	public TreeSettings treeSettings;

	public float zOffset = 0;

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

	void Start () {

		treeSettings = new TreeSettings();

		//setRandomValues();

		treeStructure = Instantiate(Resources.Load("TreePlant/TreeStructure"), transform.position + new Vector3(0, 0, zOffset), Quaternion.identity) as GameObject;
		treeStructure.transform.parent = gameObject.transform;
		treeStructure.GetComponent<TreeStructure>().loadTreeSettings(treeSettings);
	}

	public override void Update()
    {
        base.Update();

        //propogate maturity

        treeStructure.GetComponent<TreeStructure>().maturity = maturity;
        treeStructure.GetComponent<TreeStructure>().isMaturing = isMaturing;
    }

	private void setRandomValues()
    {
    	treeSettings.treeMaxHeight = Random.Range(1f, 4f);
    	treeSettings.treeDirectionWeight = Random.Range(0.5f, 1f);
    }

    public void setTreeTrajectory(Vector3 traj){treeSettings.treeTrajectory = traj;}
    public void setTreeDirectionWeight(float w){treeSettings.treeDirectionWeight = w;}
    public void setTreeMaxHeight(float h){treeSettings.treeMaxHeight = h;}
    public void setTreeMaxDepth(float d){treeSettings.treeMaxDepth = (int) d;}
    public void setTreeMaxWidth(float w){treeSettings.treeMaxWidth = w;}
    public void setTrunkExtend(bool e){treeSettings.trunkExtend = e;}
    public void setTrunkPercent(float p){treeSettings.trunkPercent = p;}
    public void setMaxNodeChanceToBranch(float c){treeSettings.maxNodeChanceToBranch = c;}
    public void setMaxNumNodeBranches(float n){treeSettings.maxNumNodeBranches = (int) n;}
    public void setMinBranchAngle(float a){treeSettings.minBranchAngle = a;}
    public void setMaxBranchAngle(float a){treeSettings.maxBranchAngle = a;}
    public void setBranchResolution(float r){treeSettings.branchResolution = (int) r;}
    public void setBranchNodeMaxAngle(float a){treeSettings.branchNodeMaxAngle = a;}
    public void setBranchSegLength(float l){treeSettings.branchSegLength = l;}
    public void setBranchMinWidth(float w){treeSettings.branchMinWidth = w;}
    public void setBranchMaxWidth(float w){treeSettings.branchMaxWidth = w;}
    public void setBranchTrajectoryNoise(float t){treeSettings.branchTrajectoryNoise = t;}
    public void setBranchTrajectoryWeight(float w){treeSettings.branchTrajectoryWeight = w;}
    public void setBranchTrajectoryWeightVariation(float v){treeSettings.branchTrajectoryWeightVariation = v;}


}