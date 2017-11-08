using Meta.HandInput;
using UnityEngine;

namespace Meta 
{
    /// <summary>
    /// Interaction to grab the model to translate its position.
    /// </summary>
    [AddComponentMenu("Meta/Interaction/GrabInteraction")]
    public class GrabInteraction : Interaction
    {
        private HandFeature _handFeature;

        protected override bool CanEngage(Hand handProxy)
        {
            return GrabbingHands.Count == 1;
        }

        protected override void Engage()
        {
            _handFeature = GrabbingHands[0];

            //rigidbody should be kinematic as to not interfere with grab translation
            SetIsKinematic(true);

            SetGrabOffset(GrabbingHands[0].transform.position);
        }

        protected override bool CanDisengage(Hand handProxy)
        {
            return _handFeature != null && handProxy == _handFeature.Hand;
        }

        protected override void Disengage()
        {
            SetIsKinematic(false);
            _handFeature = null;
        }

        protected override void Manipulate()
        {
            Move(_handFeature.transform.position);
        }
    }
}
