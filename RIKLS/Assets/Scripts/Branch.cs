using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Don't extend MonoBehavious because this script will never be attached to a GameObject.
// It's simply an Object that will be used by another script.
public class Branch
{
	public class BranchNode
	{
		public float radius;
		public float length;
		public Vector3 startPoint;
		public Vector3 direction;

		public BranchNode(float rad, float magnitude, Vector3 start, Vector3 normalizedRay)
		{
			radius = rad;
			length = magnitude;
			startPoint = start;
			direction = normalizedRay;
		}

		public Vector3 getNodeRay()
		{
			return direction * length;
		}

		public Vector3 getNodeEndPoint()
		{
			return startPoint + getNodeRay();
		}
	}

	public List<BranchNode> skeleton;
	public Branch parent;
	public int parentNode;
	public List<Branch> children;
	protected int depth;
	public Vector3 startPoint;
	public Vector3 direction;
	public Vector3 trajectory;
	public float maxNodeAngle;
	public float branchMaturity;
	public float lengthGoal;
	public float widthGoal;
	protected float growthRate = 0.05f;
	protected float growthStart = 0;
	protected float thicknessStart = 0;
	public bool isGrowing = false;
	protected float initialRadius = 0.02f;
	protected float thickness;
	public float chanceToBranch = 0.25f;
	public int randomBranchingFactor = 3;
	public float randomBranchAngleFactor = 30f;
	public float treeMaturityStart = 0f;
	public float currentTreeMaturity = 0f;
	public int maxDepth;

	public TreePlant_Procedural.TreeSettings treeSettings;

	// parameterless constructor for child classes
	public Branch()
	{
		skeleton = new List<BranchNode>();

		parent = null;
		parentNode = -1;
		children = new List<Branch>();
		depth = 0;
		maxNodeAngle = 5f;
		thickness = 0;
	}

	// "trunk" branch constructor
	public Branch(TreePlant_Procedural.TreeSettings ts, Vector3 start, Vector3 dir, int md = -1)
	{
		// since there is no parent branch, we need a start point for this branch

		skeleton = new List<BranchNode>();

		parent = null;
		parentNode = -1;
		children = new List<Branch>();
		depth = 0;
		treeSettings = ts;
		startPoint = start;
		direction = dir;
		trajectory = direction;
		maxNodeAngle = 5f;
		lengthGoal = treeSettings.treeMaxHeight;
		thickness = 0;
		if (md == -1) {maxDepth = treeSettings.treeMaxDepth;}

		// only do this for the trunk
		widthGoal = treeSettings.treeMaxWidth;

		skeleton.Add(new BranchNode(thickness, 0, startPoint, direction));
	}

	/*

	// Platform Branch constructor
	public Branch(TreePlant_Procedural.TreeSettings ts, Vector3 start, Vector3 dir)
	{

		// since there is no parent branch, we need a start point for this branch

		skeleton = new List<BranchNode>();

		parent = null;
		parentNode = -1;
		children = new List<Branch>();
		depth = 0;
		treeSettings = ts;
		startPoint = start;
		direction = dir;
		trajectory = treeSettings.treeTrajectory;
		// determine maxNodeAngle
		maxNodeAngle = 5f;
		lengthGoal = treeSettings.treeMaxHeight;
		thickness = 0;	
	}

	*/

