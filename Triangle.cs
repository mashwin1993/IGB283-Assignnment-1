using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle : MonoBehaviour
{
    //Public Variables

    //Locations
    public Vector2 pos1;
    public Vector2 pos2;

    GameObject point1;
    GameObject point2;

    Transform currentTarget;
    Point targetPoint;

    public float size = 1;
    public Vector2 scale;

    public Vector2 direction;
    public float speed;

    public GameObject target;

    //Materials
    public Material material;
    public Mesh mesh;

    //Offset for Rotation
    private Vector3 offset;

    //Public Rotation Angle Attach to slider
    public float RotateAngle;

    public Vector3 centre {
        get { return mesh.bounds.center; }
    }

    //Tranform
    IGB283Transform meshTransform = new IGB283Transform();

    // Use this for initialization
    void Start()
    {

        Draw();

        meshTransform.Initialise(mesh);
        meshTransform.Translate(pos1);

        point1 = PointAt(pos1);
        point2 = PointAt(pos2);

        SwitchTarget();
    }

    // Update is called once per frame
    void Update()
    {
        if (ReachedTarget()) {
            SwitchTarget();
        }

        direction = targetPoint.centre - centre;


        meshTransform.Translate(direction.normalized * speed * Time.deltaTime);

        meshTransform.Scale(scale);
    }


    // Draw a rectangle
    public void Draw()
    {

        //Adding MeshFilter and MeshRenderer  to Empty GameObject
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();

        // Get Mesh from the MeshFilter
        mesh = GetComponent<MeshFilter>().mesh;

        //Set the material to selected material 
        GetComponent<MeshRenderer>().material = material;

        //Clear all data from the mesh
        mesh.Clear();

        //Set triangle points 
        mesh.vertices = new Vector3[]
        {
            new Vector3(0, 0, size),
            new Vector3(0, size, size),
            new Vector3(size, size, size),
            new Vector3(size, 0, size)
        };

        // Set colours of the triangle
        mesh.colors = new Color[]
        {
            new Color(1.0f, 1.0f, 1.0f, 1.0f),
            new Color(1.0f, 1.0f, 1.0f, 1.0f),
            new Color(1.0f, 1.0f, 1.0f, 1.0f),
            new Color(1.0f, 1.0f, 1.0f, 1.0f)
        };


        // Set vertex indicies
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };

        // Calculate the bounds of the rectangle
        offset.x = mesh.bounds.size.x / 2;
        offset.y = mesh.bounds.size.y / 2;
    }

    GameObject PointAt(Vector2 pos) {
        GameObject point = Instantiate(new GameObject("Point"), transform);
        Point script = point.AddComponent<Point>();
        script.Initialise(pos, material);
        return point;
    }

    void SwitchTarget() {
        if (!currentTarget || currentTarget == point1.transform) {
            currentTarget = point2.transform;
        } else {
            currentTarget = point1.transform;
        }

        targetPoint = currentTarget.GetComponent<Point>();
    }

    bool ReachedTarget() {
        return Vector3.Distance(centre, targetPoint.centre) < speed * Time.deltaTime;
    }
}