using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Point : MonoBehaviour
{

    public bool isMoving;
    public bool isSelected;
    public bool lockX;

    public Point pairPoint;

    //Materials
    public Material material;
    public MeshRenderer meshRenderer;
    public Mesh mesh;

    //Offset for Rotation
    private Vector3 offset;

    public Vector3 Centre
    {
        get { return mesh.bounds.center; }
    }

    //Tranform
    IGB283Transform meshTransform = new IGB283Transform();

    //Sliders
    public Slider sliderR;
    public Slider sliderG;
    public Slider sliderB;

    public float colorR = 1.0f;
    public float colorG = 1.0f;
    public float colorB = 1.0f;



    // Use this for initialization
    public void Initialise(Vector2 pos, Material mat)
    {
        tag = "Knob";
        material = mat;
        Draw();
        meshTransform.Translate(pos);

        gameObject.AddComponent<PolygonCollider2D>();

        GetReferences();
    }

    void GetReferences() {
        sliderR = GameObject.FindGameObjectWithTag("SliderRed").GetComponent<Slider>();
        sliderG = GameObject.FindGameObjectWithTag("SliderGreen").GetComponent<Slider>();
        sliderB = GameObject.FindGameObjectWithTag("SliderBlue").GetComponent<Slider>();
    }

    // Update is called once per frame
    void Update()
    {
        MouseClickAction();
        Move();
        SelectClick();

        

        if (isSelected)
        {
            if (sliderR) {
                colorR = sliderR.value;
            } else {
                Debug.LogError("Red colour slider not found");
            }

            if (sliderG) {
                colorG = sliderG.value;
            } else {
                Debug.LogError("Greem colour slider not found");
            }

            if (sliderB) {
                colorB = sliderB.value;
            } else {
                Debug.LogError("Blue colour slider not found");
            }

            UpdateColor(colorR, colorG, colorB);
        }
    }

    public void Draw()
    {

        //Adding MeshFilter and MeshRenderer  to Empty GameObject
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();

        // Get Mesh from the MeshFilter
        mesh = GetComponent<MeshFilter>().mesh;

        //Set the material to selected material
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = material;

        meshTransform.Initialise(mesh);

        Reshape(20);
        meshTransform.Scale(Vector2.one * 0.2f);
    }

    void Move()
    {
        if (!isMoving)
            return;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //transform.position = mousePosition;
        Vector2 direction = mousePosition - new Vector2(Centre.x, Centre.y);
        if (lockX)
        {
            direction.x = 0;
        }

        meshTransform.Translate(direction);
        //GetComponent<PolygonCollider2D>().
    }

    void MouseOverAction()
    {
        Vector2 mousePostion = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Collider2D hitCollider = Physics2D.OverlapPoint(mousePostion);

        if (hitCollider && hitCollider.gameObject == gameObject)
        {
            isMoving = true;
        }
    }

    void MouseClickAction()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseOverAction();
        }
        else if (!Input.GetMouseButton(0))
        {
            if (isMoving) {
                UpdateCollider();
            }
            isMoving = false;
        }
    }

    void UpdateCollider() {
        if (gameObject.GetComponent<PolygonCollider2D>())
            Destroy(gameObject.GetComponent<PolygonCollider2D>());
        gameObject.AddComponent<PolygonCollider2D>();
    }

    public void UpdateColor(float R, float G, float B)
    {
        meshRenderer.material.color = new Color(colorR, colorG, colorB);

    }

    void SelectClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            SelectHover();
        }
    }

    void SelectHover()
    {
        Vector2 mousePostion = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Collider2D hitCollider = Physics2D.OverlapPoint(mousePostion);

        if (hitCollider && hitCollider.gameObject == gameObject)
        {
            isSelected = true;
            SetSliders();
        }
        else
        {
            isSelected = false;
        }
    }

    void SetSliders()
    {
        sliderR.value = colorR;
        sliderG.value = colorG;
        sliderB.value = colorB;
    }

    void Reshape(int sides) {
        //Reject values too low or high
        if (sides < 3 || sides > 50) {
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
        for (int i = 0; i < sides; i++) {
            verts.Add(new Vector3(Mathf.Sin(i * 2 * Mathf.PI / sides), Mathf.Cos(i * 2 * Mathf.PI / sides), 1));
        }

        List<int> tris = new List<int> {
            //Add initial (outlier) tri
            0,
            1,
            verts.Count - 1
        };

        //Add fanning out tris
        for (int i = 1; i < sides; i++) {
            tris.Add(0);
            tris.Add(i);
            tris.Add(i + 1);
        }

        //Send values to operation method
        RecreateMesh(verts.ToArray(), tris.ToArray());

        //Move new mesh to previous position
        meshTransform.Translate(position);
    }

    void RecreateMesh(Vector3[] verts, int[] tris) {
        //Clear all data from the mesh
        mesh.Clear();

        //Add vertices to mesh
        mesh.vertices = verts;

        //Add triangles to mesh
        mesh.triangles = tris;

        List<Color> colours = new List<Color>();
        //Set all vertex colours to default
        for (int i = 0; i < mesh.vertexCount; i++) {
            colours.Add(Color.white);
        }

        //Recalculate the bounds of the mesh
        mesh.RecalculateBounds();
    }
}