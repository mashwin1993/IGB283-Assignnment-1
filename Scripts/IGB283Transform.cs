using UnityEngine;
using System.Collections;

public class IGB283Transform : MonoBehaviour
{

    Mesh mesh;

    Vector2 scale = new Vector2(1, 1);
    Vector2 signedScale = new Vector2(1, 1);

    //Mesh displacement for exact origin
    Vector2 displacement = new Vector2();

    //Localises the object's mesh
    public void Initialise(Mesh objectMesh)
    {
        mesh = objectMesh;
    }

    //Move mesh in a target direction/displacement
    public void Translate(Vector2 direction)
    {
        ApplyTransform(TranslationMatrix(direction));
        displacement += direction;
    }

    //Rotate the mesh, default to origin rotate
    public void Rotate(float angle)
    {
        RotateOrigin(angle);
    }

    //Rotate the mesh based on the starting origin
    public void RotateOrigin(float angle)
    {
        RotatePoint(displacement, angle);
    }

    //Rotate the mesh around the gameobject
    public void RotateGlobal(float angle)
    {
        ApplyTransform(RotationMatrix(angle));
    }

    //Rotate the mesh around the bounds centre
    public void RotateCenter(float angle)
    {
        RotatePoint(new Vector2(mesh.bounds.center.x, mesh.bounds.center.y), angle);
    }

    //Rotate the mesh around a point
    public void RotatePoint(Vector2 rotatePoint, float angle)
    {
        Matrix3x3 fullTransform = TranslationMatrix(rotatePoint) *
            RotationMatrix(angle) *
            TranslationMatrix(-rotatePoint);

        ApplyTransform(fullTransform);
    }

    //Scales the mesh to the target ratio
    public void Scale(Vector2 toScale)
    {
        Vector2 toScalePositive = toScale;
        if (toScalePositive.x >= 0)
        {
            toScalePositive.x = 1;
        }
        else
        {
            toScalePositive.x = -1;
            toScale.x *= -1;
        }

        if (toScalePositive.y >= 0)
        {
            toScalePositive.y = 1;
        }
        else
        {
            toScalePositive.y = -1;
            toScale.y *= -1;
        }

        toScale.x = Mathf.Clamp(toScale.x, 0.000001f, float.MaxValue);
        toScale.y = Mathf.Clamp(toScale.y, 0.000001f, float.MaxValue);

        Vector2 offset = mesh.bounds.center;
        Translate(-offset);
        ApplyTransform(ScaleMatrix(new Vector2((toScale.x * toScalePositive.x) / (signedScale.x * scale.x), (toScale.y * toScalePositive.y) / (signedScale.y * scale.y))));
        Translate(offset);

        signedScale = toScalePositive;
        scale = toScale;
    }

    public void ScaleUnrestricted(Vector2 scale) {
        //Special case, assumes vertex 0 is origin
        Vector2 offset = mesh.vertices[0];
        Translate(-offset);
        ApplyTransform(ScaleMatrix(scale));
        Translate(offset);

    }

    //Applies a transform to the mesh and does subsequent functions
    public void ApplyTransform(Matrix3x3 transform)
    {
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            vertices[i] = transform.MultiplyPoint(vertices[i]);
        }

        mesh.vertices = vertices;

        mesh.RecalculateBounds();
    }

    //Creates a matrix to translate the mesh
    Matrix3x3 TranslationMatrix(Vector2 direction)
    {
        Matrix3x3 translateMatrix = new Matrix3x3();

        translateMatrix.SetRow(0, new Vector3(1, 0, direction.x));
        translateMatrix.SetRow(1, new Vector3(0, 1, direction.y));
        translateMatrix.SetRow(2, new Vector3(0, 0, 1));

        return translateMatrix;
    }

    //Creates a matrix to rotate the mesh
    Matrix3x3 RotationMatrix(float angle)
    {
        Matrix3x3 angleMatrix = new Matrix3x3();

        angleMatrix.SetRow(0, new Vector3(Mathf.Cos(angle * Mathf.PI / 180), -Mathf.Sin(angle * Mathf.PI / 180), 0));
        angleMatrix.SetRow(1, new Vector3(Mathf.Sin(angle * Mathf.PI / 180), Mathf.Cos(angle * Mathf.PI / 180), 0));
        angleMatrix.SetRow(2, new Vector3(0, 0, 1));

        return angleMatrix;
    }

    //Creates a matrix to scale the mesh
    Matrix3x3 ScaleMatrix(Vector2 scale)
    {
        Matrix3x3 scaleMatrix = new Matrix3x3();

        scaleMatrix.SetRow(0, new Vector3(scale.x, 0, 0));
        scaleMatrix.SetRow(1, new Vector3(0, scale.y, 0));
        scaleMatrix.SetRow(2, new Vector3(0, 0, 1));

        return scaleMatrix;
    }
}