	// standard branch constructor
	public Branch(TreePlant_Procedural.TreeSettings ts, Branch parentBranch, Vector3 dir, float tms, int node = -1)
	{
		// parentBranch determines what branch this branch will... branch... off of.
		// node determines where along the parent branch this branch will protrude from
		// we determine the actual start point from the parent branch's node

		skeleton = new List<BranchNode>();

		parent = parentBranch;
		parentNode = node;

		// -1 is code for "the end of the branch"
		if (parentNode == -1)
		{
			int count = parent.skeleton.Count;
			if (count > 1)
			{
				parentNode = count - 2;
			}
			else
			{
				parentNode = 0;
			}
		}

		children = new List<Branch>();
		depth = parent.getDepth() + 1;
		treeSettings = ts;
		startPoint = parent.skeleton[parentNode].startPoint;
		direction = dir;

		// determine trajectory
		Vector3 crossWith = Vector3.up;
		if (crossWith == treeSettings.treeTrajectory)
		{
			crossWith = Vector3.right;
		}
		Vector3 axis = Vector3.Cross(direction, treeSettings.treeTrajectory);
		axis = Quaternion.AngleAxis(Random.Range(0, 360f), treeSettings.treeTrajectory) * axis;
		float rotAmount = Random.Range(0, treeSettings.branchTrajectoryNoise);

		trajectory = Quaternion.AngleAxis(rotAmount, axis) * treeSettings.treeTrajectory;

		// determine maxNodeAngle
		float variedWeight = Random.Range(ts.branchTrajectoryWeight - ts.branchTrajectoryWeight * ts.branchTrajectoryWeightVariation,
		                                  ts.branchTrajectoryWeight + ts.branchTrajectoryWeight * ts.branchTrajectoryWeightVariation);
		maxNodeAngle = ts.branchNodeMaxAngle * variedWeight;

		// determine lengthGoal
		float plr = parent.getLengthRemaining(parentNode);
		lengthGoal = Random.Range(0.5f * plr, 1.2f * plr);
		thickness = treeSettings.branchMaxWidth / (depth + 1);
		maxDepth = treeSettings.treeMaxDepth;

		skeleton.Add(new BranchNode(thickness, 0, startPoint, direction));

		// branchMaturity = Mathf.Clamp01(getLength() / lengthGoal);
		treeMaturityStart = tms;
	}

	public virtual void UpdateBranch(float treeMaturity)
	{
		currentTreeMaturity = treeMaturity;
		bool wasGrowing = isGrowing;
		// update vine skeleton structure (such as adding a new segment)
		if (getLength() < lengthGoal)
		{
			if (currentTreeMaturity < 0.5f || parent != null)
			{
				grow();
			}
			else
			{
				growFromBottom();
			}
			
			isGrowing = true;
		}
		else if (wasGrowing)
		{
			isGrowing = false;
		}

		updateSkeleton(skeleton);
	}

	protected void grow()
	{

		// Use linear interpolation to determine what length the branch should be,
		// at this stage in the tree's maturity.
		// All branches must be done growing at treeMaturity == 1.0.
		// Therefore, branches spawned later must grow at a faster relative rate.

		// First determine the local branch's maturity.
		// (Maybe this should be done in update)

		branchMaturity = (currentTreeMaturity - treeMaturityStart) / (1.0f - treeMaturityStart);
		float newBranchLength = Mathf.Lerp(0, lengthGoal, branchMaturity);

		float newGrowth = newBranchLength - getLength();

		// grow the branch, creating as many new segments as needed.

		while (newGrowth > 0)
		{
			float amountToGrow = Mathf.Min(newGrowth, treeSettings.branchSegLength);
			newGrowth -= amountToGrow;

			// Extend the length of the branch.
			// The segment before the tip ring will be extended. If it reaches its max length,
			// then a new segment will be added, and the overflow growth distance
			// will be its initial length.

			int growIndex = skeleton.Count - 2;

			// if the only segment is the tip segment, then we need to start fresh on a new one.
			if (skeleton.Count == 1)
			{
				addSegment(treeSettings.branchMinWidth / (depth + 1), amountToGrow, determineSegDirection(skeleton.Last().direction));
				//Debug.Log("Creating first non-tip segment");
			}
			else
			{
				float currentLength = skeleton[growIndex].length;
				float roomLeftToGrow = treeSettings.branchSegLength - currentLength;

				if (amountToGrow <= roomLeftToGrow)
				{
					skeleton[growIndex].length += amountToGrow;
				}
				else
				{
					skeleton[growIndex].length = treeSettings.branchSegLength;
					float overflow = amountToGrow - roomLeftToGrow;
					addSegment(treeSettings.branchMinWidth / (depth + 1), overflow, determineSegDirection(skeleton.Last().direction));
				}
			}
		}

		// Increase the thickness of the branch.
		// This will be largely dependent on the thickness of the parent branch,
		// more specifically the parent node this branch protrudes from.
		if (parent == null)
		{
			thickness = Mathf.Lerp(0, treeSettings.treeMaxWidth, branchMaturity);
		}

		else
		{
			thickness = parent.getNodeThickness(parentNode);
		}

		//updateSkeleton(skeleton);
	}

