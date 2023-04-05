using DebugMod.Canvas;
using UnityEngine;

namespace DebugMod
{
    public struct EnemyData
    {
        public int HP;
        public int maxHP;
        // public PlayMakerFSM FSM;
        public HealthManager HM;
        public tk2dSprite Spr;
        public CanvasPanel hpBar;
        public CanvasPanel hitbox;
        public GameObject gameObject;
        public BoxCollider2D boxCollider2D;

        public EnemyData(int hp, HealthManager hm, tk2dSprite spr, GameObject parent, GameObject go)
        {
            HP = hp;
            maxHP = hp;
            HM = hm;
            Spr = spr;
            gameObject = go;
            boxCollider2D = Spr?.boxCollider2D ?? gameObject?.GetComponent<BoxCollider2D>();

            Texture2D tex = new Texture2D(120, 40);

            for (int x = 0; x < 120; x++)
            {
                for (int y = 0; y < 40; y++)
                {
                    if (x < 3 || x > 116 || y < 3 || y > 36)
                    {
                        tex.SetPixel(x, y, Color.black);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.red);
                    }
                }
            }

            tex.Apply();

            Texture2D tex2 = new Texture2D(1, 1);
            Color yellow = Color.yellow;
            yellow.a = .7f;
            tex2.SetPixel(1, 1, yellow);

            hpBar = new CanvasPanel(parent, tex, Vector2.zero, Vector2.zero, new Rect(0, 0, 120, 40));
            hpBar.AddText("HP", "", Vector2.zero, new Vector2(120, 40), GUIController.Instance.arial, 20, FontStyle.Normal, TextAnchor.MiddleCenter);
            hpBar.FixRenderOrder();

            hitbox = new CanvasPanel(parent, tex2, Vector2.zero, Vector2.zero, new Rect(0, 0, 1, 1));

            hpBar.SetActive(false, true);
            hitbox.SetActive(false, true);
        }

        public void SetHP(int health)
        {
            HP = health;
            HM.hp = health;
        }

        // Use tk2d bounds if available, otherwise the collider, otherwise a small box of size (1, 1)
        public Bounds GetBounds() => Spr?.GetBounds() ?? boxCollider2D?.bounds ?? new Bounds(gameObject.transform.position, new(1, 1, 0));
    }
}
