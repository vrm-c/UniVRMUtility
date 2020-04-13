using UnityEngine;
using UnityEngine.UI;
using UniVRM10;

namespace UniVRMUtility.VRMViewer
{
    public class EyeControlCamera : MonoBehaviour
    {
        [SerializeField]
        private GUICollapse _closeGameObject;

        [SerializeField]
        private GameObject _mainCamera;

        [SerializeField]
        private GameObject _targetSphere;

        [SerializeField]
        private GameObject _targetCamera;

        [SerializeField]
        private GameObject _flyThroughCameraView;

        [SerializeField]
        private Toggle _lookAtCameraToggle;

        [SerializeField]
        private Toggle _cameraAutoChangedViewpoint;

        private GameObject _vrmModel = null;
        public GameObject VrmModel { set { _vrmModel = value; } }

        private void Start()
        {
            // Add listener to eye operation mode
            _lookAtCameraToggle.onValueChanged.AddListener(EyeLookAtCameraValueChanged);
        }

        private void LateUpdate()
        {
            if(_lookAtCameraToggle.isOn)
            {
                if (_cameraAutoChangedViewpoint.isOn)
                {
                    var pos = _flyThroughCameraView.transform.position;
                    transform.localPosition = new Vector3(pos.x + Time.deltaTime * 0.001f, pos.y, pos.z);
                }
                else
                {
                    var pos = _mainCamera.transform.position;
                    transform.localPosition = new Vector3(pos.x + Time.deltaTime * 0.001f, pos.y, pos.z);
                }
            }
        }

        private void EyeLookAtCameraValueChanged(bool _)
        {
            Toggle eyeOperationModeLookAtCamera = _lookAtCameraToggle;

            if (eyeOperationModeLookAtCamera.isOn)
            {
                if (_vrmModel != null)
                {
                    _vrmModel.GetComponent<VRMBlendShapeProxy>().Gaze = _targetCamera.transform;
                }
                _closeGameObject.DisableSphere();
            }
        }
    }
}