	protected void growFromBottom()
	{
		// Use linear interpolation to determine what length the branch should be,
		// at this stage in the tree's maturity.
		// All branches must be done growing at treeMaturity == 1.0.
		// Therefore, branches spawned later must grow at a faster relative rate.

		// First determine the local branch's maturity.
		// (Maybe this should be done in update)

		branchMaturity = (currentTreeMaturity - treeMaturityStart) / (1.0f - treeMaturityStart);
		float newBranchLength = Mathf.Lerp(0, lengthGoal, branchMaturity);

		float newGrowth = newBranchLength - getLength();

		// Extend the length of the branch.
		// The segment before the tip ring will be extended. If it reaches its max length,
		// then a new segment will be added, and the overflow growth distance
		// will be its initial length.

		int growIndex = 1;

		// should probably throw an error if newGrowth > treeSettings.branchSegLength
		if (newGrowth > treeSettings.branchSegLength)
		{
			Debug.Log("Whoops, newGrowth > treeSettings.branchSegLength in growFromBottom(). We should do something to handle this case. " + newGrowth + " > " + treeSettings.branchSegLength);
		}

		// trim the new growth if our vine is overshooting the total length goal
		if (getLength() + newGrowth > lengthGoal)
		{
			newGrowth = lengthGoal - getLength();
			growthStart = lengthGoal;
		}

		float currentLength = skeleton[growIndex].length;
		float newSegLength = currentLength + newGrowth;

		if (newSegLength < treeSettings.branchSegLength)
		{
			skeleton[growIndex].length = newSegLength;
		}
		else
		{
			skeleton[growIndex].length = treeSettings.branchSegLength;
			float overflow = newSegLength - treeSettings.branchSegLength;
			insertSegment(growIndex, treeSettings.branchMinWidth / (depth + 1), overflow, determineSegDirection(skeleton.Last().direction));

			// increment all children's parentNode index
			foreach (Branch c in children)
			{
				c.parentNode++;
			}
			//Debug.Log("Segment overflow (" + newSegLength + "). segment " + growIndex + " maxed out at " + skeleton[growIndex].length + ", so a new node is created with length " + overflow);
		}

		// Increase the thickness of the branch.
		// This will be largely dependent on the thickness of the parent branch,
		// more specifically the parent node this branch protrudes from.
		if (parent == null)
		{
			thickness = Mathf.Lerp(0, treeSettings.treeMaxWidth, branchMaturity);
		}

		else
		{
			thickness = parent.getNodeThickness(parentNode);
		}

		//updateSkeleton(skeleton);
	}

	protected virtual void addSegment(float rad, float magnitude, Vector3 direction)
	{
		// since the tip is always of uniform length, we are actually adding a new tip,
		// and shrinking the previous end segment. It can now grow to its full length,
		// and then the process will start again.

		if (magnitude > treeSettings.branchSegLength)
		{
			Debug.Log("Whoops, magnitude > treeSettings.branchSegLength in addSegment.\n" + magnitude + ">" + treeSettings.branchSegLength);
		}
		float tipLength = skeleton.Last().length;
		skeleton.Last().length = magnitude;
		BranchNode newNode = new BranchNode(rad, tipLength, skeleton.Last().startPoint + skeleton.Last().getNodeRay(), direction);
		skeleton.Add(newNode);

		if (Random.Range(0, 1f) < treeSettings.maxNodeChanceToBranch
			&& depth < (maxDepth - 1)
			&& skeleton.Count > 2)
		{
			growRandomChildren(skeleton.Count - 2);
		}

		//expandMesh();
	}

	protected void insertSegment(int index, float rad, float magnitude, Vector3 direction)
	{
		// since the tip is always of uniform length, we are actually adding a new tip,
		// and shrinking the previous end segment. It can now grow to its full length,
		// and then the process will start again.

		if (magnitude > treeSettings.branchSegLength)
		{
			Debug.Log("Whoops, magnitude > treeSettings.branchSegLength in insertSegment(). We should do something to handle this case.");
		}

		BranchNode newNode = new BranchNode(rad, magnitude, skeleton.Last().startPoint + skeleton.Last().getNodeRay(), direction);
		skeleton.Insert(index, newNode);

		//expandMesh(); 
	}

	protected Vector3 determineSegDirection(Vector3 pDirection)
	{
		/*

		Determine the axis of rotation. 
		This shouldn't always point us directly where we need to go, it should be noisy.
		Probably based on treeSettings.branchTrajectoryWeightVariation.
		*/

		Vector3 cDirection;

		Vector3 axis = Vector3.Cross(pDirection, trajectory);
		float maxAxisVariation = 20;
		axis = Quaternion.AngleAxis(Random.Range(-maxAxisVariation, maxAxisVariation), pDirection) * axis;

		float rotAmount = Random.Range(0, maxNodeAngle);

		cDirection = Quaternion.AngleAxis(rotAmount, axis) * pDirection;

		return cDirection;
	}

