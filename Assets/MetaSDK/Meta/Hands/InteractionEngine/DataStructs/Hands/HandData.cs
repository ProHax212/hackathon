using UnityEngine;

namespace Meta.HandInput
{
    [System.Serializable]
    public class HandData
    {
        private readonly float _startTime;

        /// <summary> Unique id for hand </summary>
        public int HandId        { get; private set; }
        /// <summary> Hand's top point </summary>
        public Vector3 Top       { get; private set; }
        /// <summary> Hand's palm anchor </summary>
        public Vector3 Palm      { get; private set; }
        /// <summary> Hand's anchor </summary>
        public Vector3 Anchor    { get; private set; }
        /// <summary> Hand's grab anchor </summary>
        public Vector3 GrabAnchor{ get; private set; }
        /// <summary> Hand's grab value </summary>
        public float GrabValue   { get; private set; }
        /// <summary> hand's HandType </summary>
        public HandType HandType { get; private set; }

        /// <summary> Total time since hand became visible. </summary>
        public float TimeAlive
        {
            get
            {
                return Time.time - _startTime;
            }
        }

        public HandData(meta.types.HandData cocoHand, Transform origin = null)
        {
            _startTime = Time.time;
            AdoptProperties(cocoHand, origin);
        }

        /// <summary>
        /// Applies hand properties from input meta.types.HandData to current hand.
        /// </summary>
        /// <param name="cocoHand">Input data</param>
        /// <param name="origin">Depth camera's transform</param>
        public void AdoptProperties(meta.types.HandData cocoHand, Transform origin = null)
        {
            HandId = cocoHand.HandId;

            HandType = cocoHand.HandType == meta.types.HandType.RIGHT ? HandType.Right : HandType.Left;
            GrabValue = cocoHand.IsGrabbing ? 0f : 2f;

            Anchor = Vec3TToVector3(cocoHand.HandAnchor, origin);
            GrabAnchor = Vec3TToVector3(cocoHand.GrabAnchor, origin);
            Palm = Vec3TToVector3(cocoHand.HandAnchor, origin);
            Top = Vec3TToVector3(cocoHand.Top, origin);
        }

        private static Vector3 Vec3TToVector3(meta.types.Vec3T? position, Transform origin = null)
        {
            return origin == null
                ? position.Value.ToVector3()
                : origin.localToWorldMatrix.MultiplyPoint3x4(position.Value.ToVector3());
        }
    };
}
