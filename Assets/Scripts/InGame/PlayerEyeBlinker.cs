using System.Collections;
using DG.Tweening;
using UnityEngine;

public class PlayerEyeBlinker : MonoBehaviour
{
    [SerializeField] private Transform eye;
    [SerializeField] private float blinkMin = 0.8f, blinkMax = 2f;
    [SerializeField] private float blinkSpeed = 0.2f;
    [SerializeField, Range(0f, 1f)] private float blinkTwoTimesRate = 0.5f;

    private WaitForSeconds blinkWait;
    private Vector3 eyeOpenScale = new Vector3(0.85f, 0.85f, 0.85f);
    private Vector3 eyeCloseScale = new Vector3(0.85f, 0, 0.85f);

    void Start()
    {
        blinkWait = new WaitForSeconds(blinkSpeed);
        eye.localScale = eyeOpenScale;
        StartCoroutine(EyeBlinkCoroutine());
    }

    private IEnumerator EyeBlinkCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(blinkMin, blinkMax));
            int eyeCount = Random.Range(0f, 1f) < blinkTwoTimesRate ? 2 : 1;
            for (int i = 0; i < eyeCount; i++)
            {
                if (i == 1 && Random.Range(0, 3) == 0)
                    yield return new WaitForSeconds(0.3f);
                eye.DOScale(eyeCloseScale, blinkSpeed);
                yield return blinkWait;
                eye.DOScale(eyeOpenScale, blinkSpeed);
                yield return blinkWait;
            }
        }
    }
}
