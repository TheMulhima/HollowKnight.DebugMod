using System;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMod.Hitbox
{
    class ShadeSpawnLocation :MonoBehaviour
    {
        private Vector3 ClosestSpawn;
        
        private static GameObject go_ClosestSpawn;

        public static bool EnabledCompass = false;

        public static int ShowShadeRetreatBorder = 0;

        private GameObject Compass = null;

        private static List<GameObject> ShadeLocations;

        private void Start()
        {
            ShadeLocations = new List<GameObject>();
            Compass = new GameObject(
                "Compass", 
                typeof(SpriteRenderer));
            DontDestroyOnLoad(Compass);
            
            //right arrow was used because the arctan calculates angle relative to +x so using the right arrow means no extra correction
            Texture2D texture = GUIController.Instance.images["ScrollBarArrowRight"];
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                64);

            SpriteRenderer renderer = Compass.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 32767;
            renderer.enabled = false;
            
            //image is too small and i dont wanna add a new image
            Compass.transform.localScale *= 2;
        }

        public void Destroy()
        {
            UnityEngine.Object.Destroy(Compass);
        }

        private void Update()
        {
            if (EnabledCompass)
            {
                ShadeLocations = FindShadeMarkers();
                go_ClosestSpawn = FindClosest(ShadeLocations);
                ClosestSpawn = go_ClosestSpawn.transform.position;
                
                if (ShadeLocations.Count > 0)
                {
                    Vector3 CompassPos = HeroController.instance.transform.position + new Vector3(0f, 1f, 0);
                    Vector3 Distance = ClosestSpawn - CompassPos;


                    //make sure the compass stays over the player
                    Compass.transform.position = CompassPos;

                    //rotate the image to point to the nearest spawn location
                    //the rotation on z axis is what we need hence we keep x and y 0
                    // the calculation is arctan(y/x) which gives us the angle we need
                    Compass.transform.rotation =
                        Quaternion.Euler(0, 0, Mathf.Atan2(Distance.y, Distance.x) * Mathf.Rad2Deg);
                }
            }

            Compass.GetComponent<SpriteRenderer>().enabled = EnabledCompass && ClosestSpawn != null;
        }

        public void OnGUI()
        {

            if (Event.current?.type != EventType.Repaint || GameManager.instance.isPaused || Camera.main == null)
            {
                return;
            }

            GUI.depth = int.MaxValue;
            Camera camera = Camera.main;
            float lineWidth = HitboxRender.LineWidth;

            if (!EnabledCompass) // dont wanna repeat if needed
            {
                ShadeLocations = FindShadeMarkers();
                go_ClosestSpawn = FindClosest(ShadeLocations);
            }

            //if no shadelocations, no circles/squares required
            if (ShadeLocations.Count < 1) return;

            foreach (var go in ShadeLocations)
            {
                Vector2 halfSize = new Vector2(0.5f, 0.5f) / 2f;
                Vector2 topLeft = new(-halfSize.x, halfSize.y);
                Vector2 topRight = halfSize;
                Vector2 bottomRight = new Vector2(halfSize.x, -halfSize.y * 4f);
                Vector2 bottomLeft = new Vector2(-halfSize.x, -halfSize.y * 4f);
                var points = new List<Vector2>
                {
                    topLeft, topRight, bottomRight, bottomLeft, topLeft
                };

                for (int i = 0; i < points.Count - 1; i++)
                {
                    Vector2 pointA = LocalToScreenPoint(camera, go, points[i]);
                    Vector2 pointB = LocalToScreenPoint(camera, go, points[i + 1]);
                    Drawing.DrawLine(pointA, pointB, Color.magenta, lineWidth, true);
                }
            }

            if (ShowShadeRetreatBorder == 0) return;

            //if ShowShadeRetreatBorder is == 1, draw only closest circle
            // if ShowShadeRetreatBorder == 2, draw all circles

            foreach (var go in ShadeLocations)
            {
                if (ShowShadeRetreatBorder == 1)
                {
                    if (go != go_ClosestSpawn) continue;
                }

                Vector2 center = LocalToScreenPoint(camera, go, Vector2.zero);
                Vector2 right = LocalToScreenPoint(camera, go, new Vector2(0f + 25f, 0f));
                int radius = (int) Math.Round(Vector2.Distance(center, right));
                Drawing.DrawCircle(center, radius, Color.magenta, lineWidth, true, Mathf.Clamp(radius / 16, 4, 32));
            }
        }

        private Vector2 LocalToScreenPoint(Camera camera, GameObject go, Vector2 point)
        {
            Vector2 result = camera.WorldToScreenPoint(go.transform.TransformPoint(point));
            return new Vector2((int) Math.Round(result.x), (int) Math.Round(Screen.height - result.y));
        }

        private List<GameObject> FindShadeMarkers()
        {
            List<GameObject> ShadeMarkers = new List<GameObject>();
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Shade Marker"))
            {
                ShadeMarkers.Add(go);
            }

            return ShadeMarkers;
        }
        
        private GameObject FindClosest(List<GameObject> ShadeMarkers)
        {
            GameObject Closest = null;
            float f = float.PositiveInfinity;
            foreach (GameObject go in ShadeMarkers)
            {
                float sqrMagnitude = (HeroController.instance.transform.position - go.transform.position).sqrMagnitude;
                if ((double) sqrMagnitude < f)
                {
                    f = sqrMagnitude;
                    Closest = go;
                }
            }
            return Closest;
        }
    }
}