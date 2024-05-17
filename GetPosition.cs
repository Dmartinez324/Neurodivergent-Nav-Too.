using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using static JsonHelper;


public class GetPosition : MonoBehaviour
{
    public System.Random random = new System.Random();

    public GameObject eventManager;

    public GameObject quad;

    // public int[] path = {5, 1, 2, 3};
    public List<int> path;
    public GameObject[] quads = {};
    private bool paths = false;

    public GameObject showPathQuestionPrefab;
    private GameObject showPathQuestionInstance = null;

    enum State
    {
        GET_START,
        GET_END,
        SHOW_PATH,
        OFFSET,
        NAV,
        CREATE,
    }

    State state = State.GET_START;
    // State state = State.CREATE;

    // State state = State.SHOW_PATH;
    // State state = State.OFFSET;

    // private string startRoom = "vrlab";
    // private string endRoom = "57-122";
    private string startRoom = "";
    private string endRoom = "";


    private string url = "https://thiswalker.net/asdf/test";
    public WayPoint[] dataArray = null;
    public WayPoint[] waypoints = null;
    private string urlGET = "https://thiswalker.net/asdf/get";

    public GameObject prefab;
    private bool gotData = false;
    private GameObject[] circles = null;


    [System.Serializable]
    public class PositionData
    {
        public string Type = "Meta Quest";
        public float x;
        public float y;
        public float z;
        public float ax;
        public float ay;
        public float az;
    }


