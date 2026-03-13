using UnityEditor;
using UnityEngine;

namespace TowerBreak.GameData.Editor
{
    public static class ExcelImportMenu
    {
        [MenuItem("Tools/GameData/Show Excel Dependency Folder")]
        private static void ShowExcelDependencyFolder()
        {
            EditorUtility.RevealInFinder(ExcelDependencyInfo.DependencyRootFolder);
            Debug.Log($"Excel dependency folder: {ExcelDependencyInfo.DependencyRootFolder}");
        }
    }
}
