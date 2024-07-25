using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SelectMeshSave : Editor
{
    [MenuItem("Gameobject/SelectMeshSave")]
    static void Copy()
    {
        Transform t = Selection.activeTransform;
        GameObject obj = t ? t.gameObject : null;
        if(obj)
        {
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter ? meshFilter.sharedMesh : null;
            Mesh newMesh = new Mesh();
            newMesh.vertices = mesh.vertices;
            newMesh.uv = mesh.uv;
            newMesh.normals = mesh.normals;
            newMesh.triangles = mesh.triangles;
            newMesh.RecalculateNormals();
            newMesh.RecalculateBounds();
#if true    
            Vector3 diff = Vector3.Scale(newMesh.bounds.extents, new Vector3(0,0,0));
            obj.transform.position -= Vector3.Scale(diff, obj.transform.localScale);
            Vector3[] verts = newMesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] += diff;
            }
            newMesh.vertices = verts;
            newMesh.RecalculateBounds();
#endif
            string fileName = EditorUtility.SaveFilePanelInProject("Save Mesh", "mesh", "asset", "");
            AssetDatabase.CreateAsset(newMesh, fileName);
        }


    }











}
