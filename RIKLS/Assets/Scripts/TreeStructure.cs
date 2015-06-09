using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

class TreeStructure : MonoBehaviour
{
	public Branch trunk;

	public float maturity;

	public int resolution = 6;
	public static float initialRadius = 0.02f;
	public float maxRadius = 0.02f;
	public float initialSegLength = 0f;
	public float maxSegLength = 0.1f;
	public float crookedFactor = 10f;

	private float ringRadians;

	public bool isGrowing = false;
	public bool debugDoneGrowing = false;
	public float growthRate = 0.1f;
	
	public float lengthGoal = 1f;
	public bool isMaturing = false;

	public bool skeletonExpanded = false;

	// debug draw values
	private float debugSphereSize = 0.005f;

	public List<Vector3> vertices;
	private List<int> triangles;
	private List<Vector2> uvs;

	private Mesh mesh;
	private Transform _transform; // cached transform to increase speeds
	private MeshRenderer meshRenderer;
	private Material branchMat;

	public TreePlant_Procedural.TreeSettings treeSettings;

	void Start()
	{
		meshRenderer = GetComponent<MeshRenderer>();
		branchMat = Resources.Load("Materials/TreePlant_Branch", typeof(Material)) as Material;
		meshRenderer.material = branchMat;

		_transform = transform;

		ringRadians = 2 * Mathf.PI / treeSettings.branchResolution;

		// initialize our mesh's data structures
		vertices = new List<Vector3>();
		triangles = new List<int>();
		uvs = new List<Vector2>();

		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.Clear();
		mesh.name = "TreeStructure";

		createInitialTreeSkeleton();
	}

	public void loadTreeSettings(TreePlant_Procedural.TreeSettings ts)
	{
		treeSettings = ts;
	}

	void Update()
	{
		if (isMaturing)
		{
			// update each Branch
			updateBranches(trunk, maturity);
			updateTreeSkeleton(trunk);
		}

		if (skeletonExpanded)
		{
			createMesh();
		}
		else
		{
			createMesh();
		}
	}

	// recursively call Update() on all branches. This is not automatic since they are not Script Components
	private void updateBranches(Branch b, float maturity)
	{
		b.UpdateBranch(maturity);

		foreach (Branch c in b.getChildren())
		{
			updateBranches(c, maturity);
		}
	}

	private void createInitialTreeSkeleton()
	{
		trunk = new Branch(treeSettings, Vector3.zero, Vector3.up);

		createMesh();
	}

	private void updateTreeSkeleton(Branch b)
	{
		if (b.getParent() != null)
		{
			// reposition the branch so it is still attached to the correct parent node.
			b.startPoint = b.getParent().skeleton[b.parentNode].startPoint;
		}

		foreach(Branch c in b.getChildren())
			{
				updateTreeSkeleton(c);
			}
	}
	
	private void createMesh()
	{

		// maybe do some error handling here to make sure that there is at least one segment!

		// just in case, clear things that should already be empty
		mesh.Clear();
		vertices.Clear();
		uvs.Clear();
		triangles.Clear();

		addTreeVerts(trunk);

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();

		for (int vert = 0; vert < vertices.Count; vert++)
		{
			uvs.Add(new Vector2(0,0));
		}

		mesh.uv = uvs.ToArray();
	}

