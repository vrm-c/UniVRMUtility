using System.Collections;
using UnityEngine;
using UniVRM10;
using VrmLib;

namespace UniVRMUtility.SimpleViewer
{
    /// <summary>
    /// LipSyncの あ、い、う、え、お を繰り返す
    /// </smumary>
    public class AIUEO : MonoBehaviour
    {
        [SerializeField]
        public VRMBlendShapeProxy BlendShapes;

        void Reset()
        {
            BlendShapes = GetComponent<VRMBlendShapeProxy>();
        }

        Coroutine m_coroutine;

        [SerializeField]
        float m_wait = 0.5f;

        IEnumerator RoutineNest(BlendShapePreset preset, float velocity, float wait)
        {
            if (BlendShapes == null)
                yield break;

            for (var value = 0.0f; value <= 1.0f; value += velocity)
            {
                BlendShapes.SetValue(preset, value);
                yield return null;
            }
            BlendShapes.SetValue(preset, 1.0f);
            yield return new WaitForSeconds(wait);
            for (var value = 1.0f; value >= 0; value -= velocity)
            {
                BlendShapes.SetValue(preset, value);
                yield return null;
            }
            BlendShapes.SetValue(preset, 0);
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
