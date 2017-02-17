﻿// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2016/03/01 12:29
// License Copyright (c) Daniele Giardini

using UnityEngine;

namespace DG.DeExtensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns TRUE if the string is null or empty
        /// </summary>
        /// <param name="trimSpaces">If TRUE (default) and the string contains only spaces, considers it empty</param>
        public static bool IsNullOrEmpty(this string s, bool trimSpaces = true)
        {
            if (s == null) return true;
            return trimSpaces ? s.Trim().Length == 0 : s.Length == 0;
        }

        /// <summary>
        /// Converts a HEX color to a Unity Color and returns it
        /// </summary>
        /// <param name="hex">The HEX color, either with or without the initial # (accepts both regular and short format)</param>
        public static Color HexToColor(this string hex)
        {
            if (hex[0] == '#') hex = hex.Substring(1);
            int len = hex.Length;
            bool isShortFormat = len < 6;
            if (isShortFormat) {
                float r = (HexToInt(hex[0]) + HexToInt(hex[0]) * 16f) / 255f;
                float g = (HexToInt(hex[1]) + HexToInt(hex[1]) * 16f) / 255f;
                float b = (HexToInt(hex[2]) + HexToInt(hex[2]) * 16f) / 255f;
                float a = len == 4 ? (HexToInt(hex[3]) + HexToInt(hex[3]) * 16f) / 255f : 1;
                return new Color(r, g, b, a);
            } else {
                float r = (HexToInt(hex[1]) + HexToInt(hex[0]) * 16f) / 255f;
                float g = (HexToInt(hex[3]) + HexToInt(hex[2]) * 16f) / 255f;
                float b = (HexToInt(hex[5]) + HexToInt(hex[4]) * 16f) / 255f;
                float a = len == 8 ? (HexToInt(hex[7]) + HexToInt(hex[6]) * 16f) / 255f : 1;
                return new Color(r, g, b, a);
            }
        }

        #region Methods

        static int HexToInt(char hexVal)
        {
            return int.Parse(hexVal.ToString(), System.Globalization.NumberStyles.HexNumber);
        }

        #endregion
    }
}