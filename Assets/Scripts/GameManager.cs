using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private RoadNode playerNode;
    private bool isGameWon = false;
    private void Awake()
    {
        Instance = this;
        isGameWon = false;
    }

    private void Start()
    {
        RoadNode[] nodes = FindObjectsByType<RoadNode>();

        foreach (RoadNode node in nodes)
        {
            if (node.IsPlayer)
            {
                playerNode = node;
                break;
            }
        }
    }

    public void CheckConnections()
    {
        EdgeCollider2D[] roads = FindObjectsByType<EdgeCollider2D>();

        List<Vector2> connectedPoints = new List<Vector2>();

        connectedPoints.Add(playerNode.transform.position);

        bool foundNewConnection = true;

        while (foundNewConnection)
        {
            foundNewConnection = false;

            foreach (EdgeCollider2D road in roads)
            {
                Vector2 start = road.transform.TransformPoint(road.points[0]);
                Vector2 end = road.transform.TransformPoint(road.points[1]);

                bool startConnected = IsPointConnected(start, connectedPoints, roads);
                bool endConnected = IsPointConnected(end, connectedPoints, roads);

                if (startConnected && !IsPointConnected(end, connectedPoints, roads))
                {
                    connectedPoints.Add(end);
                    foundNewConnection = true;
                }

                if (endConnected && !IsPointConnected(start, connectedPoints, roads))
                {
                    connectedPoints.Add(start);
                    foundNewConnection = true;
                }
            }
        }

        RoadNode[] nodes = FindObjectsByType<RoadNode>();

        int destinationCount = 0;
        int connectedDestinationCount = 0;

        foreach (RoadNode node in nodes)
        {
            if (!node.IsDestination)
                continue;

            destinationCount++;

            if (IsPointConnected(node.transform.position, connectedPoints, roads))
            {
                connectedDestinationCount++;
            }
        }

        if (connectedDestinationCount == destinationCount)
        {
            Debug.Log("YOU WIN!");
            isGameWon = true;
        }
    }

    private bool IsPointConnected(Vector2 point, List<Vector2> connectedPoints, EdgeCollider2D[] roads)
    {
        foreach (Vector2 connectedPoint in connectedPoints)
        {
            if (Vector2.Distance(point, connectedPoint) < 0.01f)
            {
                return true;
            }
        }

        foreach (EdgeCollider2D road in roads)
        {
            Vector2 start = road.transform.TransformPoint(road.points[0]);
            Vector2 end = road.transform.TransformPoint(road.points[1]);

            if (IsPointOnLine(point, start, end))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPointOnLine(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 closestPoint = Vector2.Lerp(
            start, end,Mathf.Clamp01(
                Vector2.Dot(point - start, end - start) /
                Vector2.Dot(end - start, end - start)
            )
        );

        return Vector2.Distance(point, closestPoint) < 0.01f;
    }
}