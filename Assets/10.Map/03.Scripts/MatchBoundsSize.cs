using UnityEngine;

public class MatchBoundsSize : MonoBehaviour
{
    public GameObject targetObject; // ũ�⸦ ���߰��� �ϴ� ��� ������Ʈ

    void Start()
    {
        if (targetObject != null)
        {
            // ��� ������Ʈ�� Bounds ũ�⸦ ������
            Renderer targetRenderer = targetObject.GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                Vector3 targetSize = targetRenderer.bounds.size;

                // ���� ������Ʈ�� Bounds ũ�⸦ ������
                Renderer currentRenderer = GetComponent<Renderer>();
                if (currentRenderer != null)
                {
                    Vector3 currentSize = currentRenderer.bounds.size;

                    // ũ�� ������ ����Ͽ� ������ ����
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
