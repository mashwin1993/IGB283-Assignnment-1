using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Triangle : MonoBehaviour
{
    //Public Variables

    public bool reshape;
    public int sides = 3;
    public bool isStar;
    public bool isSelected;

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

    //Transform Values
    public Vector2 direction;
    public float speed = 1;

    public float rotateAngle;

    public Vector2 scale = Vector2.one;


    //Materials
    public Material material;
    public Mesh mesh;

    //References
    Point point1;
    Point point2;

    MeshRenderer meshRenderer;
    MeshRenderer point1Renderer;
    MeshRenderer point2Renderer;

    ParticleSystem particles;
    ParticleSystemRenderer particleRenderer;
    ParticleSystem.ShapeModule shape;

    //UI Elements V2
    // Bool Toggle for isStar
    public Toggle starToggle;
    //# of Sides Slider
    public Slider sideSlider;
    //Rotation Slider
    public Slider rotateSlider;
    //Speed Slider
    public Slider speedSlider;
    //X Scale Slider
    public Slider xScaleSlider;
    //Y Scale Slider
    public Slider yScaleSlider;

    //Music Player
    public AudioSource music;

    public Vector3 Centre
    {
        get { return mesh.bounds.center; }
    }

    public Vector3 Origin {
        get { if (mesh.vertices.Length > 0) {
                return mesh.vertices[0];
            } else {
                return Vector3.zero;
            }
        }
    }

    //Tranform
    IGB283Transform meshTransform = new IGB283Transform();

    // Use this for initialization
    void Start()
    {
        Draw();

        GetReferences();

        //Duplication Code
        if (!GameObject.Find("ObjectParent")) {
            name = "ObjectParent";
            new GameObject("Object Clone").AddComponent<Triangle>();
            meshTransform.Translate(Vector2.up * 2);
            Reshape(15);
            rotateAngle = 5;
        } else {
            name = "Object Clone";
            isStar = true;
            ReshapeStar(15);
            rotateAngle = -10;
            speed = 3;
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

        GameObject tempObject;

        tempObject = GameObject.Find("StarMode");
        if (tempObject) {
            starToggle = tempObject.GetComponent<Toggle>();
        } else {
            Debug.LogError("Star toggle not found");
        }

        tempObject = GameObject.Find("SpeedSlider");
        if (tempObject) {
            speedSlider = tempObject.GetComponent<Slider>();
        } else {
            Debug.LogError("Speed slider not found");
        }

        tempObject = GameObject.Find("RotateSlider");
        if (tempObject) {
            rotateSlider = tempObject.GetComponent<Slider>();
        } else {
            Debug.LogError("Rotate slider not found");
        }

        tempObject = GameObject.Find("SideSlider");
        if (tempObject) {
            sideSlider = tempObject.GetComponent<Slider>();
        } else {
            Debug.LogError("Side slider not found");
        }

        tempObject = GameObject.Find("XScaleSlider");
        if (tempObject) {
            xScaleSlider = tempObject.GetComponent<Slider>();
        } else {
            Debug.LogError("X Scale slider not found");
        }

        tempObject = GameObject.Find("YScaleSlider");
        if (tempObject) {
            yScaleSlider = tempObject.GetComponent<Slider>();
        } else {
            Debug.LogError("Y Scale slider not found");
        }
        
        tempObject = GameObject.Find("Music");
        if (tempObject) {
            music = tempObject.GetComponent<AudioSource>();
        } else {
            Debug.LogError("Music instance not found");
        }
    }

    // Update is called once per frame
    void Update()
    {
        SelectClick();
        Highlight();

        GetUIUpdates();

        //Reached point check
        if (ReachedTarget())
        {
            SwitchTarget();
        }

        direction = GetTargetPoint.Centre - Origin;

        UpdateTransform();

        //Color Changing with Lerp
        float totalDist = Vector3.Distance(point1.Centre, point2.Centre);
        float remDist = Vector3.Distance(Origin, point2.Centre);

        float t = (totalDist - remDist) / totalDist;

        meshRenderer.material.color = Color.Lerp(point1Renderer.material.color, point2Renderer.material.color, t);

        //Tie music volume to if the object is moving
        if (music) {
            music.volume = 0;
        } else {
            Debug.LogError("Music instance not found");
        }

        UpdateCollider();
    }

    void Highlight() {
        if (isSelected) {
            shape.position = Origin;
            if (!particles.isPlaying) {
                particles.Play();
            }
        } else {
            particles.Stop();
        }
    }

    void GetUIUpdates() {
        if (!isSelected) {
            return;
        }

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

        //X Scale slider update
        if (xScaleSlider) {
            scale.x = xScaleSlider.value;
        } else {
            Debug.LogError("Cannot find x scale slider reference");
        }

        //Y Scale slider update
        if (yScaleSlider) {
            scale.y = yScaleSlider.value;
        } else {
            Debug.LogError("Cannot find y scale slider reference");
        }
    }

    void UpdateTransform() {
        //Transform updates
        meshTransform.Translate(direction.normalized * speed * Time.deltaTime);
        meshTransform.Scale(scale);
        meshTransform.RotateOrigin(rotateAngle);
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
        GameObject defaultMat = GameObject.Find("DefaultMaterial");
        if (defaultMat) {
            material = defaultMat.GetComponent<MeshRenderer>().material;
        } else {
            Debug.LogError("Default sprite material not found");
            material = new Material(Shader.Find("Sprites/Default"));
        }

        GetComponent<MeshRenderer>().material = material;
        
        //Make new mesh for object
        Reshape(sides);
        gameObject.AddComponent<BoxCollider2D>();

        ParticleSetup();
    }

    void ParticleSetup() {
        particles = gameObject.AddComponent<ParticleSystem>();

        particleRenderer = gameObject.GetComponent<ParticleSystemRenderer>();

        GameObject defaultParticle = GameObject.Find("DefaultParticle");
        if (defaultParticle) {
            particleRenderer.material = defaultParticle.GetComponent<MeshRenderer>().material;
        } else {
            Debug.LogError("Default particle sprite not found");
            particleRenderer.material = new Material(Shader.Find("Particles/Additive"));
        }

        ParticleSystem.EmissionModule emmission = particles.emission;
        emmission.rateOverTimeMultiplier = 5f;
        shape = particles.shape;
        shape.scale = Vector3.one * 0.2f;
        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = 1;

        shape.position = Origin;
    }

    Point PointAt(Vector2 pos)
    {
        GameObject point = new GameObject("Point");
        Point script = point.AddComponent<Point>();
        script.Initialise(pos, material);
        script.owner = this;
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
        return Vector3.Distance(Origin, GetTargetPoint.Centre) < speed * Time.deltaTime;
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
        Vector2 position = Origin;

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

        meshTransform.ResetScale();

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

    public void MoveToY(float y) {
        Vector2 movement = new Vector2(0, y - Origin.y);

        meshTransform.Translate(movement);
    }

    void SelectClick() {
        if (Input.GetMouseButtonDown(1)) {
            SelectHover();
        }
    }

    void SelectHover() {
        Vector2 mousePostion = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Collider2D hitCollider = Physics2D.OverlapPoint(mousePostion);

        if (hitCollider && hitCollider.gameObject == gameObject) {
            isSelected = true;
            SetSliders();
        } else {
            isSelected = false;
        }
    }

    void SetSliders() {
        if (xScaleSlider)
            xScaleSlider.value = scale.x;
        if (yScaleSlider)
            yScaleSlider.value = scale.y;

        if (speedSlider)
            speedSlider.value = speed;
        if (rotateSlider)
            rotateSlider.value = rotateAngle;

        if (sideSlider)
            sideSlider.value = sides;
        if (starToggle)
            starToggle.isOn = isStar;
    }

    void UpdateCollider() {
        if (gameObject.GetComponent<BoxCollider2D>())
            Destroy(gameObject.GetComponent<BoxCollider2D>());
        gameObject.AddComponent<BoxCollider2D>();
    }

    private void FixedUpdate() {
        //UpdateCollider();
    }

    private void LateUpdate() {
        if (music) {
            music.volume += speed / 20f;
        } else {
            Debug.LogError("Music instance not found");
        }
    }
}