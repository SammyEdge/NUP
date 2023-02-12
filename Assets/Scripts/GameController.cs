using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
    private CubePosition nowCube = new CubePosition(0, 1, 0);
    public float cubeChangePlaceSpeed = 0.5f;
    private float camMoveToYPosition, camMoveSpeed = 2f;
    private List<Vector3> allCubesPositions = new List<Vector3>{
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(-1, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(0, 0, 1),
        new Vector3(0, 0, -1),
        new Vector3(1, 0, 1),
        new Vector3(1, 0, -1),
        new Vector3(-1, 0, 1),
        new Vector3(-1, 0, -1)
    };

    public Color[] bgColors;
    private Color toCameraColor;
    public Transform cubeToPlace;
    public GameObject cubeToCreate, allCubes, vfx;
    public GameObject[] canvasStartPage;
    private bool isLose, firstCube;    
    private Rigidbody allCubesRB;
    private Coroutine showCubePlace;
    private Transform mainCam;
    private int prevCountMaxHorizontal = 0;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        camMoveToYPosition = 6f + nowCube.y - 1f;
        mainCam = Camera.main.transform;
        toCameraColor = Camera.main.backgroundColor;

        showCubePlace = StartCoroutine(ShowCubePlace());
        allCubesRB = allCubes.GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if ((Input.GetMouseButtonDown(0) || Input.touchCount > 0) && cubeToPlace != null && allCubes != null && !EventSystem.current.IsPointerOverGameObject())
        {
            #if !UNITY_EDITOR
            if (Input.GetTouch(0).phase != TouchPhase.Began)
            {
                return;
            }
            #endif

            if (!firstCube)
            {
                firstCube = true;
                // скрыть логотипы и кнопки
                foreach (GameObject obj in canvasStartPage)
                {
                    Destroy(obj);
                }
            }

            GameObject newCube = Instantiate(cubeToCreate, cubeToPlace.position, Quaternion.identity);

            newCube.transform.SetParent(allCubes.transform);
            
            nowCube.SetVector(newCube.transform.position);
            allCubesPositions.Add(nowCube.GetVector());

            GameObject vfxGO = Instantiate(vfx, cubeToPlace.position, Quaternion.identity) as GameObject;
            Destroy(vfxGO, 1.5f);

            if (PlayerPrefs.GetString("music") != "No")
            {
                GetComponent<AudioSource>().Play();
            }

            allCubesRB.isKinematic = true;
            allCubesRB.isKinematic = false;

            SpawnPositions();
            MoveCameraChangeBG();
        }

        // Check if stable
        if (!isLose && allCubesRB.velocity.magnitude > 0.1f )
        {
            Destroy(cubeToPlace.gameObject);
            isLose = true;
            StopCoroutine(showCubePlace);
        }

        mainCam.localPosition = Vector3.MoveTowards(mainCam.localPosition, 
            new Vector3(mainCam.localPosition.x, camMoveToYPosition, mainCam.localPosition.z), 
            camMoveSpeed * Time.deltaTime);

        if (Camera.main.backgroundColor != toCameraColor)
        {
            Camera.main.backgroundColor = Color.Lerp(Camera.main.backgroundColor, toCameraColor, Time.deltaTime / 1.5f);
        }
    }

    IEnumerator ShowCubePlace()
    {
        while (true)
        {
            SpawnPositions();

            yield return new WaitForSeconds(cubeChangePlaceSpeed);
        }
    }

    private void SpawnPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        
        Vector3 checkedPosition = new Vector3(nowCube.x + 1, nowCube.y, nowCube.z);

        // x+1 check
        if (IsPositionEmpty(checkedPosition) && checkedPosition.x != cubeToPlace.position.x)
        {
            positions.Add(checkedPosition);
        }

        // x-1 check
        checkedPosition = new Vector3(nowCube.x - 1, nowCube.y, nowCube.z);
        if (IsPositionEmpty(checkedPosition) && checkedPosition.x != cubeToPlace.position.x)
        {
            positions.Add(checkedPosition);
        }
        
        // y+1 check
        checkedPosition = new Vector3(nowCube.x, nowCube.y + 1, nowCube.z);
        if (IsPositionEmpty(checkedPosition) && checkedPosition.y != cubeToPlace.position.y)
        {
            positions.Add(checkedPosition);
        }

        // y-1 check
        checkedPosition = new Vector3(nowCube.x, nowCube.y - 1, nowCube.z);
        if (IsPositionEmpty(checkedPosition) && checkedPosition.y != cubeToPlace.position.y)
        {
            positions.Add(checkedPosition);
        }

        // z+1 check
        checkedPosition = new Vector3(nowCube.x, nowCube.y, nowCube.z + 1);
        if (IsPositionEmpty(checkedPosition) && checkedPosition.z != cubeToPlace.position.z)
        {
            positions.Add(checkedPosition);
        }

        // z-1 check
        checkedPosition = new Vector3(nowCube.x, nowCube.y, nowCube.z - 1);
        if (IsPositionEmpty(checkedPosition) && checkedPosition.z != cubeToPlace.position.z)
        {
            positions.Add(checkedPosition);
        }
        if (positions.Count > 1)
        {
            cubeToPlace.position = positions[UnityEngine.Random.Range(0, positions.Count)];
        }
        else if (positions.Count == 0)
        {
            isLose = true;
        }
        else
        {
            cubeToPlace.position = positions[0];
        }
        //Debug.Log();
    }

    private bool IsPositionEmpty(Vector3 targetPos)
    {
        if (targetPos.y == 0)
        {
            return false;
        }

        if (allCubesPositions.Exists(elem => elem.x == targetPos.x && elem.y == targetPos.y && elem.z == targetPos.z))
        {
            return false;
        }

        return true;
    }

    private void MoveCameraChangeBG()
    {
        int maxX = 0, maxY = 0, maxZ = 0, maxHorz;

        foreach (Vector3 vec3 in allCubesPositions)
        {
            if (Mathf.Abs(Convert.ToInt32(vec3.x)) > maxX)
                maxX = Convert.ToInt32(vec3.x);
            if (Convert.ToInt32(vec3.y) > maxY)
                maxY = Convert.ToInt32(vec3.y);
            if (Mathf.Abs(Convert.ToInt32(vec3.z)) > maxZ)
                maxZ = Convert.ToInt32(vec3.z);
        }
        camMoveToYPosition = 6f + nowCube.y - 1f;

        maxHorz = maxX > maxZ ? maxX : maxZ;

        if (maxHorz % 3 == 0 && prevCountMaxHorizontal != maxHorz)
        {
            mainCam.localPosition += new Vector3(0, 0, -2.5f);
            prevCountMaxHorizontal = maxHorz;
        }

        // change BG
        if (maxY >= 7)
            toCameraColor = bgColors[2];
        else if (maxY >= 5)
            toCameraColor = bgColors[1];
        else if (maxY >= 2)
            toCameraColor = bgColors[0];
    }
}

struct CubePosition
{
    public int x, y, z;

    public CubePosition(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vector3 GetVector()
    {
        return new Vector3(x, y, z);
    }

    public void SetVector(Vector3 vector)
    {
        x = Convert.ToInt32(vector.x);
        y = Convert.ToInt32(vector.y);
        z = Convert.ToInt32(vector.z);
    }
}