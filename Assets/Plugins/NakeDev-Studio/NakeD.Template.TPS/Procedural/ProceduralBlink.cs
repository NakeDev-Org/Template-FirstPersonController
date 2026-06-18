using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class ProceduralBlink : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField]
    [Tooltip(
        "O SkinnedMeshRenderer que contém as Blendshapes dos olhos (geralmente a malha do corpo do Rex)"
    )]
    private SkinnedMeshRenderer bodyRenderer;

    [Header("Configurações das Blendshapes")]
    [SerializeField]
    [Tooltip("Nome exato da blendshape do olho esquerdo no modelo (ex: Blink_L ou vrc.blink_left)")]
    private string leftEyeBlinkName = "Blink_L";

    [SerializeField]
    [Tooltip("Nome exato da blendshape do olho direito no modelo (ex: Blink_R ou vrc.blink_right)")]
    private string rightEyeBlinkName = "Blink_R";

    [Header("Configurações de Tempo")]
    [SerializeField]
    [Range(0.5f, 15f)]
    [Tooltip("Tempo mínimo entre as piscadas")]
    private float minBlinkInterval = 2f;

    [SerializeField]
    [Range(0.5f, 15f)]
    [Tooltip("Tempo máximo entre as piscadas")]
    private float maxBlinkInterval = 6f;

    [SerializeField]
    [Range(0.01f, 0.5f)]
    [Tooltip("Duração do piscar em segundos (fechar e abrir)")]
    private float blinkSpeed = 0.08f;

    private int leftBlinkIndex = -1;
    private int rightBlinkIndex = -1;

    void Start()
    {
        // Se não foi atribuído no Inspetor, tenta buscar nos filhos
        if (bodyRenderer == null)
        {
            bodyRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        }

        if (bodyRenderer != null)
        {
            // Busca o índice das Blendshapes correspondentes pelo nome
            leftBlinkIndex = bodyRenderer.sharedMesh.GetBlendShapeIndex(leftEyeBlinkName);
            rightBlinkIndex = bodyRenderer.sharedMesh.GetBlendShapeIndex(rightEyeBlinkName);

            // Inicia o loop de piscadas caso encontre os índices
            if (leftBlinkIndex != -1 && rightBlinkIndex != -1)
            {
                StartCoroutine(BlinkRoutine());
            }
            else
            {
                Debug.LogWarning(
                    $"[ProceduralBlink] Não foi possível encontrar as Blendshapes com os nomes '{leftEyeBlinkName}' ou '{rightEyeBlinkName}'. Verifique os nomes das Blendshapes no seu modelo 3D."
                );
            }
        }
        else
        {
            Debug.LogError(
                "[ProceduralBlink] Nenhum SkinnedMeshRenderer encontrado. Certifique-se de atribuir o componente correto!"
            );
        }
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            // Aguarda um intervalo de tempo aleatório
            float waitTime = Random.Range(minBlinkInterval, maxBlinkInterval);
            yield return new WaitForSeconds(waitTime);

            // Suaviza o fechamento dos olhos
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / blinkSpeed;
                float weight = Mathf.Lerp(0f, 100f, t);
                SetBlinkWeight(weight);
                yield return null;
            }

            // Suaviza a abertura dos olhos
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / blinkSpeed;
                float weight = Mathf.Lerp(100f, 0f, t);
                SetBlinkWeight(weight);
                yield return null;
            }
        }
    }

    private void SetBlinkWeight(float weight)
    {
        bodyRenderer.SetBlendShapeWeight(leftBlinkIndex, weight);
        bodyRenderer.SetBlendShapeWeight(rightBlinkIndex, weight);
    }
}
