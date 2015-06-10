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
}