using UnityEngine;

public class Rope : MonoBehaviour {


	public RopeData ropeData;

    public Transform anchor1, anchor2;

    LineRenderer lineRenderer;

    public int nodeCount = 10;
    public float nodeSpacing = 0.5f;
    public float gravity = -0.5f;
    public Vector3 windForce = Vector3.zero;
    public float friction = 0.02f;

    public bool drawGizmo = false;

    void Start () {

        lineRenderer = GetComponent<LineRenderer>();

        ropeData = new RopeData(nodeCount);
        ropeData.SetAnchor(0, true);
        ropeData.SetAnchor(nodeCount-1, true);

        //nodePrefabs = new Transform[nodeCount];
        //for(int i = 0; i < nodeCount; i++)
        //{
        //    nodePrefabs[i] = Instantiate(nodePrefab, ropeData.nodes[i].position(), nodePrefab.rotation);
        //}

        lineRenderer.positionCount = nodeCount;
    }

	void Update() {

        ropeData.MoveNode(anchor1.position, 0);
        ropeData.MoveNode(anchor2.position, nodeCount-1);
        ropeData.UpdateNodes(nodeSpacing, gravity, friction, windForce);

        //for (int i = 0; i < nodeCount; i++)
        //{
        //    nodePrefabs[i].position = ropeData.nodes[i].position();
        //}
        for (int i = 0; i < nodeCount; i++)
        {
            lineRenderer.SetPosition(i, ropeData.nodes[i].position);
        }
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying && drawGizmo)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < ropeData.nodes.Length - 1; i++)
            {
                Gizmos.DrawLine(ropeData.nodes[i].position, ropeData.nodes[i + 1].position);
            }
        }
    }

    #region Rope
    public class RopeData
    {
        int nodeCount;
        float nodeSpacing;
        float gravity;
        float friction;
        Vector3 windForce;

        int previousNodeIndex;

        public Node[] nodes;

        public RopeData(int _nodeCount)
        {
            nodeCount = _nodeCount;
            nodes = new Node[nodeCount];
        }

        public void MoveNode(Vector3 _position, int _nodeIndex)
        {
            nodes[_nodeIndex].position = _position;
        }

        public void SetAnchor(int _nodeIndex, bool _isAnchor)
        {
            nodes[_nodeIndex].isAnchor = _isAnchor;
        }

        public void UpdateNodes(float _nodeSpacing, float _gravity, float _friction, Vector3 _windForce)
        {
            nodeSpacing = _nodeSpacing;
            gravity = _gravity;
            friction = _friction;
            windForce = _windForce;

            previousNodeIndex = 0;

            for (int i = 0; i < nodeCount; i++)
            {
                if (!nodes[i].isAnchor)
                {
                    ProcessNodes(i);
                }
            }

            //reverse
            previousNodeIndex = nodeCount-1;

            for (int i = nodeCount-1; i >= 0; i--)
            {
                if (!nodes[i].isAnchor)
                {
                    ProcessNodes(i);
                }
            }
        }

        void ProcessNodes(int i)
        {
            Vector3 p = Vector3.zero;

            nodes[i].position += nodes[i].velocity;

            //much more understandable, but when using a vector the rope doesn't unfold when origin is at (0,0,0) 
            Vector3 dir = nodes[previousNodeIndex].position - nodes[i].position;
            dir.Normalize();

            p = nodes[i].position + dir * nodeSpacing;

            nodes[i].position = nodes[previousNodeIndex].position - (p - nodes[i].position);

            nodes[i].velocity = nodes[i].position - nodes[i].oldPosition;

            nodes[i].velocity *= friction * (1 - friction);

            nodes[i].velocity += windForce;
            nodes[i].velocity.y += gravity;

            nodes[i].oldPosition = nodes[i].position;

            previousNodeIndex = i;
        }
    }
    #endregion

    #region Node
    public struct Node
    {
        public Vector3 position;

        public Vector3 oldPosition;

        public Vector3 velocity;

        public bool isAnchor;
    }
    #endregion

}
