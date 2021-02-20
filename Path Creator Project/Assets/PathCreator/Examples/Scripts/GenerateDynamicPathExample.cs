using UnityEngine;

namespace PathCreation.Examples {
    // Example of creating a path at runtime from a set of points.

    [RequireComponent(typeof(PathCreator))]
    public class GenerateDynamicPathExample : MonoBehaviour {

        bool closedLoop = false;
        public PathFollowerAdvanced pathFollower;
        PathCreator pathCreator;

        private void Awake()
        {

            pathCreator = GetComponent<PathCreator>();
            if(pathCreator == null)
            {
                Debug.LogError("pathCreator is null");
            }

            if(pathFollower == null)
            {
                Debug.LogError("path follower is null");
            }
        }

        void Start () {

            Vector3[] pts = { Vector3.zero, Random.onUnitSphere * Random.Range(5f,15f) };

            BezierPath bezierPath = new BezierPath(pts, closedLoop, PathSpace.xyz);
            pathCreator.bezierPath = bezierPath;
            bezierPath.ControlPointMode = BezierPath.ControlMode.Aligned;

            // Need enough points that follower doesn't run off the path
            for (int i = 0; i < 5; ++i)
                AddRandomSegmentToPath();

        }

        // Need for random rotations. Uses plane eqn to find perpendicular vector
        // if v is (approximately) zero, returns zero
        Vector3 AnyPerpUnitV(Vector3 v)
        {
            Vector3 ret = Vector3.zero;

            if (!Mathf.Approximately(0f, v.x))
            {
                ret = new Vector3((-v.y - v.z) / v.x, 1f, 1f);
            }
            else if (!Mathf.Approximately(0f, v.y))
            {
                ret = new Vector3(1f, (-v.x - v.z) / v.y, 1f);
            }
            else if (!Mathf.Approximately(0f, v.z))
            {
                ret = new Vector3(1f, 1f, (-v.x - v.y) / v.z);
            }

            return ret.normalized;
        }



        void AddRandomSegmentToPath()
        {

            var bp = pathCreator.bezierPath;

            var lasti = bp.NumSegments - 1;
            var lastSeg = bp.GetPointsInSegment(lasti);

            var lastAnchor = lastSeg[3];
            var nextToLastAnchor = lastSeg[0];
            var lastControl = lastSeg[2];
            var contDir = (lastAnchor - lastControl).normalized;

            // any perpendicular v will do
            Vector3 perpDir = AnyPerpUnitV(contDir);

            var angleRange = 30f;

            var angRot = Quaternion.AngleAxis(Random.Range(0f, angleRange), perpDir);

            var newAnchorDir = angRot * (contDir * Random.Range(2f, 15f));

            angRot = Quaternion.AngleAxis(Random.Range(-180f, 180f), contDir);

            var newAnchor = lastAnchor + angRot * newAnchorDir;

            bp.AddSegmentToEnd(newAnchor);

        }



        private void Update()
        {
            //var pathLength = pathCreator.path.length;
            //var dist = pathFollower.distanceTravelled;
            var currSeg = pathFollower.currentBezierSegment;

            var bp = pathCreator.bezierPath;

            if (currSeg > 2)
            {
                // del first seg
                bp.DeleteSegment(0);

                // replace del seg with new random one at the end
                AddRandomSegmentToPath();

            }

        }

    }
}