using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Triangle : MonoBehaviour
{
    //Public Variables

    //UI Elements V2
    // Bool Toggle for isStar
    public Toggle starToggle;
    //# of Sides Slider
    public Slider sideSlider;
    //Rotation Slider
    public Slider rotateSlider;
    //Speed Slider
    public Slider speedSlider;


    public bool reshape;
    public int sides = 3;
    public bool isStar;

    //Locations
    public Vector2 pos1 = new Vector2(-3, 0);
    public Vector2 pos2 = new Vector2(3, 0);

    enum TargetPoints {
        Point1,
        Point2
    }

    TargetPoints currentTarget;

    Point GetTargetPoint {
        get {
            if (currentTarget == TargetPoints.Point1) {
                return point1;
            } else if (currentTarget == TargetPoints.Point2) {
                return point2;
            } else {
                Debug.LogError("Invalid current target");
                return null;
            }
        }
    }

    public Vector2 scale = Vector2.one;

    public Vector2 direction;
    public float speed = 1;

    //Materials
    public Material material;
    public Mesh mesh;

    //Public Rotation Angle Attach to slider
    public float rotateAngle;

    //Music Player
    public AudioSource music;

    //References
    Point point1;
    Point point2;

    MeshRenderer meshRenderer;
    MeshRenderer point1Renderer;
    MeshRenderer point2Renderer;

    public Vector3 Centre
    {
        get { return mesh.bounds.center; }
    }

    //Tranform
    IGB283Transform meshTransform = new IGB283Transform();

    // Use this for initialization
    void Start()
    {
        Draw();
        GetReferences();

        //Duplication Code
        if (!GameObject.FindGameObjectWithTag("ObjectParent")) {
            tag = "ObjectParent";
            new GameObject("Shape Clone").AddComponent<Triangle>();
        } else {
            tag = "Object";
        }
    }

    void GetReferences() {
        point1 = PointAt(pos1);
        point2 = PointAt(pos2);

        point1.pairPoint = point2;
        point2.pairPoint = point1;

        meshRenderer = GetComponent<MeshRenderer>();
        point1Renderer = point1.GetComponent<MeshRenderer>();
        point2Renderer = point2.GetComponent<MeshRenderer>();

        starToggle = GameObject.FindGameObjectWithTag("StarButton").GetComponent<Toggle>();
        speedSlider = GameObject.FindGameObjectWithTag("SpeedSlider").GetComponent<Slider>();
        rotateSlider = GameObject.FindGameObjectWithTag("RotationSlider").GetComponent<Slider>();
        sideSlider = GameObject.FindGameObjectWithTag("SideSlider").GetComponent<Slider>();

        music = GameObject.FindGameObjectWithTag("Music").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        print(name + " has a total of " + mesh.vertexCount + " vertices");
        print("centred at " + Centre);
        print("the first tri is: " + mesh.triangles[0] + ", " + mesh.triangles[1] + ", " + mesh.triangles[2]);
        print(mesh.vertices[0]);
        print(mesh.vertices[1]);
        print(mesh.vertices[3]);

        //UI updates
        //Speed slider update
        if (speedSlider) {
            speed = speedSlider.value;
        } else {
            Debug.LogError("Cannot find speed slider reference");
        }

        //Rotation slider update
        if (rotateSlider) {
            rotateAngle = rotateSlider.value;
        } else {
            Debug.LogError("Cannot find rotation slider reference");
        }

        //Star Toggle update
        if (starToggle) {
            isStar = starToggle.isOn;
        } else {
            Debug.LogError("Cannot find star toggle reference");
        }

        //Side slider update
        if (sideSlider) {
            int sliderVal = (int)sideSlider.value;
            //Only modify shape if it is different
            if (sides != sliderVal) {
                sides = sliderVal;
                if (starToggle.isOn) {
                    ReshapeStar(sides);
                } else {
                    Reshape(sides);
                }
            }
        } else {
            Debug.LogError("Cannot find slider reference");
        }

        //Reached point check
        if (ReachedTarget())
        {
            SwitchTarget();
        }

        direction = GetTargetPoint.Centre - Centre;

        
        //Transform updates
        meshTransform.Translate(direction.normalized * speed * Time.deltaTime);
        meshTransform.Scale(scale);
        meshTransform.RotateCenter(rotateAngle);

        //Color Changing with Lerp
        float totalDist = Vector3.Distance(point1.Centre, point2.Centre);
        float remDist = Vector3.Distance(Centre, point2.Centre);

        float t = (totalDist - remDist) / totalDist;

        meshRenderer.material.color = Color.Lerp(point1Renderer.material.color, point2Renderer.material.color, t);

        //Tie music volume to if the object is moving
        music.volume = speed;
    }

    // Draw a rectangle
    public void Draw()
    {
        //Adding MeshFilter and MeshRenderer  to Empty GameObject
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();

        // Get Mesh from the MeshFilter
        mesh = GetComponent<MeshFilter>().mesh;

        meshTransform.Initialise(mesh);
        meshTransform.Translate(pos1);

        //Set the material to selected material 
        material = GameObject.FindGameObjectWithTag("DefaultMaterial").GetComponent<MeshRenderer>().material;
        GetComponent<MeshRenderer>().material = material;
        
        //Make new mesh for object
        Reshape(sides);
    }

    Point PointAt(Vector2 pos)
    {
        GameObject point = new GameObject("Point");
        Point script = point.AddComponent<Point>();
        script.Initialise(pos, material);
        return script;
    }


    void SwitchTarget()
    {
        //Initialise/Switch between targets
        if (currentTarget == TargetPoints.Point1)
        {
            currentTarget = TargetPoints.Point2;
        }
        else
        {
            currentTarget = TargetPoints.Point1;
        }
    }

    bool ReachedTarget()
    {
        //Confirm whether the target is in touching distance of the frame
        return Vector3.Distance(Centre, GetTargetPoint.Centre) < speed * Time.deltaTime;
    }

    void Reshape(int sides)
    {
        //Reject values too low or high
        if (sides < 3 || sides > 50)
        {
            Debug.LogErrorFormat("{0} is an invalid number of sides for the shape to have.", sides);
            return;
        }

        //Get the current position to move the new mesh to
        Vector2 position = Centre;

        List<Vector3> verts = new List<Vector3> {
            //Add centre vertex
            Vector3.zero
        };

        //Add surrounding vertices
        for (int i = 0; i < sides; i++)
        {
            verts.Add(new Vector3(Mathf.Sin(i * 2 * Mathf.PI / sides), Mathf.Cos(i * 2 * Mathf.PI / sides), 1));
        }

        List<int> tris = new List<int> {
            //Add initial (outlier) tri
            0,
            1,
            verts.Count - 1
        };

        //Add fanning out tris
        for (int i = 1; i < sides; i++)
        {
            tris.Add(0);
            tris.Add(i);
            tris.Add(i + 1);
        }

        //Send values to operation method
        RecreateMesh(verts.ToArray(), tris.ToArray());

        //Move new mesh to previous position
        meshTransform.Translate(position);
    }

    void RecreateMesh(Vector3[] verts, int[] tris)
    {
        print("reshaping " + name);
        print(sides + " sided shape");
        print("total vertices: " + verts.Length);

        //Clear all data from the mesh
        mesh.Clear();

        //Add vertices to mesh
        mesh.vertices = verts;

        //Add triangles to mesh
        mesh.triangles = tris;

        List<Color> colours = new List<Color>();
        //Set all vertex colours to default
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            colours.Add(Color.white);
        }

        //Recalculate the bounds of the mesh
        mesh.RecalculateBounds();
    }

    void ReshapeStar(int points)
    {
        //Reject values too low or high
        if (points < 3 || points > 50)
        {
            Debug.LogErrorFormat("{0} is an invalid number of sides for the shape to have.", points);
            return;
        }

        Reshape(points);

        Vector2 offset = mesh.vertices[0];
        meshTransform.Translate(-offset);
        meshTransform.ScaleUnrestricted(Vector2.one * 0.5f);

        int newStart = mesh.vertexCount;

        List<Vector3> verts = new List<Vector3>(mesh.vertices);
        //Add point vertices
        for (int i = 0; i < points; i++)
        {
            verts.Add(new Vector3(Mathf.Sin((i + 0.5f) * 2 * Mathf.PI / points), Mathf.Cos((i + 0.5f) * 2 * Mathf.PI / points), 1));
        }

        List<int> tris = new List<int>(mesh.triangles) {
            //Add initial (outlier) tri
            verts.Count - 1,
            1,
            newStart - 1
        };

        //Add fanning out tris
        for (int i = 0; i < points - 1; i++)
        {
            tris.Add(newStart + i);
            tris.Add(i + 1);
            tris.Add(i + 2);
        }

        RecreateMesh(verts.ToArray(), tris.ToArray());

        meshTransform.Translate(offset);
    }

    public void ReshapePress()
    {
        reshape = true;
    }
}