using System.Collections;
using UnityEngine;

public class RuntimeHost : MonoBehaviour
{
    public Coroutine RunCoroutine(IEnumerator routine)
    {
        return StartCoroutine(routine);
    }
}