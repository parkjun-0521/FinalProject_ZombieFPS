using UnityEngine;

public class MatchBoundsSize : MonoBehaviour
{
    public GameObject targetObject; // 크기를 맞추고자 하는 대상 오브젝트

    void Start()
    {
        if (targetObject != null)
        {
            // 대상 오브젝트의 Bounds 크기를 가져옴
            Renderer targetRenderer = targetObject.GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                Vector3 targetSize = targetRenderer.bounds.size;

                // 현재 오브젝트의 Bounds 크기를 가져옴
                Renderer currentRenderer = GetComponent<Renderer>();
                if (currentRenderer != null)
                {
                    Vector3 currentSize = currentRenderer.bounds.size;

                    // 크기 비율을 계산하여 스케일 조정
                    Vector3 scaleRatio = new Vector3(
                        targetSize.x / currentSize.x,
                        targetSize.y / currentSize.y,
                        targetSize.z / currentSize.z
                    );

                    transform.localScale = Vector3.Scale(transform.localScale, scaleRatio);
                }
                else
                {
                    Debug.LogError("Current object does not have a Renderer component.");
                }
            }
            else
            {
                Debug.LogError("Target object does not have a Renderer component.");
            }
        }
        else
        {
            Debug.LogError("Target object is not set.");
        }
    }
}
