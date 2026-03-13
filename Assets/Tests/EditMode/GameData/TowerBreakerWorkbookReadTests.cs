using System.IO;
using System.Linq;

using NUnit.Framework;

using TowerBreak.GameData.Editor;

using UnityEngine;

namespace TowerBreak.GameData.Tests
{
    public sealed class TowerBreakerWorkbookReadTests
    {
        [Test]
        public void ExcelWorkbookReader_ReadsTowerBreakerMvpWorkbook()
        {
            string workbookPath = Path.Combine(Application.dataPath, "Data", "Design", "TowerBreaker-MVP.xlsx");
            ExcelWorkbookReader reader = new();

            WorkbookData workbook = reader.Read(workbookPath);

            Assert.That(workbook.SourcePath, Is.EqualTo(workbookPath));
            Assert.That(workbook.Sheets.Count, Is.EqualTo(8));

            SheetData floors = workbook.Sheets.First(sheet => sheet.Name == "Floors");
            Assert.That(floors.Headers, Is.EqualTo(new[]
            {
                "Id",
                "DisplayName",
                "RewardTableId",
                "RecommendedPower",
                "BattleBackdropKey",
                "BattleBgmKey"
            }));
            Assert.That(floors.Rows.Count, Is.EqualTo(3));
            Assert.That(floors.Rows[0][1].DisplayText, Is.EqualTo("Floor 1"));

            SheetData enemies = workbook.Sheets.First(sheet => sheet.Name == "Enemies");
            Assert.That(enemies.Rows.Count, Is.EqualTo(2));
            Assert.That(enemies.Rows[1][1].DisplayText, Is.EqualTo("ArmoredPusher"));

            SheetData rewardEntries = workbook.Sheets.First(sheet => sheet.Name == "RewardEntries");
            Assert.That(rewardEntries.Rows.Count, Is.EqualTo(6));
            Assert.That(rewardEntries.Rows[0][2].DisplayText, Is.EqualTo("Weapon"));
        }
    }
}
