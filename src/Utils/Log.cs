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
    }
}
