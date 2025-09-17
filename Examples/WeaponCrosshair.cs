using UnityEngine;
using Remedy.UI;

namespace Remedy.Weapons.UI
{
    public class WeaponCroshair : MonoBehaviour
    {
        public ScriptableEventVector3.Input GoalAim;
        public ScriptableEventVector3.Input ActualAim;

        public UISprite MainCrosshair;
        public UISprite LeadingCrosshair;
        public Transform LeadRoot;
        public float Distance = 3f;

        private Vector3 _goalAim;
        private Vector3 _actualAim;

        private void OnEnable()
        {
            GoalAim?.Subscribe(this, (Vector3 val) => _goalAim = val);
            ActualAim?.Subscribe(this, (Vector3 val) => _actualAim = val);
        }

        private void OnValidate()
        {
            if (MainCrosshair == null)
                MainCrosshair = UISprite.New("Main Crosshair", transform, 1);
            if (LeadingCrosshair == null)
                LeadingCrosshair = UISprite.New("Leading Crosshair", transform, 1);
            if (LeadRoot == null)
            {
                LeadRoot = new GameObject("Lead Root").transform;
                LeadingCrosshair.transform.parent = LeadRoot;
            }
            LeadRoot.parent = UIManager.Camera.transform;
            LeadRoot.position = UIManager.Camera.transform.position;
            LeadingCrosshair.transform.localPosition = new Vector3(0, 0, -Distance);
        }

        private void Update()
        {
            Vector3 offset;

            offset = Quaternion.FromToRotation(_goalAim, _actualAim) * Vector3.forward;
            if (Vector3.Dot(_goalAim, Vector3.forward) < 0)
                offset = -offset;

            offset.z = -1;
            LeadingCrosshair.transform.localPosition = offset * -Distance;

            // LeadingCrosshair.transform.localPosition = (directionOffset * magnitide) * 3.14f;    
        }
    }
}