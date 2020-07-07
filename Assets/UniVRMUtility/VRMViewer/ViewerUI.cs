using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UniHumanoid;
using UnityEngine;
using UnityEngine.UI;
using UniVRM10;

namespace UniVRMUtility.VRMViewer
{
    public class ViewerUI : MonoBehaviour
    {
        #region UI
        [SerializeField]
        private LicensePanel _licensePanel;

        [SerializeField]
        private MotionControlPanel _motionControlPanel;

        [SerializeField]
        private FacialExpressionPanel _facialExpressionPanel;

        [SerializeField]
        private InformationUpdate _informationUpdate;

        [SerializeField]
        private GUICollapse _closeGameObject;

        [SerializeField]
        private MessagePanel _errorMessagePanel;

        [SerializeField]
        private MessagePanel _pauseMessagePanel;

        [SerializeField]
        private GameObject _targetSphere;

        [SerializeField]
        private GameObject _targetCamera;

        [SerializeField]
        private GameObject _referenceObject;

        [SerializeField]
        private GameObject _canvasRoot;

        [SerializeField]
        private Text _version;

        [SerializeField]
        private Button _openVRM;

        [SerializeField]
        private Button _openBVH;

        [SerializeField]
        private Toggle _toggleMotionBVH;

        [SerializeField]
        private Toggle _toggleMotionTPose;

        [SerializeField]
        private Toggle _lookAtCamera;

        [SerializeField]
        private Toggle _lookAtSphere;

        [SerializeField]
        private Toggle _freeViewpointToggle;

        [SerializeField]
        private Toggle _faceViewToggle;

        [SerializeField]
        private HumanPoseClip _avatarTPose;

        private HumanPoseTransfer _bvhSource;
        private HumanPoseTransfer _loadedBvhSourceOnAvatar;
        private BvhImporterContext _bvhMotion;

        // GLTFからモデルのオブジェクト
        private GameObject _vrmModel = null;

        // BVHのオブジェクト
        private string _bvhPathLocal = null;
        private string _bvhPathSaved = null;
        // Pause the scene
        private bool _pause;
        // Initial_BVH_Crush_flag
        private bool _bvhLoadingTrigger = false;
        // VRMLookAtBlendShape flag
        private bool _lookAtBlendShapeFlag = false;
        #endregion

        private void Start()
        {
            _version.text = string.Format("VRMViewer UniVRM-{0}.{1}", VRMVersion.MAJOR, VRMVersion.MINOR);
            _pause = false;
            _openVRM.onClick.AddListener(OnOpenClickedVRM);
            _openBVH.onClick.AddListener(OnOpenClickedBVH);

            // Load initial motion
            string path = Application.streamingAssetsPath + "/VRM.Samples/Motions/test.txt";
            if (File.Exists(path))
            {
                LoadMotion(path);
                _bvhPathSaved = path;
            }

            string[] cmds = System.Environment.GetCommandLineArgs();
            if (cmds.Length > 1)
            {
                LoadModel(cmds[1]);
            }
        }

        private void LoadMotion(string path)
        {
            try
            {
                // Trigger BVH
                _bvhLoadingTrigger = true;
                // Save current path
                _bvhPathLocal = path;
                var previous_motion = _bvhMotion;
                if (previous_motion != null) { Destroy(previous_motion.Root); }

                var context = new UniHumanoid.BvhImporterContext();
                _bvhMotion = context;
                context.Parse(path);
                context.Load();
                if (context.Avatar == null || context.Avatar.isValid == false)
                {
                    if (context.Root != null) { Destroy(context.Root); }
                    throw new Exception("BVH importer failed");
                }

                // Send BVH 
                _informationUpdate.SetBVH(_bvhMotion.Root);

                SetMotion(context.Root.GetComponent<HumanPoseTransfer>());
            }
            catch (Exception e)
            {
                if (_bvhMotion.Root == true) { Destroy(_bvhMotion.Root); }
                _errorMessagePanel.SetMessage(MultipleLanguageSupport.BvhLoadErrorMessage + "\nError message: " + e.Message);
                throw;
            }
        }

        private void Update()
        {
            UIOperation();
        }

        private void UIOperation()
        {
            if (Input.GetKeyDown(KeyCode.Tab)) // hide the panel
            {
                if (_canvasRoot != null) { _canvasRoot.SetActive(!_canvasRoot.activeSelf); }
            }
            // Pause the rendering scene
            if (Input.GetKeyDown(KeyCode.P))
            {
                _pause = !_pause;
                _pauseMessagePanel.gameObject.SetActive(_pause);
                Time.timeScale = _pause ? 0 : 1;
            }
            // Resume the normal activity
            if (Input.GetKeyDown(KeyCode.R) && _errorMessagePanel.gameObject.activeSelf == true)
            {
                _errorMessagePanel.gameObject.SetActive(false);
                LoadMotion(_bvhPathSaved);
            }
        }

        private void OnOpenClickedVRM()
        {
            var path = ComDialog.Open("open VRM", "*.vrm");

            if (string.IsNullOrEmpty(path)) { return; }
            _errorMessagePanel.gameObject.SetActive(false);
            LoadModel(path);
        }

