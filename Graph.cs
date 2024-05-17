using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using UnityEngine;

public class Graph
{
    public static List<WayPoint> _myJson;
    public static float[,] _edges;
    public static WayPoint[] _vertices;
    public static int _n;


    public Graph(WayPoint[] waypoints)
    {
        Debug.Log("making graph");
        // string jsonString = File.ReadAllText(jsonFilePath);
        // _myJson = JsonSerializer.Deserialize<List<WayPoint>>(jsonString);

        // _myJson = new List<WayPoint>(JsonHelper.FromJson<WayPoint>(jsonString));

        // _myJson = new List<WayPoint>(waypoints);
        _myJson = new List<WayPoint>();
        _myJson.AddRange(waypoints);



        _n = _myJson.Count;
        _vertices = new WayPoint[_n];
        _edges = new float[_n, _n];
        Array.Clear(_edges, 0, _n);

        int index = 0;
        foreach (var item in _myJson)
        {
            int[] connection = item.conns;

            foreach (var con in connection)
            {
                // Debug.Log($"{item.name}, {con}");

                _edges[index, con] = GetDistanceVectors(_myJson[index], _myJson[con]);
                _edges[con, index] = GetDistanceVectors(_myJson[index], _myJson[con]);
            }

            index++;
        }
    }

    public void printall()
    {
        for (int i = 0; i < _n; i++)
        {
            for (int j = 0; j < _n; j++)
            {
                Console.Write($"{_edges[i, j]}, ");
            }
            Console.WriteLine();
        }
    }



    public static float GetDistanceVectors(WayPoint w1, WayPoint w2)
    {
        Vector3 pos1 = new Vector3(w1.x, 0, w1.z);
        Vector3 pos2 = new Vector3(w2.x, 0, w2.z);
        return Vector3.Distance(pos1, pos2);
    }





    public static int[] getNeighbors(string name)
    {
        int[] neighbors;
        int index = getPosV(name);
        neighbors = _myJson[index].conns;
        return neighbors;
    }






    public List<int> Dijkstra(string start, string end)
    {
        int si = getPosV(start);
        int ei = getPosV(end);

        List<int> Q = new List<int>();
        HashSet<int> V = new HashSet<int>();
        Dictionary<int, int> P = new Dictionary<int, int>();

        Dictionary<int, float> DS = new Dictionary<int, float>();

        for (int k = 0; k < _myJson.Count; k++)
        {
            DS.Add(k, k == si ? 0 : int.MaxValue);
            P.Add(k, -1);
            Q.Add(k);
        }

        V.Add(si);

        while (Q.Count > 0)
        {
            Q.Sort((a, b) => (int)(DS[a] - DS[b])); // make priority Q

            int u = Q[0]; Q.RemoveAt(0);
            List<int> nb = new List<int>(_myJson[u].conns);
            nb.Sort((a, b) => (int)(DS[a] - DS[b]));

            foreach (int n in nb)
            {
                if (Q.FindIndex(e => n == e) == -1) continue;

                float dist = DS[u] + GetDistanceVectors(_myJson[u], _myJson[n]);
                if (dist < DS[n])
                {
                    DS[n] = dist;
                    P[n] = u;
                }
            }
        }

        List<int> path = new List<int>();
        int c = ei;
        if (P[c] != -1 || c == si)
        {
            while (c != -1)
            {
                path.Add(c);
                c = P[c];
            }
        }

        path.Reverse();

        return path;
    }





    public static int getMinDistance(float[] dist, bool[] explored)
    {
        float min = int.MaxValue;
        int minIndex = -1;

        for (int i = 0; i < _n; i++)
        {
            if (explored[i] == false && dist[i] <= min)
            {
                min = dist[i];
                minIndex = i;
            }
        }
        return minIndex;
    }



    public static int getPosV(string n)
    {
        for (int i = 0; i < _n; i++)
        {
            if (_myJson[i].name == n) return i;
        }

        return -1;
    }
}