using UnityEngine;
using UnityEngine.UI;
using UniVRM10;

namespace UniVRMUtility.VRMViewer
{
    public class TargetMover : MonoBehaviour
    {
        [SerializeField]
        private GUICollapse _closeGameObject;

        [SerializeField]
        private GameObject _targetSphere;

        [SerializeField]
        private GameObject _targetCamera;

        [SerializeField]
        private GameObject _referenceObject;

        [SerializeField]
        private Toggle _lookStraightAheadToggle;

        [SerializeField]
        private Toggle _lookAtSphereToggle;

        [SerializeField]
        private Slider _orbitalRadius;

        [SerializeField]
        private Slider _verticalPosition;

        [SerializeField]
        private float _sphereOrbitalRadius = 0.0f;

        [SerializeField]
        private float _angluarVelocity = -70.0f;

        [SerializeField]
        private float _sphereInitialHeight = 1.0f;

        [SerializeField]
        private float _sphereMovableRangeInVertical = 0.8f;

        [SerializeField]
        private float _currentAngle = 0.0f;

        private GameObject _vrmModel = null;
        private GameObject _bvhGameObject = null;
        public GameObject VrmModel { set { _vrmModel = value; } }
        public GameObject BvhGameObject { set { _bvhGameObject = value; } }

        private void Start()
        {
            // Add listener to eye operation mode
            _lookStraightAheadToggle.onValueChanged.AddListener(EyeLookStraightAheadValueChanged);
            _lookAtSphereToggle.onValueChanged.AddListener(EyeLookAtSphereValueChanged);
        }

        private void LateUpdate()
        {
            _currentAngle += _angluarVelocity * Time.deltaTime * Mathf.Deg2Rad;

            var x = Mathf.Cos(_currentAngle) * (_sphereOrbitalRadius + _orbitalRadius.value);
            var z = Mathf.Sin(_currentAngle) * (_sphereOrbitalRadius + _orbitalRadius.value);
            var y = (_sphereInitialHeight + _verticalPosition.value) + 
                     _sphereMovableRangeInVertical * Mathf.Cos(_currentAngle / 3);            
            transform.localPosition = new Vector3(x, y, z);

            // Fly-through viewpoint (virtual camera)
            if (_bvhGameObject != null)
            {
                if (_vrmModel != null)
                {
                    var tLookAt = _vrmModel.GetComponent<VRMController>().Head.position;
                    transform.LookAt(new Vector3(tLookAt.x, tLookAt.y, tLookAt.z));
                }
                else
                {
                    transform.LookAt(new Vector3(0.0f, 1.2f, 0.0f));
                }
            }
            else
            {
                transform.LookAt(new Vector3(0.0f, 1.2f, 0.0f));
            }
        } // update

        private void EyeLookStraightAheadValueChanged(bool _)
        {
            Toggle eyeOperationModeLookStraightAhead = _lookStraightAheadToggle;

            if (eyeOperationModeLookStraightAhead.isOn)
            {
                if (_vrmModel != null)
                {
                    // Make eyes static
                    _vrmModel.GetComponent<VRMController>().Gaze = _referenceObject.transform;
                }
                _closeGameObject.DisableSphere();
            }
        }

        private void EyeLookAtSphereValueChanged(bool _)
        {
            Toggle eyeOperationModeLookAtSphere = _lookAtSphereToggle;

            if (eyeOperationModeLookAtSphere.isOn)
            {
                if (_vrmModel != null)
                {
                    _vrmModel.GetComponent<VRMController>().Gaze = _targetSphere.transform;
                }
                _closeGameObject.EnableSphere();
            }
        }

    } // class
} // namespace