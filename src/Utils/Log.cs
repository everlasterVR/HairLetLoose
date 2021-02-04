namespace HairLetLoose
{
    public static class Log
    {
        public static void Error(string message, string name = nameof(Main))
        {
            SuperController.LogError($"{nameof(HairLetLoose)}.{name}: {message}");
        }

        public static void Message(string message, string name = nameof(Main))
        {
            SuperController.LogMessage($"{nameof(HairLetLoose)}.{name}: {message}");
        }

        public static string BaseDebugInfo(float roll, float pitch, float tipRigidity, float mainRigidity, float baseVal)
        {
            return
                $"{NameValueString("Roll", roll, 100f, 10)}\n" +
                $"{NameValueString("Pitch", pitch, 100f, 10)}\n" +
                $"{NameValueString("Base val", baseVal, 1000f, 10)}\n" +
                $"{NameValueString("Tip rigidity", tipRigidity, 1000f, 22)}\n" +
                $"{NameValueString("Main rigidity", mainRigidity, 1000f, 20)}";
        }

        private static string NameValueString(
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
