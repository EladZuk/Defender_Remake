using UnityEngine;
using TMPro;
using DefenderRemake.Systems;

namespace DefenderRemake.UI
{
    public class MapWarningUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("The TextMeshPro text that says WARNING: LEAVING COMBAT ZONE")]
        private TextMeshProUGUI warningText;
        [SerializeField, Tooltip("Reference to the map bounds to check distance")]
        private MapBounds3D mapBounds;

        [Header("Settings")]
        [SerializeField, Tooltip("Toggle the entire warning system on or off")]
        private bool enableWarning = true;
        [SerializeField, Tooltip("Distance at which the warning appears")]
        private float warningDistance = 40f;
        [SerializeField, Tooltip("Distance at which the text turns red and flashes")]
        private float dangerDistance = 15f;

        [Header("Appearance")]
        [SerializeField, Tooltip("Color of the text when in the warning zone")]
        private Color warningColor = Color.yellow;
        [SerializeField, Tooltip("Color of the text when in the danger zone")]
        private Color dangerColor = Color.red;
        [SerializeField, Tooltip("How fast the text flashes in the danger zone")]
        private float flashSpeed = 5f;

        private void Start()
        {
            if (warningText != null)
            {
                warningText.alpha = 0f;
            }
        }

        private void Update()
        {
            if (mapBounds == null || warningText == null) return;

            if (!enableWarning)
            {
                warningText.alpha = 0f;
                return;
            }

            float dist = mapBounds.DistanceToNearestWall();

            if (dist > warningDistance)
            {
                // Safe zone, hide warning
                warningText.alpha = Mathf.MoveTowards(warningText.alpha, 0f, Time.deltaTime * 2f);
            }
            else if (dist <= warningDistance && dist > dangerDistance)
            {
                // Warning zone, solid color text
                warningText.alpha = Mathf.MoveTowards(warningText.alpha, 1f, Time.deltaTime * 3f);
                warningText.color = warningColor;
            }
            else if (dist <= dangerDistance)
            {
                // Danger zone, flashing text!
                warningText.alpha = 1f;
                float flash = Mathf.PingPong(Time.time * flashSpeed, 1f);
                
                // Flash between solid danger color and transparent danger color
                Color transparentDanger = dangerColor;
                transparentDanger.a = 0.2f;
                warningText.color = Color.Lerp(dangerColor, transparentDanger, flash);
            }
        }
    }
}
