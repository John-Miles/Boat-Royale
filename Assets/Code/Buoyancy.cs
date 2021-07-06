using System;
using UnityEngine;

// Cams mostly hack buoyancy
public class Buoyancy : MonoBehaviour
{
	public float splashVelocityThreshold;
	public float forceScalar;
	public float waterLineHack; // HACK

	public int underwaterVerts;
	public float dragScalar;

	public static event Action<GameObject, Vector3, Vector3> OnSplash;
	public static event Action<GameObject> OnDestroyed;

	Vector3 worldVertPos;
	
	//my variables and hopeful fixes
	private Rigidbody rb;
	private Vector3[] meshNormals;
	private Vector3[] meshVertices;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		meshNormals = GetComponent<MeshFilter>().mesh.normals;
		meshVertices = GetComponent<MeshFilter>().mesh.vertices;
		
	}

	void FixedUpdate()
	{
		CalculateForces();
	}

	private void CalculateForces()
	{
		underwaterVerts = 0;

		for (var index = 0; index < meshNormals.Length; index++)
		{
			worldVertPos = transform.position + transform.TransformDirection(meshVertices[index]);
			if (worldVertPos.y < waterLineHack)
			{
				
				Vector3	forceAmount = (transform.TransformDirection(-meshNormals[index]) * forceScalar) * Time.deltaTime;
				Vector3 forcePosition = transform.position + transform.TransformDirection(meshVertices[index]);
				rb.AddForceAtPosition(forceAmount, forcePosition, ForceMode.Force);
				underwaterVerts++;
			}
			// HACK to remove sunken boats
			if (worldVertPos.y < waterLineHack - 10f)
			{
				DestroyParentGO();
				break;
			}
			
		}
		
		// Splashes only on surface of water plane
		if (worldVertPos.y > waterLineHack - 0.1f)
		{
			if (rb.velocity.magnitude > splashVelocityThreshold || rb.angularVelocity.magnitude > splashVelocityThreshold)
			{
				if (OnSplash != null)
				{
					OnSplash.Invoke(gameObject, worldVertPos, rb.velocity);
				}
			}
		}
		// Drag for percentage underwater
		rb.drag = (underwaterVerts / (float)meshVertices.Length) * dragScalar;
		rb.angularDrag = (underwaterVerts / (float)meshVertices.Length) * dragScalar;
	}

	private void DestroyParentGO()
	{
		if (OnDestroyed != null)
		{
			OnDestroyed.Invoke(gameObject);
		}
		Destroy(gameObject);
	}
}
