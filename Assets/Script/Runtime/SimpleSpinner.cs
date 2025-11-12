using UnityEngine;

namespace IyagiAI.Runtime
{
    /// <summary>
    /// 로딩 팝업용 간단한 회전 애니메이션
    /// </summary>
    public class SimpleSpinner : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 180f; // 초당 회전 각도

        void Update()
        {
            transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }
    }
}
