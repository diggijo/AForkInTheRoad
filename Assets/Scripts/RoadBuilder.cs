using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoadBuilder : MonoBehaviour
{
    public static RoadBuilder Instance { get; private set; }
    private LineRenderer currentRoad;
    private LineRenderer currentFork;
    private Vector2 mousePos;
    private Vector2 startMousePos;
    private Vector2 endMousePos;
    private Vector2 mirroredPosition;
    private Vector2 forkDirection;
    private bool isForking = false;
    private bool isDrawingRoad = false;

    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private LineRenderer roadPrefab;
    private float totalDistance = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (GameInput.Instance.IsLeftClickPressed())
        {
            if (!isDrawingRoad)
            {
                StartRoad();
            }
            else if (isForking)
            {
                EndFork();
                EndRoad();
            }
            else
            {
                EndRoad();
            }
        }

        if (isForking)
        {
            UpdateFork();
        }

        if (isDrawingRoad)
        {
            UpdateRoad();
        }
    }

    private void StartRoad()
    {
        Collider2D hit = GetColliderUnderMouse();

        if (hit == null)
        {
            return;
        }

        RoadNode node = hit.GetComponent<RoadNode>();

        if (node != null)
        {
            startMousePos = node.transform.position;
        }
        else
        {
            startMousePos = GetMouseWorldPosition();
        }

        isDrawingRoad = true;

        currentRoad = Instantiate(roadPrefab);
        currentRoad.positionCount = 2;

        SetLinePosition(currentRoad, 0, startMousePos);
        SetLinePosition(currentRoad, 1, startMousePos);

        DisplayDistance(totalDistance);
    }

    private void UpdateRoad()
    {
        mousePos = GetMouseWorldPosition();

        RoadNode node = GetNodeUnderMouse();

        if (node != null)
        {
            mousePos = node.transform.position;
        }

        SetLinePosition(currentRoad, 1, mousePos);

        float currentRoadDistance = CalculateDistance(startMousePos, mousePos);

        float currentForkDistance = 0f;

        if (isForking)
        {
            currentForkDistance = CalculateDistance(startMousePos, mirroredPosition);
        }

        DisplayDistance(totalDistance + currentRoadDistance + currentForkDistance);
    }

    private void EndRoad()
    {
        Collider2D hit = GetColliderUnderMouse();

        if (hit == null)
        {
            endMousePos = SnapToGrid(GetMouseWorldPosition());
        }
        else
        {
            RoadNode node = hit.GetComponent<RoadNode>();

            if (node != null)
            {
                endMousePos = node.transform.position;
            }
            else
            {
                endMousePos = SnapToGrid(GetMouseWorldPosition());
            }
        }

        SetLinePosition(currentRoad, 0, startMousePos);
        SetLinePosition(currentRoad, 1, endMousePos);

        CreateEdgeCollider(currentRoad, startMousePos, endMousePos);

        totalDistance += CalculateDistance(startMousePos, endMousePos);

        DisplayDistance(totalDistance);

        isDrawingRoad = false;
        currentRoad = null;

        if (!isForking)
        {
            if (hit != null)
            {
                return;
            }

            CreateFork();
        }
    }

    private float CalculateDistance(Vector3 endPos, Vector3 startPos)
    {
        return (endPos - startPos).magnitude;
    }

    private void DisplayDistance(float distance)
    {           
        distanceText.text = distance.ToString("F2") + " meters";
    }

    private void SetLinePosition(LineRenderer lineRenderer, int index, Vector2 position)
    {
        lineRenderer.SetPosition(index, new Vector3(position.x, position.y, 0));
    }

    private Collider2D GetColliderUnderMouse()
    {
        return Physics2D.OverlapPoint(GetMouseWorldPosition());
    }

    private RoadNode GetNodeUnderMouse()
    {
        Collider2D hit = GetColliderUnderMouse();

        if (hit == null)
            return null;

        return hit.GetComponent<RoadNode>();
    }

    private Vector2 GetMouseWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(
            GameInput.Instance.GetMousePosition()
        );
    }

    private void CreateEdgeCollider(LineRenderer lineRenderer, Vector2 startPos, Vector2 endPos)
    {
        EdgeCollider2D edgeCollider = lineRenderer.GetComponent<EdgeCollider2D>();

        edgeCollider.points = new Vector2[]
        {
            startPos, endPos
        };

        edgeCollider.edgeRadius = .25f;
    }

    private void CreateFork()
    {
        isForking = true;

        forkDirection = (endMousePos - startMousePos).normalized;

        startMousePos = endMousePos;

        currentFork = Instantiate(roadPrefab);
        currentFork.positionCount = 2;

        currentRoad = Instantiate(roadPrefab);
        currentRoad.positionCount = 2;

        SetLinePosition(currentRoad, 0, startMousePos);
        SetLinePosition(currentRoad, 1, startMousePos);

        isDrawingRoad = true;
    }

    private void UpdateFork()
    {
        mousePos = GetMouseWorldPosition();

        RoadNode node = GetNodeUnderMouse();

        if (node != null)
        {
            mousePos = node.transform.position;
        }

        Vector2 perpendicular = new Vector2( -forkDirection.y, forkDirection.x);

        Vector2 offset = mousePos - startMousePos;

        float perpendicularDistance = Vector2.Dot(offset, perpendicular);

        mirroredPosition = mousePos - 2f * perpendicularDistance * perpendicular;

        Collider2D hit = Physics2D.OverlapPoint(mirroredPosition);

        if (hit != null)
        {
            RoadNode node1 = hit.GetComponent<RoadNode>();

            if (node1 != null)
            {
                mirroredPosition = node1.transform.position;
            }
        }

        SetLinePosition(currentRoad, 0, startMousePos);
        SetLinePosition(currentRoad, 1, mousePos);

        SetLinePosition(currentFork, 0, startMousePos);
        SetLinePosition(currentFork, 1, mirroredPosition);
    }

    private Vector2 SnapToGrid(Vector2 position)
    {
        return new Vector2(Mathf.Round(position.x), Mathf.Round(position.y));
    }

    private void EndFork()
    {
        Debug.Log("Mirrored Position: " + mirroredPosition);
        mirroredPosition = SnapToGrid(mirroredPosition);
        Debug.Log("Mirrored Position after Snap: " + mirroredPosition);

        SetLinePosition(currentFork, 0, startMousePos);
        SetLinePosition(currentFork, 1, mirroredPosition);
        CreateEdgeCollider(currentFork, startMousePos, mirroredPosition);

        totalDistance += CalculateDistance(startMousePos, mirroredPosition);

        DisplayDistance(totalDistance);

        isForking = false;
        currentFork = null;
    }
}