        private void OnOpenClickedBVH()
        {
            var path = ComDialog.Open("open BVH", "*.bvh");

            if (string.IsNullOrEmpty(path)) { return; }
            _errorMessagePanel.gameObject.SetActive(false);
            LoadMotion(path);
        }

        private void LoadModel(string path)
        {
            try
            {
                // If BVH trigger is still on
                if (_bvhLoadingTrigger == true) { LoadMotion(_bvhPathSaved); }

                if (!File.Exists(path)) { return; }

                Debug.LogFormat("{0}", path);
                var vrmModel = VrmLoader.CreateVrmModel(path);

                // Call License Update function
                _licensePanel.LicenseUpdatefunc(vrmModel);

                // UniVRM-0.XXのコンポーネントを構築する
                var assets = new ModelAsset();

                // build
                UnityBuilder.ToUnityAsset(vrmModel, assets);
                UniVRM10.ComponentBuilder.Build10(vrmModel, assets);

                // Set up Model
                SetModel(assets);
            }
            catch (Exception e)
            {
                _errorMessagePanel.SetMessage(MultipleLanguageSupport.VrmLoadErrorMessage + "\nError message: " + e.Message);
                throw;
            }
        }

        private void SetModel(ModelAsset assets)
        {
            var vrmModel = _vrmModel;
            vrmModel = assets.Root;

            // Cleanup
            var loaded = _loadedBvhSourceOnAvatar;
            _loadedBvhSourceOnAvatar = null;

            if (loaded != null)
            {
                Debug.LogFormat("destroy {0}", loaded);
                Destroy(loaded.gameObject);
            }

            if (vrmModel != null)
            {
                // Set up expressions
                _facialExpressionPanel.CreateDynamicObject(vrmModel);
                _informationUpdate.SetVRM(vrmModel);

                SkinnedMeshRenderer[] skinnedMeshes = vrmModel.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (var skinMesh in skinnedMeshes)
                {
                    skinMesh.updateWhenOffscreen = true;
                }

                // Set up LookAt
                var lookAt = vrmModel.GetComponent<VRMController>();
                if (lookAt != null)
                {
                    _loadedBvhSourceOnAvatar = vrmModel.AddComponent<HumanPoseTransfer>();

                    _loadedBvhSourceOnAvatar.Source = _bvhSource;
                    _motionControlPanel.LoadedBvhSourceOnAvatar = _loadedBvhSourceOnAvatar;

                    if (_toggleMotionBVH.isOn) { _loadedBvhSourceOnAvatar.SourceType = HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseTransfer; }
                    if (_toggleMotionTPose.isOn) { _loadedBvhSourceOnAvatar.SourceType = HumanPoseTransfer.HumanPoseTransferSourceType.HumanPoseClip; }

                    if (_faceViewToggle.isOn) { _closeGameObject.FaceCameraPropertyActivateVRM(); }

                    _motionControlPanel.AssignAutoPlay(vrmModel);

                    if (_lookAtSphere.isOn)
                        lookAt.Gaze = _targetSphere.transform;
                    else if (_lookAtCamera.isOn)
                        lookAt.Gaze = _targetCamera.transform;
                    else
                        lookAt.Gaze = _referenceObject.transform;

                    // Check the model's LookAt type
                    var animator = vrmModel.GetComponent<Animator>();
                    var leftEye = OffsetOnTransform.Create(animator.GetBoneTransform(HumanBodyBones.LeftEye)).Transform;
                    var rightEye = OffsetOnTransform.Create(animator.GetBoneTransform(HumanBodyBones.RightEye)).Transform;
                    if (leftEye == null && rightEye == null)
                    {
                        _lookAtBlendShapeFlag = true;
                        lookAt.LookAtType = VRMController.LookAtTypes.BlendShape;
                    }

                    // Send information
                    _informationUpdate.SetVRM(vrmModel);
                    _informationUpdate.SetLookAtType(_lookAtBlendShapeFlag);
                }

                // Set up animation
                var animation = vrmModel.GetComponent<Animation>();
                if (animation && animation.clip != null)
                {
                    animation.Play(animation.clip.name);
                }

                // Show mesh
                foreach (var r in assets.Renderers)
                {
                    r.enabled = true;
                }

                // VRMFirstPerson initialization
                var m_firstPerson = vrmModel.GetComponent<VRMFirstPerson>();
                if (m_firstPerson != null) { m_firstPerson.Setup(); }
                if (_freeViewpointToggle.isOn) { _closeGameObject.EnableFirstPersonModeOption(); }
            }
        }

        private void SetMotion(HumanPoseTransfer src)
        {
            if (src.Avatar.isValid)  // check whether the source is valid
            {
                _bvhSource = src;
                src.GetComponent<Renderer>().enabled = false;
                _motionControlPanel.BvhSource = _bvhSource;
                _bvhLoadingTrigger = false;
                _bvhPathSaved = _bvhPathLocal;

                _motionControlPanel.EnableBvh();
                _toggleMotionBVH.isOn = true;
                _toggleMotionTPose.isOn = false;
            }
        }
    }
}
