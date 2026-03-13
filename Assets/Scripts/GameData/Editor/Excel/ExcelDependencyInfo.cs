namespace TowerBreak.GameData.Editor
{
    public static class ExcelDependencyInfo
    {
        public const string DependencyRootFolder = "Assets/Plugins/Editor/ExcelDataReader";

        public static readonly string[] RequiredDllNames =
        {
            "ExcelDataReader.dll",
            "ExcelDataReader.DataSet.dll",
            "System.Text.Encoding.CodePages.dll"
        };
    }
}