	protected virtual void growRandomChildren(int parentBranchNode)
	{
		int numChildren = Random.Range(1, treeSettings.maxNumNodeBranches);
		float angleStart = Random.Range(0, 360);
		float branchAngle = Random.Range(treeSettings.minBranchAngle, treeSettings.maxBranchAngle);

		for (int b = 0; b < numChildren; b++)
		{
			Vector3 direction = skeleton[parentBranchNode].direction;
			Vector3 crossWith = Vector3.up;
			if (crossWith == direction)
			{
				crossWith = Vector3.right;
			}
			Vector3 axis = Vector3.Cross(direction, crossWith);
			axis = Quaternion.AngleAxis((360f * (float)b / (float)numChildren) + angleStart, direction) * axis;

			direction = Quaternion.AngleAxis(branchAngle, axis) * direction;

			addChild(dir : direction);
		}
	}

	protected virtual void updateSkeleton(List<BranchNode> skel)
	{
		/*
		// Iterate through all the nodes and make sure the start points correspond 
		// to the ends of the previous nodes.
		b[0].startPoint = startPoint;

		if (b.Count > 1)
		{
			for (int node = 1; node < b.Count; node++)
			{
				b[node].startPoint = b[node - 1].startPoint + b[node - 1].getNodeRay();
			}
		}
		*/

		// iterate through all nodes in the skeleton
		for (int n = 0; n < skel.Count; n++)
		{
			////////////////////////////////////////////////////////////////////////////
			// Make sure all start points correspond to the end of the previous nodes //
			////////////////////////////////////////////////////////////////////////////

			// node 0 starts at the branch's start point
			if (n == 0) {skel[0].startPoint = startPoint;}
			else if (skel.Count > 1)
			{
				skel[n].startPoint = skel[n-1].getNodeEndPoint();
			}

			/////////////////////////////////////////////////////////////////////////////
			//         Update each node's radius to match the branch shape        ///////
			/////////////////////////////////////////////////////////////////////////////

			skel[n].radius = branchWidthFunction(getNodeLocation(n)) * getBranchThickness();

		}
	}

	public Branch getParent()
	{
		// getter
		return parent;
	}

	public void setParent(Branch parentNode)
	{
		// setter
		parent = parentNode;
	}

	public List<Branch> getChildren()
	{
		// getter
		return children;
	}

	public virtual void addChild(Vector3 dir, int node = -1)
	{
		Branch newChild = new Branch(treeSettings, this, dir, currentTreeMaturity, node);
		children.Add(newChild);
	}

	public int getDepth()
	{
		return depth;
	}

	public float getLength()
	{
		if (skeleton != null)
		{
			float len = 0f;

			for (int node = 0; node < skeleton.Count; node++)
			{
				len += skeleton[node].length;
			}
			return len;
		}
		else
		{
			Debug.Log("Can not get length of branch before it's been created!");

			return 0f;
		}
	}

	public float getLengthRemaining(int node)
	{
		float lengthToNode = 0;

		if (skeleton != null)
		{

			for (int n = 0; n < node; n++)
			{
				lengthToNode += skeleton[n].length;
			}

			return lengthGoal - lengthToNode;
		}
		else
		{
			Debug.Log("Can not get length of branch before it's been created!");

			return 0f;
		}
	}

	public float getNodeLocation(int node)
	{
		// returns a value between 0 and 1 to represent the node's loaction along the leaf's length.
		// 0 is the base, 1 is the tip.
		// this function is mainly used to determine the node's width.
		float branchLength = getLength();
		float location = 0f;
		float nodeDistance = 0f;

		for (int n = 0; n < node; n++)
		{
			nodeDistance += skeleton[n].length;
		}

		location = nodeDistance / branchLength;

		return location;

	}

	public float branchWidthFunction(float x)
	{
		// x is a number between 0 and 1 defining the node's position along the branch


		// Function defining the branch's shape
		float y = -1.0f * Mathf.Sqrt(x) + 1.0f;

		// return the coefficient for the branch's width

		return y;
	}

	public virtual float getBranchThickness()
	{
		// this will be the thickness at the base of the branch
		return thickness;
	}

	public float getNodeThickness(int node)
	{
		return skeleton[node].radius;
	}
}