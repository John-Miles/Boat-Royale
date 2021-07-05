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


	void FixedUpdate()
	{
		CalculateForces();
	}

	private void CalculateForces()
	{
		underwaterVerts = 0;

		for (var index = 0; index < GetComponent<MeshFilter>().mesh.normals.Length; index++)
		{
			worldVertPos = transform.position + transform.TransformDirection(GetComponent<MeshFilter>().mesh.vertices[index]);
			if (worldVertPos.y < waterLineHack)
			{
				// Splashes only on surface of water plane
				if (worldVertPos.y > waterLineHack - 0.1f)
				{
					if (GetComponent<Rigidbody>().velocity.magnitude > splashVelocityThreshold || GetComponent<Rigidbody>().angularVelocity.magnitude > splashVelocityThreshold)
					{
						if (OnSplash != null)
						{
							OnSplash.Invoke(gameObject, worldVertPos, GetComponent<Rigidbody>().velocity);
						}
					}
				}
				Vector3	forceAmount = (transform.TransformDirection(-GetComponent<MeshFilter>().mesh.normals[index]) * forceScalar) * Time.deltaTime;
				Vector3 forcePosition = transform.position + transform.TransformDirection(GetComponent<MeshFilter>().mesh.vertices[index]);
				GetComponent<Rigidbody>().AddForceAtPosition(forceAmount, forcePosition, ForceMode.Force);
				underwaterVerts++;
			}
			// HACK to remove sunken boats
			if (worldVertPos.y < waterLineHack - 10f)
			{
				DestroyParentGO();
				break;
			}
			// Drag for percentage underwater
			GetComponent<Rigidbody>().drag = (underwaterVerts / (float)GetComponent<MeshFilter>().mesh.vertices.Length) * dragScalar;
			GetComponent<Rigidbody>().angularDrag = (underwaterVerts / (float)GetComponent<MeshFilter>().mesh.vertices.Length) * dragScalar;
		}
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