	private void addTreeVerts(Branch b)
	{
		int res = treeSettings.branchResolution;
		int verticesStart = vertices.Count;

		// push the tip vertex
		vertices.Add(b.skeleton.Last().getNodeEndPoint());

		// push vertices for each ring

		for (int node = 0; node < b.skeleton.Count; node++)
		{
			Vector3 bisectAxis = Vector3.up;
			Vector3 prevSegAxis = Vector3.up;
			float bottomAngle = 0f;
			float bisectAngle = 0f;

			if (node > 0)
			{
				bisectAxis = Vector3.Cross(b.skeleton[node-1].direction, b.skeleton[node].direction).normalized;
				bisectAngle = Vector3.Angle(b.skeleton[node-1].direction, b.skeleton[node].direction) / 2f;
				bottomAngle = Vector3.Angle(b.skeleton[node-1].direction, Vector3.up);
				prevSegAxis = Vector3.Cross(b.skeleton[node-1].direction, Vector3.up);

				// debugString += "\n\t\taVec: " + vineSkeleton[node-1].direction.ToString("F8");
				// debugString += "\n\t\tbVec: " + vineSkeleton[node].direction.ToString("F8");
			}
			else
			{
				bisectAxis = Vector3.Cross(Vector3.up, b.skeleton[node].direction).normalized;
				bisectAngle = Vector3.Angle(Vector3.up, b.skeleton[node].direction) / 2f;
			}

			// float nodeLoc = b.getNodeLocation(node);
			// b.skeleton[node].radius = b.branchWidthFunction(nodeLoc) * b.getBranchThickness();

			for (int ringVert = 0; ringVert < res; ringVert++)
			{
				float angle = ringVert * ringRadians * -1;
				float v_x = b.skeleton[node].radius * Mathf.Cos(angle);
				float v_z = b.skeleton[node].radius * Mathf.Sin(angle);
				float v_y = 0;

				// I'm 99% sure that the "+ _transform.position" part should not be there... test this some time.
				Vector3 relativeVec = new Vector3(v_x, v_y, v_z);

				relativeVec = Quaternion.AngleAxis(-bottomAngle, prevSegAxis) * relativeVec;

				relativeVec = Quaternion.AngleAxis(bisectAngle, bisectAxis) * relativeVec;

				vertices.Add(b.skeleton[node].startPoint + relativeVec);
			}
		}
		addBranchTriangles(b, verticesStart);

		foreach (Branch c in b.getChildren())
		{
			addTreeVerts(c);
		}
	}

	private void addBranchTriangles(Branch b, int startIndex)
	{
		int res = treeSettings.branchResolution;

		for (int node = 0; node < b.skeleton.Count; node++)
		{
			for (int faceNum = 0; faceNum < res; faceNum++)
			{
				//check if we're at a segment, or at the point
				if (node < b.skeleton.Count - 1)
				{
					// this is a segment.
					// we will draw two triangles for each rectangular face created between this ring and the next ring

					int bottomLeft = startIndex + (node * res) + faceNum + 1;
					int bottomRight = startIndex + (node * res) + ((faceNum + 1) % res) + 1;
					int topLeft = bottomLeft + res;
					int topRight = bottomRight + res;

					// add first triangle's vertices
					triangles.Add(bottomLeft);
					triangles.Add(topRight);
					triangles.Add(topLeft);

					// add second triangle's vertices
					triangles.Add(bottomLeft);
					triangles.Add(bottomRight);
					triangles.Add(topRight);
				}
				else
				{
					// this is the point.
					// we will draw one triangle for each face between this ring and the tip

					int bottomLeft = startIndex + (node * res) + faceNum + 1;
					int bottomRight = startIndex + (node * res) + ((faceNum + 1) % res + 1);
					int top = startIndex + 0;

					// add the triangle's vertices
					triangles.Add(bottomLeft);
					triangles.Add(bottomRight);
					triangles.Add(top);
				}
			}
		}
	}
	
	/*
	public float getTotalLength()
	{}
	*/

	void OnDrawGizmos()
	{
		// Avoid NPE in the editor when the game isn't running,
		// (and therefore the object hasn't been initialized).
		if (_transform)
		{
			if (trunk != null)
			{
				// recursively draw tree
				//drawTreeSkeletonGizmos(trunk, mPos);
			}
		}	
	}

	void drawTreeSkeletonGizmos(Branch b, Vector3 mPos)
	{
		// recursive draw function

		for (int node = 0; node < b.skeleton.Count; node++)
		{
			Vector3 nStart = b.skeleton[node].startPoint;
			Vector3 nEnd = b.skeleton[node].getNodeEndPoint();

			// Draw segment endpoints with red spheres
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(mPos + nStart, debugSphereSize);

			// Draw segments with lines. Alternate colors for each segment,
			// but keep the tip segment red (or it swaps colors weirdly)
			if (b.getDepth() == 0)
			{
				Gizmos.color = Color.red;
			}
			else if (b.getDepth() == 1)
			{
				Gizmos.color = Color.blue;
			}
			else if (b.getDepth() == 2)
			{
				Gizmos.color = Color.white;
			}
			else if (b.getDepth() == 3)
			{
				Gizmos.color = Color.green;
			}
			else
			{
				Gizmos.color = Color.yellow;
			}
			
			Gizmos.DrawLine(mPos + nStart, mPos + nEnd);
		}

		// Draw the tip as another segment endpoint
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(mPos + b.skeleton.Last().getNodeEndPoint(), debugSphereSize);

		foreach (Branch childBranch in b.children)
		{
			drawTreeSkeletonGizmos(childBranch, mPos);
		}
	}
}