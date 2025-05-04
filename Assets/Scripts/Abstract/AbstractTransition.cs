using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class AbstractTransition : MonoBehaviour
{
    public CanvasGroup canvas;
    
    protected IEnumerator Fade(float target, float duration)
    {
        var start = canvas.alpha;
        var timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            var time = timer / duration;
            canvas.alpha = Mathf.Lerp(start, target, time);
            
            yield return null;
        }

        canvas.alpha = target;
    }

    public virtual IEnumerator FadeIn(float duration)
    {
        gameObject.SetActive(true);
        yield return Fade(1, duration);
    }
    
    public virtual IEnumerator FadeOut(float duration)
    {
        yield return Fade(0, duration);
        gameObject.SetActive(false);
    }

    public virtual async Task Initialize(object data = default)
    {
        await Task.Yield();
    }
}