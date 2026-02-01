using UnityEngine;
using Mirror;

namespace MaskEffect
{
    /// <summary>
    /// Networked tile component. SyncVars ensure all clients see the same
    /// grid layout, zone coloring, and any future per-tile properties.
    /// </summary>
    public class TileController : NetworkBehaviour
    {
        [SyncVar]
        public int tileIndex;

        [SyncVar]
        public TileZone zone;

        [SyncVar(hook = nameof(OnColorChanged))]
        public Color tileColor = Color.gray;

        public override void OnStartClient()
        {
            base.OnStartClient();
            ApplyColor();
            RegisterWithGrid();
        }

        private void OnColorChanged(Color oldColor, Color newColor)
        {
            ApplyColor();
        }

        private void ApplyColor()
        {
            var rend = GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = tileColor;
        }

        private void RegisterWithGrid()
        {
            var grid = FindFirstObjectByType<SimpleFlatGrid>();
            if (grid != null)
                grid.RegisterTileVisual(tileIndex, gameObject);
        }
    }
}
