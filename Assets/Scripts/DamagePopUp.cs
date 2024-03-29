using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// This script handles the text and colour of pop-ups, and destroys it after a second.
public class DamagePopUp : MonoBehaviour
{
    [SerializeField] Text popUp;
    [SerializeField] Outline outline;
    private Color playerColor;
    private Color enemyColor;

    private void OnEnable()
    {
        StartCoroutine(DestroyAfterSecond());
    }

    /* Displays the textToDisplay, with either the player colour or the enemy colour,
     * depending on whether this pop-up was caused by the player or the enemy. */
    public void DisplayText(string textToDisplay, bool byPlayer)
    {
        playerColor = new Color(1, 0, 76 / 255f);
        enemyColor = new Color(229 / 255f, 134 / 255f, 0);
        if (byPlayer)
        {
            outline.effectColor = playerColor;
        } else { outline.effectColor = enemyColor; }
        popUp.text = textToDisplay;
    }

    // Destroys the pop-up after a second.
    IEnumerator DestroyAfterSecond()
    {
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
}
