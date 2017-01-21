﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace Gamestrap
{
    public enum SchemeType { Analogous, Triadic, Complement, SplitComplement, Monochromatic }

    /// <summary>
    /// Methods that help Gamestrap UI such as color conversion, generation and getting Unity's gameobjects in the scene.
    /// </summary>
    public class GamestrapUIHelper
    {
        private static string gamestrapRoute = "";

        public static string GamestrapRoute
        {
            get
            {
                if (gamestrapRoute.Length == 0)
                {
                    string[] assets = AssetDatabase.FindAssets("t:script GamestrapUI");
                    if (assets.Length == 0)
                    {
                        Debug.LogError("GamestrapUI name not found, make sure you have the Gamestrap scripts in your project.");
                        return null;
                    }

                    string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                    DirectoryInfo dir = Directory.GetParent(path);
                    gamestrapRoute = "Assets" + dir.Parent.FullName.Substring(Application.dataPath.Length) + "\\";
                }
                return gamestrapRoute;
            }
        }

        #region Color conversions
        public static Color ColorRGBInt(int r, int g, int b)
        {
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        public static Vector3 RGBtoHSV(Color rgbColor)
        {
            float h, s, v;
#if UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
        EditorGUIUtility.RGBToHSV(rgbColor,out h, out s, out v);
#else
            Color.RGBToHSV(rgbColor, out h, out s, out v);
#endif
            return new Vector3(h * 360, s * 255, v * 255);
        }

        public static Color HSVtoRGB(Vector3 hsvColor)
        {
#if UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
        return EditorGUIUtility.HSVToRGB(hsvColor.x/360f, hsvColor.y/255f, hsvColor.z/255f);
#else
            return Color.HSVToRGB(hsvColor.x / 360f, hsvColor.y / 255f, hsvColor.z / 255f);
#endif

        }
        #endregion

        #region Palette Color Generator Methods
        public static Color[] GetColorPalette(Color mainColor, SchemeType type)
        {
            switch (type)
            {
                case SchemeType.Monochromatic:
                    return GetColorPaletteMonochromatic(mainColor);
                case SchemeType.Analogous:
                    return GetColorPaletteAnalogous(mainColor, 30f, 4);
                case SchemeType.Triadic:
                    return GetColorPaletteAngleDiff(mainColor, 120f);
                case SchemeType.SplitComplement:
                    return GetColorPaletteAngleDiff(mainColor, 150f);
                case SchemeType.Complement:
                    return GetColorPaletteComplement(mainColor);
            }
            return null;
        }

        public static Color[] GetColorPaletteMonochromatic(Color mainColor)
        {
            Vector3 hsv = RGBtoHSV(mainColor);
            hsv.z = 0;
            Color[] palette = new Color[5];
            for (int i = 0; i < palette.Length; i++)
            {
                hsv.z += 51;
                palette[i] = HSVtoRGB(hsv);
            }
            return palette;
        }

        public static Color[] GetColorPaletteAnalogous(Color mainColor, float angle, int number)
        {
            Vector3 hsv = RGBtoHSV(mainColor);
            float originalHue = Mathf.RoundToInt(hsv.x);
            Color[] palette = new Color[number];

            hsv.x -= angle * ((number / 2) + 1);
            while (hsv.x < 0f)
            {
                hsv.x = 360f + hsv.x;
            }
            int index = 0;
            for (int i = 0; i < number + 1; i++)
            {
                hsv.x = (hsv.x + angle) % 360f;
                if (Mathf.RoundToInt(hsv.x) == originalHue)
                {
                    continue;
                }
                if (index < palette.Length)
                {
                    palette[index] = HSVtoRGB(hsv);
                    index++;
                }
            }

            return palette;
        }

        public static Color[] GetColorPaletteAngleDiff(Color mainColor, float angle)
        {
            Vector3 hsv = RGBtoHSV(mainColor);
            Color[] palette = new Color[2];

            Vector3 left = hsv + new Vector3(-angle, 0, 0);
            if (left.x < 0)
            {
                left.x = 360 + left.x;
            }
            palette[0] = HSVtoRGB(left);

            Vector3 right = hsv + new Vector3(angle, 0, 0);
            right.x = right.x % 360;
            palette[1] = HSVtoRGB(right);

            return palette;
        }

        public static Color[] GetColorPaletteComplement(Color mainColor)
        {
            Vector3 hsv = RGBtoHSV(mainColor);
            Color[] palette = new Color[1];

            hsv.x = (hsv.x + 180f) % 360f;
            palette[0] = HSVtoRGB(hsv);

            return palette;
        }
        #endregion

        #region Unity Useful Methods

        public static IEnumerable<GameObject> GetSceneGameObjectRoots()
        {
            var prop = new HierarchyProperty(HierarchyType.GameObjects);
            var expanded = new int[0];
            while (prop.Next(expanded))
            {
                yield return prop.pptrValue as GameObject;
            }
        }

        #endregion
    }
}