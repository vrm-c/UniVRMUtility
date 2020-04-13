using System.Collections.Generic;
using UnityEngine;
using UniVRM10;

namespace UniVRMUtility.VRMViewer
{
    public class InformationUpdate : MonoBehaviour
    {
        [SerializeField]
        private RokuroCameraViewer _rokuroCamera;

        [SerializeField]
        private TargetMover _targetMover;

        [SerializeField]
        private EyeControlCamera _eyeControlCamera;

        [SerializeField]
        private FaceView _faceView;

        [SerializeField]
        private FacialExpressionPanel _facialExpressionPanel;

        [SerializeField]
        private FlyThroughCameraView _flyThroughCameraView;

        [SerializeField]
        private ViewpointPanel _viewpointPanel;

        [SerializeField]
        private MultipleLanguageSupport _multiLanguageSupport;

        [SerializeField]
        private GUICollapse _closeGameObject;

        public void SetVRM(GameObject VRM)
        {
            _targetMover.VrmModel = VRM; 
            _eyeControlCamera.VrmModel = VRM;
            _faceView.VrmModel = VRM;
            _facialExpressionPanel.VrmModel = VRM;
            _flyThroughCameraView.VrmModel = VRM;
            _multiLanguageSupport.VrmModel = VRM;
            _viewpointPanel.VrmModel = VRM;
            _closeGameObject.VrmModel = VRM;
        }

        public void SetLookAtType(bool lookAtBlendShape)
        {
            _facialExpressionPanel.LookAtBlendShape = lookAtBlendShape;
        }

        public void SetBVH(GameObject BVH)
        {
            _targetMover.BvhGameObject = BVH;
            _flyThroughCameraView.BvhGameObject = BVH;
        }

        public void SetExpression(List<GameObject> objs, int validExpNum)
        {
            _rokuroCamera.Objs = objs;
            _rokuroCamera.ValidExpNum = validExpNum;
        }
    }
}
