﻿using UnityEngine;

namespace HairLetLoose
{
    public static class Log
    {
        public static void Error(string message, string name = nameof(Script))
        {
            SuperController.LogError($"{nameof(HairLetLoose)}.{name}: {message}");
        }

        public static void Message(string message, string name = nameof(Script))
        {
            SuperController.LogMessage($"{nameof(HairLetLoose)}.{name}: {message}");
        }

        public static string NameValueString(
            string name,
            float value,
            float roundFactor = 1000f,
            int padRight = 0
        )
        {
            float rounded = Calc.RoundToDecimals(value, roundFactor);
            string printName = name.PadRight(padRight, ' ');
            return string.Format("{0} {1}", printName, $"{rounded}");
        }
    }
}
