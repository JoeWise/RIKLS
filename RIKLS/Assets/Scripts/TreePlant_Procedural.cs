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

		public Vector3 treeTrajectory = Vector3.up;
		// random
		public float treeDirectionWeight = 0.8f;
		// random
		public float treeMaxHeight = 10f;
		public int treeMaxDepth = 3;
		public float treeMaxWidth = 0.3f;
		// random
		public float maxNodeChanceToBranch = 0.35f;
		// random
		public int maxNumNodeBranches = 3;
		// random
		public float minBranchAngle = 15f;
		public float maxBranchAngle = 40f;

		public float maxPlatformBranchLength = 2f;

		public int branchResolution = 5;
		public float branchNodeMaxAngle = 40f;
		public float branchSegLength = 0.3f;
		public float branchMaxLength = 1f;
		public float branchGrowthRate = 0.1f;
		public float branchMinWidth = 0.00f;
		public float branchMaxWidth = 0.04f;
		public float branchCurviness = 10f;
		public float branchTrajectoryNoise = 45f;
		public float branchTrajectoryWeight = 0.5f;
		public float branchTrajectoryWeightVariation = 0;

		public int platformResolution = 5;
		public int platformMaxDepth = 2;
		public float platformMaxLength = 2f;
		public float platformMaxChildLength = .5f;
		public float platformMaxThickness = 0.15f;
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