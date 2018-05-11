using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

[ExecuteInEditMode]
public class Rail : MonoBehaviour {

    private Transform[] nodes;

    private void Start()
    {
        nodes = new Transform[transform.childCount];
        int i = 0;
        foreach (Transform t in transform) {
            nodes[i] = t;
            i++;
        }
    }

#if UNITY_EDITOR
    void Update() {
        Start();
    }

    private void OnDrawGizmos() {
        for (int i = 0; i < nodes.Length - 1; i++) {
            UnityEditor.Handles.DrawDottedLine(nodes[i].position, nodes[i + 1].position, 3f);
        }
    }
#endif

    public IEnumerator MoveObjectAlongRail(Transform obj, float speed, bool lerpToFirstNode = false) {
        yield return MoveObjectAlongNodes(nodes, obj, speed, lerpToFirstNode);
    }

    public IEnumerator MoveObjectAlongRailReverse(Transform obj, float speed, bool lerpToFirstNode = false) {
        var newNodes = new Transform[nodes.Length];
        
        // Reverse the nodes
        for (int i = nodes.Length-1; i >= 0; i--) {
            newNodes[(nodes.Length-1) - i] = nodes[i];
        }

        yield return MoveObjectAlongNodes(newNodes, obj, speed, lerpToFirstNode);
    }

    public static IEnumerator MoveObjectAlongNodes(Transform[] nodes,Transform obj, float speed, bool lerpToFirstNode = false) {
        if (nodes.Length < 1) yield break;

        int currentTargetSegment = 0;
        if (!lerpToFirstNode) {
            obj.transform.position = nodes[0].position;
            currentTargetSegment = 1;
        }

        while (true) {

            // TODO: Should the rotation be hardcoded like this?
            var delta = nodes[currentTargetSegment].position - obj.position;
            var angle = Vector2.SignedAngle(Vector2.right, new Vector2(delta.x, delta.z));
            obj.eulerAngles = new Vector3(0, -angle, 0);

            // Handle the movement
            obj.transform.position = Vector3.MoveTowards(obj.transform.position, nodes[currentTargetSegment].position, Time.deltaTime * speed);
            if (Vector3.Distance(nodes[currentTargetSegment].position, obj.position) < 0.01f) {
                currentTargetSegment++;
                if (currentTargetSegment >= nodes.Length) {
                    yield break;
                }
            }

            yield return null;
        }
    }

    public Vector3 LinearPosition(int segmentOn, float ratio) {
        if (segmentOn >= nodes.Length - 1)
            return nodes[nodes.Length - 1].position;

        Vector3 p1 = nodes[segmentOn].position;
        Vector3 p2 = nodes[segmentOn + 1].position;

        return Vector3.Lerp(p1, p2, ratio);
    }

    public Vector3 CatmullPosition(int seg, float ratio) {
        Vector3 p1, p2, p3, p4;

        if (seg == 0) {
            p1 = nodes[seg].position;
            p2 = p1;
            p3 = nodes[seg + 1].position;
            p4 = nodes[seg + 2].position;
        }
        else if (seg == nodes.Length - 2) {
            p1 = nodes[seg - 1].position;
            p2 = nodes[seg].position;
            p3 = nodes[seg + 1].position;
            p4 = p3;
        }
        else {
            p1 = nodes[seg - 1].position;
            p2 = nodes[seg].position;
            p3 = nodes[seg + 1].position;
            p4 = nodes[seg + 2].position;
        }

        float t2 = ratio * ratio;
        float t3 = t2 * ratio;

        float x = 0.5f * 
            (
            (2.0f * p2.x) 
            + (-p1.x + p3.x)
            * ratio
            + (2.0f * p1.x - 5.0f * p2.x + 4 * p3.x -p4.x)
            * t2
            + (-p1.x + 3.0f * p2.x - 3.0f * p3.x + p4.x)
            * t3
            );

        float y = 0.5f *
            (
            (2.0f * p2.y)
            + (-p1.y + p3.y)
            * ratio
            + (2.0f * p1.y - 5.0f * p2.y + 4 * p3.y - p4.y)
            * t2
            + (-p1.y + 3.0f * p2.y - 3.0f * p3.y + p4.y)
            * t3
            );

        float z = 0.5f *
            (
            (2.0f * p2.z)
            + (-p1.z + p3.z)
            * ratio
            + (2.0f * p1.z - 5.0f * p2.z + 4 * p3.z - p4.z)
            * t2
            + (-p1.z + 3.0f * p2.z - 3.0f * p3.z + p4.z)
            * t3
            );

        return new Vector3(x, y, z); 
    }

    public Quaternion Orientation(int seg, float ratio) {
        Quaternion q1 = nodes[seg].rotation;
        Quaternion q2 = nodes[seg+1].rotation;

        return Quaternion.Lerp(q1, q2, ratio);
    }

    public Vector3 PositionOnRail(int seg, float ratio, Playmode mode) {
        switch (mode) {
            default:
            case Playmode.Linear:
                return LinearPosition(seg, ratio);
            case Playmode.Catmull:
                return CatmullPosition(seg, ratio);
        }
    }
}

public enum Playmode {
Linear,
Catmull
}
