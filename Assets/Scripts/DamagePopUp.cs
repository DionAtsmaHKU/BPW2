using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DamagePopUp : MonoBehaviour
{
    [SerializeField] Text popUp;
    // [SerializeField] Transform canvasTransform;

    private void OnEnable()
    {
        StartCoroutine(DestroyAfterSecond());
        // transform.SetParent(canvasTransform, false);
    }

    public void DisplayText(string textToDisplay)
    {
        popUp.text = textToDisplay;
    }

    IEnumerator DestroyAfterSecond()
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
