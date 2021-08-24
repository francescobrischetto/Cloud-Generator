    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cloud_Generator : MonoBehaviour
{
    [Range(0.1f, 1.0f)]
    //The lenght of the side of the cube to subdivide the space
    public float discreteStep = 0.25f;
    [Range(0.0f, 1.0f)]
    public float threshold = 0.5f;
    [Range(1, 15)]
    //points to evaluate inside the discrete cube to check if the mean value is above the threshold
    public int pointsToEvaluate = 5;
    public GameObject objectToSpawn;
    public GameObject optionalSecondToSpawn;
    public int RandomSeed = 0;

    private List<Vector3> drawedPositions;
    void Start()
    {
        //Generate Clouds for the first time with given seed if any
        if (RandomSeed == 0) RandomSeed = (int)System.DateTime.Now.Ticks;
        Random.InitState(RandomSeed);
        GenerateClouds();
    }

    public void GenerateClouds()
    {
        drawedPositions = new List<Vector3>();

        //Discretization of the current space, progressing with Cloud localScale step until we cover the whole SkyClouds local space
        for (float x = 0.0f; x < transform.localScale.x; x += discreteStep)
        {
            for (float y = 0.0f; y < transform.localScale.y; y += discreteStep)
            {
                for (float z = 0.0f; z < transform.localScale.z; z += discreteStep)
                {
                    //We evaluate up to k points and see the medium perlinNoise inside
                    float mean = 0;
                    for (int k = 0; k < pointsToEvaluate; k++)
                    {

                        Vector3 randomPos = new Vector3(Random.Range(x, x + discreteStep),
                            Random.Range(y, y + discreteStep),
                            Random.Range(z, z + discreteStep));
                        mean += perlinNoise.PerlinNoise3D(randomPos, discreteStep);

                    }
                    mean /= pointsToEvaluate;
                    //If the mean is above the threshold we instantiate a cloud in his cube position
                    if (mean > threshold)
                    {
                        //This is where to spawn the cloud
                        Vector3 positionEvaluate = new Vector3(transform.position.x - transform.localScale.x / 2 + x + discreteStep / 2, 
                                                               transform.position.y - transform.localScale.y / 2 + y + discreteStep / 2, 
                                                               transform.position.z - transform.localScale.z / 2 + z + discreteStep / 2);
                        drawedPositions.Add(positionEvaluate);
                        //optionally working with two different cloud models
                        GameObject cloud;
                        if (optionalSecondToSpawn != null)
                        {
                            cloud = Random.value < 0.5 ? Instantiate(objectToSpawn, positionEvaluate, Quaternion.identity) : Instantiate(optionalSecondToSpawn, positionEvaluate, Quaternion.identity);
                        }
                        else
                        {
                            cloud = Instantiate(objectToSpawn, positionEvaluate, Quaternion.identity); 
                        }
                        
                        //We set this cloud Parent to our gameObject (to easily combine them at the end)
                        cloud.transform.SetParent(transform);
                    }
                }
            }
        }
        //After instantiating all the clouds we merge them togheter in one single mesh (also for performance reason)
        CombineMeshes();
    }

    public void CombineMeshes()
    {
        //Temporarily set position to zero and scale to one, to make matrix math easier
        Vector3 scale = transform.localScale;
        transform.localScale = Vector3.one;
        Vector3 position = transform.position;
        transform.position = Vector3.zero;

        //Getting all mesh filters (even the parent one)
        MeshFilter[] meshFilters = transform.gameObject.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length-1];
        int i = 1;
        //We start Iterating from 1 because the first meshFilter is the parent one.
        while (i < meshFilters.Length)
        {
            combine[i-1].mesh = meshFilters[i].sharedMesh;
            combine[i - 1].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }

        //We combine all the Meshes togheter in the parent one)
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, true);
        transform.gameObject.SetActive(true);

        //Return the parent to original position and scale
        transform.position = position;
        transform.localScale = scale;

        //Destroy all the child clouds
        i = 1;
        while (i < meshFilters.Length)
        {
            Destroy(meshFilters[i].gameObject);
            i++;
        }
    }
    
    public void RegenerateClouds()
    {
        RandomSeed = (int)System.DateTime.Now.Ticks;
        Random.InitState(RandomSeed);
        GenerateClouds();
    }

void Update()
    {   
        //On mouse click regenerate Clouds (with another seed)
        if (Input.GetMouseButtonDown(0))
        {
            RegenerateClouds();
        }

        //On Z and X keypress we update the threshold
        if (Input.GetKey(KeyCode.Z))
        {
            threshold = Mathf.Clamp(threshold - 0.05f, 0.0f, 1.0f);
            GenerateClouds();
        }
        if (Input.GetKey(KeyCode.X))
        {
            threshold = Mathf.Clamp(threshold + 0.05f, 0.0f, 1.0f);
            GenerateClouds();
        }
    }
    void OnDrawGizmosSelected()
    {
        // Draw a semitransparent blue cube at the transforms position
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        foreach (Vector3 pos in drawedPositions)
        {
            Gizmos.DrawCube(pos, new Vector3(discreteStep, discreteStep, discreteStep));
        }
    }
}
