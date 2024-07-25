using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Mesh를 그리기위한 컴포넌트
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshCreate : MonoBehaviour
{
    #region 1. 할당된 정보를 바탕으로 메쉬를 그리기위해, 유니티에서 제공하는 컴포넌트
    /*
     * 메쉬에 대한 정보를 담고있는 객체
     * 1. 메쉬를 구성하는 정점의 정보(위치값)
     * 2. 각 정점의 UV좌표 데이터
     * 3. 각 정점이 가지는 법선 데이터
     * 4. 메쉬를 구성하는 삼각형의 구성정점 넘버
     */
    public Mesh mesh;
    // 원본 메쉬정보 갖기 위함
    public Mesh origine_mesh;

    /*
     * mesh filter의 Mesh로 접근하기 위해 사용하는 클래스.
     * 절차적 메쉬 인터페이스에서 사용
     */
    public MeshFilter meshFilter;

    // 박스 콜라이더
    public Collider col;

    // 메쉬에 할당된 정보를 바탕으로 메쉬를 랜더링 하는 역할.
    public MeshRenderer meshRenderer;
    #endregion

    //public bool isGizmo;

    // 정점 집합
    public Vector3[] vertices;

    // 메쉬 사이즈 설정
    [Range(0, 100f)]
    public float size = 1f;
    float hsize = 10f;

    // 정점을 보이도록 하기위함...
    GameObject[] pos;
    // 텍스쳐도 디버깅용으로 설정 가능
    public Texture m_texture;

    // Start is called before the first frame update
    void Awake()
    {
        // 각 컴포넌트 할당
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        col = GetComponent<BoxCollider>(); // 여기선 안씀...

        // 머티리얼도 받아오고, 머티리얼에 디폴트 텍스쳐도 셋팅하고~
        Material material = new Material(Shader.Find("Standard"));
        material.SetTexture("_MainTex", m_texture);
        meshRenderer.material = material;

        // 만약, 메쉬가 연결되어 있다면 받아올 것이다...
        origine_mesh = meshFilter.sharedMesh;

        // 메쉬가 기본 셋팅일때와 아닐때 구별~
        if(origine_mesh)
        {
            pos = new GameObject[origine_mesh.vertexCount]; // 버텍스 수만큼 할당
        }
        else
            pos = new GameObject[4]; // 기본 쿼드 형태로
    }

    // Start is called before the first frame update
    void Start()
    {
        // 베이스 메시가 있을 경우
        if (origine_mesh)
        {
            //Debug.Log(origine_mesh.vertexCount);
            // 메쉬 카운터만큼 게임오브젝트를 동적 생성해서 정점을 만들어 준다.
            for (int i = 0; i < origine_mesh.vertexCount; i++)
            {
                pos[i] = new GameObject("Pos"+i);
                pos[i].transform.parent = this.transform;
                pos[i].transform.localPosition = origine_mesh.vertices[i];
                pos[i].AddComponent<CreateSphereGizmo>(); // 기즈모로 클릭 가능하게...
            }
        }
        else
        {
            // 전체 비율 설정
            hsize = 0.5f * size;

            // 현재 오브젝트 자식으로 포인트용 오브젝트 생성
            for (int i = 0; i < 4; i++)
            {
                pos[i] = new GameObject("Pos");
                pos[i].transform.parent = this.transform;
                if (i == 0)
                {
                    pos[i].transform.localPosition = new Vector3(-hsize, hsize, 0f); // 정점은 설정 비율만큼~
                }
                else if (i == 1)
                {
                    pos[i].transform.localPosition = new Vector3(hsize, hsize, 0f);
                }
                else if (i == 2)
                {
                    pos[i].transform.localPosition = new Vector3(hsize, -hsize, 0f);
                }
                else if (i == 3)
                {
                    pos[i].transform.localPosition = new Vector3(-hsize, -hsize, 0f);
                }
                // 각 정점 클릭 가능하도록....
                pos[i].AddComponent<CreateSphereGizmo>();
            }
        }

        //// Quad의 정점 데이터 할당하기
        //vertices = new Vector3[]
        //{
        //    new Vector3(-hsize, hsize, 0f),
        //    new Vector3(hsize, hsize, 0f),
        //    new Vector3(hsize, -hsize, 0f),
        //    new Vector3(-hsize, -hsize, 0f)
        //};
    }

    // Update is called once per frame
    void Update()
    {
        // 동적 메쉬 생성
        mesh = new Mesh();
        //hsize = 0.5f * size;

        // 만약, 기본 메쉬가 있을경우 버텍스 정보만큼 for 문을 돌면서 현재 변경된 정점을 셋팅
        if (origine_mesh)
        {
            vertices = new Vector3[origine_mesh.vertexCount];

            for (int i = 0; i < origine_mesh.vertexCount; i++)
            {
                vertices[i] = pos[i].transform.localPosition;
            }
        }
        else // 쿼드 형태일 경우...
        {
            // Quad의 정점 데이터 할당하기
            vertices = new Vector3[]
            {
                pos[0].transform.localPosition,
                pos[1].transform.localPosition,
                pos[2].transform.localPosition,
                pos[3].transform.localPosition
            };
        }

        // 설정 정보를 바탕으로, 유니티에서 제공하는 메쉬객체에 그릴 정보를 할당
        mesh.vertices = vertices;
        // 베이스 메쉬가 있을경우...(원본거 빼껴다가~)
        if (origine_mesh)
        {
            mesh.uv = origine_mesh.uv;
            mesh.normals = origine_mesh.normals;
            mesh.triangles = origine_mesh.triangles;
        }
        // 쿼드 형태일 경우..각각 셋팅
        else
        {
            // 사각형의 UV좌표 데이터
            Vector2[] uv = new Vector2[]
            {
                //new Vector2(0, 0),
                //new Vector2(1, 0),
                //new Vector2(1, 1),
                //new Vector2(0, 1)
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(1, 0),
                new Vector2(0, 0)
            };

            // 각 정점에 할당될 법선데이터
            Vector3[] normals = new Vector3[]
            {
                new Vector3(0f, 0f, -1f),//0번 정점의 법선
                new Vector3(0f, 0f, -1f),//1번 정점의 법선
                new Vector3(0f, 0f, -1f),//2번 정점의 법선
                new Vector3(0f, 0f, -1f),//3번 정점의 법선
            };

            // 위에서 할당한 정보를 바탕으로 삼각형을 구성할 정점 인덱스의 순서를 정한다.
            int[] triangles = new int[]
            {
                0, 1, 2, 
                2, 3, 0
            };
            mesh.uv = uv;
            mesh.normals = normals;
            mesh.triangles = triangles;
        }

        // 정점으로부터 메쉬의 경계면 크기를 다시 계산
        mesh.RecalculateBounds();
        // 삼각면과 정점으로부터 메쉬의 노멀을 다시 계산
        mesh.RecalculateNormals();

        // 그릴 메쉬에대한 정보를 메쉬 필터에 할당
        meshFilter.mesh = mesh;

        //OnDrawGizmos();
    }

    //private void OnDrawGizmos()
    //{
    //    if (!isGizmo) return;

    //    Gizmos.color = Color.blue;

    //    foreach(var pos in vertices)
    //    {
    //        Gizmos.DrawSphere(pos, 0.5f);
    //    }
    //}

    // PropertyDrawer

    //인스펙터에 스크립트 우 클릭시 컨텍스트 메뉴에서 함수호출 가능
    // 정점 초기화
    [ContextMenu("Point Reset")]
    void FuncStart()
    {
        // 베이스 메쉬가 있을경우...
        if (origine_mesh)
        {
            for (int i = 0; i < origine_mesh.vertexCount; i++)
            {

                pos[i].transform.localPosition = origine_mesh.vertices[i];
            }
        }
        else
        {
            hsize = 0.5f * size;

            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                {
                    pos[i].transform.localPosition = new Vector3(-hsize, hsize, 0f);
                }
                else if (i == 1)
                {
                    pos[i].transform.localPosition = new Vector3(hsize, hsize, 0f);
                }
                else if (i == 2)
                {
                    pos[i].transform.localPosition = new Vector3(hsize, -hsize, 0f);
                }
                else if (i == 3)
                {
                    pos[i].transform.localPosition = new Vector3(-hsize, -hsize, 0f);
                }
            }
        }

        Debug.Log("Point Reset");
    }
}