    IEnumerator PostRequest(string url, PositionData data)
    {
        string jsonData = JsonUtility.ToJson(data);

        // Create a UnityWebRequest object
        using (UnityWebRequest webRequest = UnityWebRequest.PostWwwForm(url, jsonData))
        {
            //PostRequestHeader();
            // Set the content type
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // Convert the JSON data to bytes
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

            // Set the request body
            webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);

            // Send the request
            yield return webRequest.SendWebRequest();

            // Check for errors
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                // Print the response
                Debug.Log("Response: " + webRequest.downloadHandler.text);
            }
        }
    }







    IEnumerator GETRequest(string url)
    {
        Debug.Log("Get Request Starting....");
        // Create a UnityWebRequest object
        using (UnityWebRequest webRequest = UnityWebRequest.Get(urlGET))
        {
            yield return webRequest.SendWebRequest();
            string jsonString = webRequest.downloadHandler.text;
            dataArray = JsonHelper.FromJson<WayPoint>(jsonString);

            foreach (var data in dataArray)
            {
                Debug.Log($"Room Name: {data.name}, x: {data.x}, y: {data.y}, z: {data.z}, ax: {data.ax}, ay: {data.ay}, az: {data.az}");
            }

            waypoints = new WayPoint[dataArray.Length];
            for (int i = 0; i < waypoints.Length; i++)
            {
                waypoints[i] = new WayPoint();
            }
            gotData = true;
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        if (state != State.CREATE) StartCoroutine(GETRequest(urlGET));
        else gotData = true;
    }

    // Update is called once per frame

    private long ticks;
    void Update()
    {
        ticks++;
        if (ticks % 500 == 0)
        {
            // Debug.Log($"current state is {state}");
        }

        if (state == State.GET_START)
        {
            CustomKeyboard.Open(); // this automatically only happens once

            if (CustomKeyboard.isReady())
            {
                startRoom = CustomKeyboard.Get();
                Debug.Log("Starting room " + startRoom);
                state = State.GET_END;
            }
        }
        else if (state == State.GET_END)
        {
            CustomKeyboard.Open(); // this automatically only happens once

            if (CustomKeyboard.isReady())
            {
                endRoom = CustomKeyboard.Get();
                Debug.Log("Ending room " + endRoom);
                state = State.SHOW_PATH;
            }
        }
        else if (state == State.SHOW_PATH)
        {
            if (showPathQuestionInstance == null)
            {
                Vector3 canvasPos = transform.position + transform.forward * 1f;
                canvasPos.y = 0;
                Quaternion canvasRot = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                showPathQuestionInstance = Instantiate(showPathQuestionPrefab, canvasPos, canvasRot);
            }

            if (OVRInput.GetDown(OVRInput.Button.One))
            {
                paths = true;
                Destroy(showPathQuestionInstance);
                state = State.OFFSET;
                Debug.Log($"paths toggle is {paths}");
            }
            else if (OVRInput.GetDown(OVRInput.Button.Two))
            {
                paths = false;
                Destroy(showPathQuestionInstance);
                state = State.OFFSET;
                Debug.Log($"paths toggle is {paths}");
            }
        }
        else if (state == State.OFFSET && gotData)
        {
            Offset();

            float speed = 30.0f;

            var joystickPrimary = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            var joystickSecondary = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

            var joystick = joystickPrimary + joystickSecondary;

            targetAngle += joystick.x * speed * Time.deltaTime;
            if (joystick.x < -1e-3 || joystick.x > 1e-3)
            {
                Debug.Log("Got joystick command");
                // Offset();
            }
            if (ticks % 50 == 0)
            {
                // Debug.Log($"joy stick x {joystick.x} y {joystick.y}");
            }

            if (OVRInput.GetDown(OVRInput.Button.One))
            {
                state = State.NAV;
                Debug.Log("Going to NAV state");
            }
        }
        else if (state == State.NAV)
        {
            DestroyCircles();

            bool found = false;

            foreach (WayPoint wp in waypoints)
            {
                if (wp.name == endRoom)
                {
                    found = true;
                    Vector3 a = new Vector3(wp.x, 0, wp.z);
                    //Vector3 b = transform.position;
                    Vector3 b = new Vector3(transform.position.x, 0, transform.position.z);

                    // Debug.Log($"Distance: {Vector3.Distance(a, b)}");

                    if (Vector3.Distance(a, b) < 1)
                    {
                        eventManager.GetComponent<ConfettiController>().InstantiateConfetti();
                    }
                }
            }

            if (!found)
            {
                Debug.Log("Can't find the end room");
            }

            // Debug.Log($"quads: {quads}, quads length: {quads.Length}, paths: {paths}");
            if (quads.Length == 0 && paths)
            {
                DrawQuads();
                Debug.Log("Drawing quads");
            }

            if (OVRInput.GetDown(OVRInput.Button.One))
            {
                Debug.Log($"PRE  Toggle quads {paths}");
                paths = !paths;
                Debug.Log($"POST Toggle quads {paths}");
                if (!paths) DestroyQuads();
            }
        }
        else if (state == State.CREATE)
        {
            if (OVRInput.GetDown(OVRInput.Button.One))
            {
               Vector3 angles = transform.eulerAngles;
               Vector3 coordinates = transform.position;
               PositionData posData = new PositionData();
               posData.ax = angles.x;
               posData.ay = angles.y;
               posData.az = angles.z;
               posData.x = coordinates.x;
               posData.y = coordinates.y;
               posData.z = coordinates.z;
               StartCoroutine(PostRequest(url, posData));
            }
        }
    }

    float targetAngle = 0.0f;

    void Offset()
    {
        if (waypoints == null)
        {
            Debug.Log("waypoints is null");
            return;
        }

        WayPoint refRoom = null;
        foreach (var data in dataArray)
        {
            //if (data.name == "57-206")
            if (data.name == startRoom)
            {
                refRoom = data;
            }
        }

        if (refRoom == null)
        {
            Debug.Log("NO REF ROOM");
            return;
        }

        Vector3 angles = transform.eulerAngles;
        Vector3 coordinates = transform.position;

        Vector3 posDiff = coordinates - new Vector3(refRoom.x, 0, refRoom.z);
        posDiff.y = 0;
        //var angleDiff = angles.y - room57_202.ay;
        //var angleDiff = targetAngle - room57_202.ay;
        var angleDiff = targetAngle;

        float ox = refRoom.x;
        float oz = refRoom.z;

       

        // position
        //foreach (var room in dataArray)
        for (int i = 0; i < dataArray.Length; i++)
        {
            dataArray[i].x -= ox;
            dataArray[i].z -= oz;

            var angle = Mathf.Atan2(dataArray[i].z, dataArray[i].x);
            var radius = Mathf.Sqrt(Mathf.Pow(dataArray[i].x, 2) + Mathf.Pow(dataArray[i].z, 2));
            angle += angleDiff * Mathf.Deg2Rad;

            waypoints[i].x = Mathf.Cos(angle) * radius + ox;
            waypoints[i].z = Mathf.Sin(angle) * radius + oz;
            waypoints[i].name = dataArray[i].name;
            waypoints[i].conns = dataArray[i].conns;

            dataArray[i].x += ox;
            dataArray[i].z += oz;
        }

        foreach (var room in waypoints)
        {
            room.x += posDiff.x;
            room.z += posDiff.z;
        }

        DrawCircles();
    }

    public void DrawCircles()
    {
        DestroyCircles();

        circles = new GameObject[dataArray.Length];

        for (int i = 0; i < circles.Length; i++)
        {
            circles[i] = Instantiate(prefab);
            WayPoint room = waypoints[i];
            //WayPoint room = dataArray[i];
            circles[i].transform.position = new Vector3(room.x, room.y, room.z);

        }
    }

    public void DestroyCircles()
    {
        if (circles != null)
        {
            for (int i = 0; i < circles.Length; i++)
            {
                Destroy(circles[i]);
            }

            circles = null;
        }
    }

    public void DrawQuads()
    {
        Graph g = new Graph(waypoints);

        path = g.Dijkstra(startRoom, endRoom);

        DestroyQuads();
        quads = new GameObject[path.Count];

        // // 0, 1, 2, 3, 4
        // // a, b, c, d, e
        // for (int i = 0; i < path.Count; i++)
        // {
        //     int p = path[i];
        //     Debug.Log($"path: {p}");
        // }
        // Debug.Log($"path count: {path.Count}");
        // Debug.Log($"waypoints: {waypoints.Length}");

        for (int i = 0; i < path.Count-1; i++)
        {
            // Debug.Log($"indexes {i} to {i+1}");
            // Debug.Log($"from {path[i]} to {path[i+1]}");

            WayPoint a = waypoints[path[i]];
            WayPoint b = waypoints[path[i+1]];

            // Debug.Log("here 1");

            // Debug.Log($"from ({i})({path[i]}){a.name} to ({i+1})({path[i+1]}){b.name}");

            Vector3 va = new Vector3(a.x, 0, a.z);
            Vector3 vb = new Vector3(b.x, 0, b.z);

            Vector3 diff = Vector3.Normalize(vb-va);

            float s = Vector3.Distance(va, vb);

            GameObject spawned = Instantiate(quad);
            spawned.transform.position = new Vector3((va.x+vb.x)/2, transform.position.y - 2 - ((float) random.NextDouble() * 0.1f), (va.z+vb.z)/2);
            spawned.transform.localScale = new Vector3(s, 1, 1);
            spawned.transform.rotation = Quaternion.Euler(new Vector3(90, 90f + Mathf.Rad2Deg * Mathf.Atan2(diff.x, diff.z), 0));
            quads[i] = spawned;
        }
    }

    public void DestroyQuads()
    {
        if (quads.Length == 0) return;

        foreach (GameObject quad in quads)
        {
            Destroy(quad);
        }

        quads = new GameObject[0];
    }
}
