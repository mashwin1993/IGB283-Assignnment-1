using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Triangle : MonoBehaviour
{
    //Public Variables

    public bool reshape;
    public int sides = 3;

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
    //Speed Slider
    public Slider SpeedSlider;

    //Materials
    public Material material;
    public Mesh mesh;

    //Public Rotation Angle Attach to slider
    public float RotateAngle;

    //Rotation Slider
    public Slider RotateSlider;

    //Color Variables
    new Color color1;
    new Color color2;


    public Vector3 centre
    {
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

        SpeedSlider.value = 1.0f;

        SwitchTarget();
    }

    // Update is called once per frame
    void Update()
    {
        if (reshape) {
            reshape = false;
            Reshape(sides);
        }

        if (ReachedTarget())
        {
            SwitchTarget();
        }

        direction = targetPoint.centre - centre;

        if(SpeedSlider)
            speed = SpeedSlider.value;

        if (RotateSlider)
            RotateAngle = RotateSlider.value;

        meshTransform.Translate(direction.normalized * speed * Time.deltaTime);

        //Maybe do something with scaling?
        meshTransform.Scale(scale);

        meshTransform.RotateCenter(RotateAngle);

        //Color Changing with Lerp

        //Probably should initialise as variables so getcomponent isn't called each update
        color1 = point1.GetComponent<MeshRenderer>().material.color;
        color2 = point2.GetComponent<MeshRenderer>().material.color;

        float totalDist = Vector3.Distance(point1.GetComponent<Point>().centre, point2.GetComponent<Point>().centre);
        float remDist = Vector3.Distance(centre, point2.GetComponent<Point>().centre);

        float t = (totalDist - remDist) / totalDist;

        gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(color1, color2, t);
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

        //Make new mesh for object
        Reshape(sides);
    }

    GameObject PointAt(Vector2 pos)
    {
        GameObject point = Instantiate(new GameObject("Point"), transform);
        Point script = point.AddComponent<Point>();
        script.Initialise(pos, material);
        return point;
    }

    void SwitchTarget()
    {
        //Initialise/Switch between targets
        if (!currentTarget || currentTarget == point1.transform)
        {
            currentTarget = point2.transform;
        }
        else
        {
            currentTarget = point1.transform;
        }

        //Get target's script for easier reference
        targetPoint = currentTarget.GetComponent<Point>();
    }

    bool ReachedTarget()
    {
        //Confirm whether the target is in touching distance of the frame
        return Vector3.Distance(centre, targetPoint.centre) < speed * Time.deltaTime;
    }

    void Reshape(int sides) {
        //Reject values too low or high
        if (sides < 3 || sides > 50) {
            Debug.LogErrorFormat("{0} is an invalid number of sides for the shape to have.", sides);
            return;
        }

        //Get the current position to move the new mesh to
        Vector2 position = new Vector2(centre.x, centre.y);

        List<Vector3> verts = new List<Vector3>();
        //Add centre vertex
        verts.Add(Vector3.zero);

        //Add surrounding vertices
        for (int i = 0; i < sides; i++) {
            verts.Add(new Vector3(Mathf.Sin(i * 2 * Mathf.PI / sides), Mathf.Cos(i * 2 * Mathf.PI / sides), 1));
        }
    
        List<int> tris = new List<int>();
        //Add initial (outlier) tri
        tris.Add(0);
        tris.Add(1);
        tris.Add(mesh.vertexCount - 1);

        //Add fanning out tris
        for (int i = 1; i < sides; i++) {
            tris.Add(0);
            tris.Add(i);
            tris.Add(i + 1);
        }

        List<Color> colours = new List<Color>();
        //Set all vertex colours to default
        for (int i = 0; i < mesh.vertexCount; i++) {
            colours.Add(Color.white);
        }

        //Send values to operation method
        RecreateMesh(verts.ToArray(), tris.ToArray(), colours.ToArray());

        //Move new mesh to previous position
        meshTransform.Translate(position);
    }

    void RecreateMesh(Vector3[] verts, int[] tris, Color[] colours) {
        //Clear all data from the mesh
        mesh.Clear();

        //Add vertices to mesh
        mesh.vertices = verts;

        //Add triangles to mesh
        mesh.triangles = tris;

        //Add colours to mesh
        mesh.colors = colours;

        //Recalculate the bounds of the mesh
        mesh.RecalculateBounds();
    }
}
