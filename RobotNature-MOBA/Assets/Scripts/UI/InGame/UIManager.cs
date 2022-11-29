using UnityEngine;

namespace UI.InGame
{
    public partial class UIManager : MonoBehaviour
    {
        public static UI.InGame.UIManager Instance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }

            Instance = this;
        }
    }
}