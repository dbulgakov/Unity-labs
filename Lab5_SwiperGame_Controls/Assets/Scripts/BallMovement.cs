using UnityEngine;
using GG.Infrastructure.Utils.Swipe;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

public class BallMovement : MonoBehaviour 
{
    [SerializeField] private SwipeListener swipeListener;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private float stepDuration = 0.1f;
    [SerializeField] private LayerMask wallsAndRoadsLayer;
    
    private const float MAX_RAY_DISTANCE = 10f;
    public UnityEvent<List<RoadTile>, float> onMoveStart = new UnityEvent<List<RoadTile>, float>();
    private Vector3 moveDirection;
    private bool canMove = true;

    private void Start()
    {
        if (swipeListener != null)
        {
            swipeListener.OnSwipe.AddListener(HandleSwipe);
        }
        
        if (levelManager != null && levelManager.defaultBallRoadTile != null)
        {
            transform.position = levelManager.defaultBallRoadTile.position;
        }
    }

    private void OnDisable()
    {
        if (swipeListener != null)
        {
            swipeListener.OnSwipe.RemoveListener(HandleSwipe);
        }
    }

    private void HandleSwipe(string swipe)
    {
        switch (swipe)
        {
            case "Right":
                moveDirection = Vector3.right;
                break;
            case "Left":
                moveDirection = Vector3.left;
                break;
            case "Up":
                moveDirection = Vector3.forward;
                break;
            case "Down":
                moveDirection = Vector3.back;
                break;
            default:
                return;
        }
        
        MoveBall();
    }

    private void MoveBall()
    {
        if (!canMove) return;
        
        // Slightly raise the raycast origin to better detect road tiles
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        
        RaycastHit[] hits = Physics.RaycastAll(
            rayStart,
            moveDirection,
            MAX_RAY_DISTANCE,
            wallsAndRoadsLayer.value
        );

        if (hits.Length == 0) 
        {
            Debug.Log("No hits detected");
            return;
        }

        // Sort hits by distance
        var sortedHits = hits.OrderBy(hit => hit.distance).ToList();
        
        List<RoadTile> pathRoadTiles = new List<RoadTile>();
        Vector3 targetPosition = transform.position;
        bool hitWall = false;

        foreach (var hit in sortedHits)
        {
            // If we hit a wall and it's the first thing we hit, stop
            if (!hit.collider.isTrigger && hit.distance < 0.1f)
            {
                Debug.Log("Wall directly in front");
                return;
            }
            
            // If we hit a wall after some road tiles, stop here
            if (!hit.collider.isTrigger)
            {
                hitWall = true;
                break;
            }

            // Must be a road tile
            RoadTile roadTile = hit.transform.GetComponent<RoadTile>();
            if (roadTile != null)
            {
                pathRoadTiles.Add(roadTile);
                targetPosition = new Vector3(
                    hit.transform.position.x,
                    transform.position.y, // Keep the same Y position
                    hit.transform.position.z
                );
            }
        }

        // Only move if we found road tiles
        if (pathRoadTiles.Count > 0)
        {
            canMove = false;
            float duration = stepDuration * pathRoadTiles.Count;

            // Kill any existing tweens on this transform
            DOTween.Kill(transform);

            transform.DOMove(targetPosition, duration)
                .SetEase(Ease.OutExpo)
                .OnComplete(() => {
                    canMove = true;
                });

            onMoveStart?.Invoke(pathRoadTiles, duration);
        }
    }
}