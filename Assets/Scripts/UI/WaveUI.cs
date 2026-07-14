using UnityEngine;
using TMPro;
using System.Collections;

namespace DefenderRemake.UI
{
    /// <summary>
    /// Displays giant retro text across the screen when a new wave starts.
    /// Attach this to a UI Canvas and assign a TextMeshProUGUI element.
    /// </summary>
    public class WaveUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private TextMeshProUGUI bossText;
        [SerializeField] private float displayDuration = 2.5f;

        private void OnEnable()
        {
            DefenderRemake.Systems.WaveManager.OnWaveAnnounced += DisplayWaveText;
            
            if (waveText != null) waveText.enabled = false;
            if (subtitleText != null) subtitleText.enabled = false;
            if (bossText != null) bossText.enabled = false;
        }

        private void OnDisable()
        {
            DefenderRemake.Systems.WaveManager.OnWaveAnnounced -= DisplayWaveText;
        }

        private void DisplayWaveText(string mainText, string subText, bool isBoss)
        {
            StopAllCoroutines();
            StartCoroutine(ShowTextRoutine(mainText, subText, isBoss));
        }

        private IEnumerator ShowTextRoutine(string mainText, string subText, bool isBoss)
        {
            if (isBoss)
            {
                if (bossText != null)
                {
                    bossText.text = mainText;
                    bossText.enabled = true;
                }
            }
            else
            {
                if (waveText != null)
                {
                    waveText.text = mainText;
                    waveText.enabled = true;
                }

                if (subtitleText != null && !string.IsNullOrEmpty(subText))
                {
                    subtitleText.text = subText;
                    subtitleText.enabled = true;
                }
            }
            
            yield return new WaitForSeconds(displayDuration);
            
            if (waveText != null) waveText.enabled = false;
            if (subtitleText != null) subtitleText.enabled = false;
            if (bossText != null) bossText.enabled = false;
        }
    }
}
