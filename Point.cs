using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Point : MonoBehaviour {

    public bool isMoving;

    //Draw size
    public float size = 0.5f;


    //Materials
    public Material material;
    public Mesh mesh;

    //Offset for Rotation
    private Vector3 offset;

    public Vector3 centre {
        get { return mesh.bounds.center; }
    }

    //Sliders
    public Slider SliderR;
    public Slider SliderG;
    public Slider SliderB;
    // RGB Colors
    public float ColorR = 1.0f;
    public float ColorG = 1.0f;
    public float ColorB = 1.0f;

    //Tranform
    IGB283Transform meshTransform = new IGB283Transform();

    // Use this for initialization
    public void Initialise(Vector2 pos, Material mat)
    {
        tag = "Knob";
        material = mat;
        Draw();
        meshTransform.Initialise(mesh);
        meshTransform.Translate(pos);

        gameObject.AddComponent<PolygonCollider2D>();
    }


    // Update is called once per frame
    void Update()
    {
        MouseClickAction();
        Move();
        //Color Change
        ColorR = SliderR.value;
        ColorG = SliderG.value;
        ColorB = SliderB.value;

        UpdateColor(ColorR, ColorG, ColorB);
    }

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

    public void UpdateColor(float R, float G, float B)
    {
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(ColorR, ColorG, ColorB);

    }

    void Move() {
        if (!isMoving)
            return;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //transform.position = mousePosition;

        meshTransform.Translate(mousePosition - new Vector2(centre.x, centre.y));
        //GetComponent<PolygonCollider2D>().
    }

    void MouseOverAction() {
        Vector2 mousePostion = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Collider2D hitCollider = Physics2D.OverlapPoint(mousePostion);

        if (hitCollider && hitCollider.gameObject == gameObject) {
            isMoving = true;
        }
    }

    void MouseClickAction() {
        if (Input.GetMouseButtonDown(0)) {
            MouseOverAction();
        } else if (!Input.GetMouseButton(0)) {
            if (isMoving) {
                Destroy(gameObject.GetComponent<PolygonCollider2D>());
                gameObject.AddComponent<PolygonCollider2D>();
            }
            isMoving = false;
        }
    }

}


