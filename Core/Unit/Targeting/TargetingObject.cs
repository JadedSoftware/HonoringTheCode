using Core.GameManagement;
using Core.Helpers;
using Core.Unit.Model;
using UnityEngine;
using EventSenderController = Core.GameManagement.EventSenders.EventSenderController;

namespace Core.Unit.Targeting
{
    public class TargetingObject : MonoBehaviour
    {
        public TargetingHealthBar healthBar;
        public IDamageable damageable;
        private ModelContainer model;
        private SphereCollider targetCollider;
        public TargetingObjectType targetObjectType => TargetHelpers.AssignTargetType(name.ToUpper().Trim());
        private TargetingController targetingController => TargetingController.instance;

        public void OnEnable()
        {
            gameObject.layer = (int) LayersEnum.Target;
            targetCollider = gameObject.AddComponent<SphereCollider>();
            targetCollider.radius = .25f;
            damageable = gameObject.GetComponentInParent<IDamageable>();
            targetingController.RegisterTargetObjects(damageable, this);
            damageable.RegisterDamageableTarget(this);
            RegesterEvents();
        }

        private void OnDisable()
        {
            UnRegestierEvents();
        }

        private void RegesterEvents()
        {
            EventSenderController.engageTargetingObject += EngageTargetingObject;
            EventSenderController.changeTargetingObject += ChangeTargetingObject;
            EventSenderController.disengageTargetingObject += DisengageTargetingObject;
        }

        private void UnRegestierEvents()
        {
            EventSenderController.engageTargetingObject += EngageTargetingObject;
            EventSenderController.changeTargetingObject += ChangeTargetingObject;
            EventSenderController.disengageTargetingObject += DisengageTargetingObject;
        }

        private void EngageTargetingObject(TargetingObject target)
        {
            //throw new NotImplementedException();
        }

        private void ChangeTargetingObject(TargetingObject target)
        {
            //throw new NotImplementedException();
        }

        private void DisengageTargetingObject()
        {
            //throw new NotImplementedException();
        }
    }
}