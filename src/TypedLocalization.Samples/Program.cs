using TypedLocalization.Samples.Localization;

namespace TypedLocalization.Samples
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            ISampleLocalizer localizer = null;
            var a = localizer.Test1;
            var b = localizer.GetTest2(2, "asd");
            var c = localizer.GetTest3("asd");
        }
    }
}
