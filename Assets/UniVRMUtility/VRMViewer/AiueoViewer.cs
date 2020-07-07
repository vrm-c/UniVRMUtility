using System.Collections;
using UnityEngine;
using UniVRM10;
using VrmLib;

namespace UniVRMUtility.VRMViewer
{
    public class AiueoViewer : MonoBehaviour
    {
        [SerializeField]
        public VRMController BlendShapes;
        private void Reset()
        {
            BlendShapes = GetComponent<VRMController>();
        }

        Coroutine m_coroutine;

        [SerializeField]
        float m_wait = 0.5f;

        private void Awake()
        {
            if (BlendShapes == null)
            {
                BlendShapes = GetComponent<VRMController>();
            }
        }

        IEnumerator RoutineNest(BlendShapePreset preset, float velocity, float wait)
        {
            for (var value = 0.0f; value <= 1.0f; value += velocity)
            {
                BlendShapes.SetPresetValue(preset, value);
                yield return null;
            }
            BlendShapes.SetPresetValue(preset, 1.0f);
            yield return new WaitForSeconds(wait);
            for (var value = 1.0f; value >= 0; value -= velocity)
            {
                BlendShapes.SetPresetValue(preset, value);
                yield return null;
            }
            BlendShapes.SetPresetValue(preset, 0);
            yield return new WaitForSeconds(wait * 2);
        }

        IEnumerator Routine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);

                var velocity = 0.1f;

                yield return RoutineNest(BlendShapePreset.Aa, velocity, m_wait);
                yield return RoutineNest(BlendShapePreset.Ih, velocity, m_wait);
                yield return RoutineNest(BlendShapePreset.Ou, velocity, m_wait);
                yield return RoutineNest(BlendShapePreset.Ee, velocity, m_wait);
                yield return RoutineNest(BlendShapePreset.Oh, velocity, m_wait);
            }
        }

        private void OnEnable()
        {
            m_coroutine = StartCoroutine(Routine());
        }

        private void OnDisable()
        {
            StopCoroutine(m_coroutine);
        }
    }
}
