﻿using UnityEngine;

namespace PathCreation.Examples {
    // Example of creating a path at runtime from a set of points.

    [RequireComponent(typeof(PathCreator))]
    public class GenerateDynamicPathExample : MonoBehaviour {

        bool closedLoop = false;
        public PathFollowerAdvanced pathFollower;
        PathCreator pathCreator;

        public bool Mode2D = true;

        public float RandAbsAngle = 30f;

        public float DB_followerPos = 0f;

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

            Vector3 v2 = Mode2D ?
                Quaternion.AngleAxis(Random.Range(-180f, 180f), Vector3.up) * Vector3.right :
                Random.onUnitSphere * Random.Range(5f, 15f);

            Vector3[] pts = { Vector3.zero, v2 };

            BezierPath bezierPath = new BezierPath(pts, closedLoop, Mode2D ? PathSpace.xz : PathSpace.xyz);
            pathCreator.bezierPath = bezierPath;
            bezierPath.ControlPointMode = BezierPath.ControlMode.Aligned;

            // Need enough points that follower doesn't run off the path
            for (int i = 0; i < 7; ++i)
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



        Vector3 GenerateRandomAnchor()
        {
            var bp = pathCreator.bezierPath;

            var lasti = bp.NumSegments - 1;
            var lastSeg = bp.GetPointsInSegment(lasti);

            var lastAnchor = lastSeg[3];
            var nextToLastAnchor = lastSeg[0];
            var lastControl = lastSeg[2];
            var contDir = (lastAnchor - lastControl).normalized;

            // any perpendicular v will do
            Vector3 perpDir = Mode2D ? Vector3.up : AnyPerpUnitV(contDir);

            var angleRange = RandAbsAngle;

            var angRot = Quaternion.AngleAxis(Random.Range(-angleRange, angleRange), perpDir);

            var newAnchorDir = angRot * (contDir * Random.Range(2f, 15f));

            Vector3 newAnchor = Vector3.zero;

            if (Mode2D)
            {
                newAnchor = lastAnchor + newAnchorDir;

                newAnchor.y = 0f;
            }
            else
            {
                angRot = Quaternion.AngleAxis(Random.Range(-180f, 180f), contDir);

                newAnchor = lastAnchor + angRot * newAnchorDir;
            }

            return newAnchor;

        }


        void AddRandomSegmentToPath()
        {

            var bp = pathCreator.bezierPath;

            var newAnchor = GenerateRandomAnchor();

            //bp.AddSegmentToEnd(newAnchor);

            bp.AddSegmentToEnd(newAnchor);

        }


        void RemoveFirstAndAddRandomSegmentToPath()
        {

            var bp = pathCreator.bezierPath;

            var newAnchor = GenerateRandomAnchor();

            //bp.AddSegmentToEnd(newAnchor);

            bp.DeleteSegmentFromBeginningAndAddToEnd(newAnchor);

        }



        private void Update()
        {
            //var pathLength = pathCreator.path.length;
            //var dist = pathFollower.distanceTravelled;
            var currSeg = pathFollower.currentBezierSegment;

            DB_followerPos = pathFollower.distanceTravelled;

            var bp = pathCreator.bezierPath;

            if (currSeg > 3)
            {
                // del first seg
                //bp.DeleteSegment(0);

                // replace del seg with new random one at the end
                RemoveFirstAndAddRandomSegmentToPath();

            }

        }

    }
}