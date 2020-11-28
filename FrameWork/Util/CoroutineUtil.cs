using UnityEngine;
using System.Collections;

public static class CoroutineUtil
{
    public static Coroutine WaitForSeconds(MonoBehaviour mb, float waitTime, System.Action cb)
    {
        if (waitTime == 0)
        {
            cb();
            return null;
        }
        else
            return mb.StartCoroutine(_WaitForSeconds(waitTime, cb));
    }
    public static IEnumerator _WaitForSeconds(float waitTime, System.Action cb)
    {
        yield return new WaitForSeconds(waitTime);
        cb();
    }

    public static Coroutine WaitForEndOfFrame(MonoBehaviour mb, System.Action cb)
    {
        return mb.StartCoroutine(_WaitForEndOfFrame(cb));
    }

    public static IEnumerator _WaitForEndOfFrame(System.Action cb)
    {
        yield return waitforendOfFrame;
        cb();
    }

    public static readonly WaitForEndOfFrame waitforendOfFrame = new WaitForEndOfFrame();
}
