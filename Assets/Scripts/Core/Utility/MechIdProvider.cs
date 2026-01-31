namespace MaskEffect
{
    public static class MechIdProvider
    {
        private static int nextId = 0;

        public static int GetNextId() => nextId++;

        public static void Reset() => nextId = 0;
    }
}