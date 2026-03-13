import os
import xml.sax.saxutils as saxutils
import zipfile


SHEETS = [
    (
        "Floors",
        ["Id", "DisplayName", "RewardTableId", "RecommendedPower", "BattleBackdropKey", "BattleBgmKey"],
        [
            [1, "Floor 1", 1001, 10, "env/floor01/backdrop", "bgm/floor01"],
            [2, "Floor 2", 1002, 18, "env/floor02/backdrop", "bgm/floor02"],
            [3, "Floor 3", 1003, 28, "env/floor03/backdrop", "bgm/floor03"],
        ],
    ),
    (
        "FloorWaves",
        ["FloorId", "WaveIndex", "EnemyId", "SpawnOrder", "SpawnTime", "Quantity"],
        [
            [1, 1, 101, 1, 0.0, 2],
            [1, 1, 102, 2, 5.0, 1],
            [2, 1, 101, 1, 0.0, 3],
            [2, 2, 102, 2, 7.0, 1],
            [3, 1, 101, 1, 0.0, 3],
            [3, 2, 102, 2, 6.5, 2],
        ],
    ),
    (
        "Enemies",
        ["Id", "Archetype", "Health", "Pressure", "MoveSpeed", "AttackCadence", "IsArmored", "PrefabKey", "PortraitKey", "HitVfxKey", "HitSfxKey", "DeathVfxKey"],
        [
            [101, "BasicMelee", 30, 4.5, 1.6, 1.2, False, "enemy/basic_melee", "portrait/enemy/basic_melee", "vfx/enemy/hit/basic", "sfx/enemy/hit/basic", "vfx/enemy/death/basic"],
            [102, "ArmoredPusher", 65, 8.0, 1.0, 2.0, True, "enemy/armored_pusher", "portrait/enemy/armored_pusher", "vfx/enemy/hit/armored", "sfx/enemy/hit/armored", "vfx/enemy/death/armored"],
        ],
    ),
    (
        "Weapons",
        ["Id", "Archetype", "Rarity", "BaseAttack", "AttackSpeed", "PushPower", "RerollGroupId", "IconKey", "AttackVfxKey", "HitSfxKey", "EquipSfxKey"],
        [
            [201, "Claw", "Common", 14, 1.35, 3.0, 1, "icon/weapon/claw/common", "vfx/weapon/claw/attack", "sfx/weapon/claw/hit", "sfx/weapon/claw/equip"],
            [202, "Lance", "Common", 18, 0.95, 5.5, 1, "icon/weapon/lance/common", "vfx/weapon/lance/attack", "sfx/weapon/lance/hit", "sfx/weapon/lance/equip"],
        ],
    ),
    (
        "RewardTables",
        ["Id", "GuaranteedGold", "WeaponDropChance", "FallbackRewardId", "RewardPopupSfxKey"],
        [
            [1001, 25, 0.55, 5001, "sfx/reward/floor01"],
            [1002, 35, 0.65, 5002, "sfx/reward/floor02"],
            [1003, 50, 0.75, 5003, "sfx/reward/floor03"],
        ],
    ),
    (
        "RewardEntries",
        ["RewardTableId", "RewardId", "RewardType", "TargetItemId", "Weight", "QuantityMin", "QuantityMax", "IconKey"],
        [
            [1001, 5001, "Weapon", 201, 70, 1, 1, "icon/reward/claw"],
            [1001, 5002, "Weapon", 202, 30, 1, 1, "icon/reward/lance"],
            [1002, 5003, "Weapon", 201, 45, 1, 1, "icon/reward/claw"],
            [1002, 5004, "Weapon", 202, 55, 1, 1, "icon/reward/lance"],
            [1003, 5005, "Weapon", 201, 35, 1, 1, "icon/reward/claw"],
            [1003, 5006, "Weapon", 202, 65, 1, 1, "icon/reward/lance"],
        ],
    ),
    (
        "EnhancementCosts",
        ["Level", "GoldCost", "MaterialCost", "AttackBonus", "PressureBonus"],
        [
            [1, 25, 1, 2, 0.25],
            [2, 45, 2, 4, 0.5],
            [3, 70, 3, 6, 0.75],
        ],
    ),
    (
        "RerollCosts",
        ["Rarity", "GoldCost", "RollCount", "MinBonus", "MaxBonus"],
        [
            ["Common", 20, 2, 1, 3],
            ["Rare", 35, 2, 2, 5],
            ["Epic", 55, 3, 4, 8],
            ["Legendary", 80, 3, 6, 10],
        ],
    ),
]


def column_name(index: int) -> str:
    result = ""
    value = index
    while value > 0:
        value, remainder = divmod(value - 1, 26)
        result = chr(65 + remainder) + result
    return result


def cell_xml(cell_ref: str, value) -> str:
    if isinstance(value, bool):
        return f'<c r="{cell_ref}" t="b"><v>{1 if value else 0}</v></c>'

    if isinstance(value, (int, float)):
        return f'<c r="{cell_ref}"><v>{value}</v></c>'

    escaped = saxutils.escape(str(value))
    return f'<c r="{cell_ref}" t="inlineStr"><is><t>{escaped}</t></is></c>'


def worksheet_xml(headers, rows):
    row_xml = []

    header_cells = []
    for index, header in enumerate(headers, start=1):
        cell_ref = f"{column_name(index)}1"
        header_cells.append(cell_xml(cell_ref, header))
    row_xml.append(f'<row r="1">{"".join(header_cells)}</row>')

    for row_index, row in enumerate(rows, start=2):
        cells = []
        for cell_index, value in enumerate(row, start=1):
            cell_ref = f"{column_name(cell_index)}{row_index}"
            cells.append(cell_xml(cell_ref, value))
        row_xml.append(f'<row r="{row_index}">{"".join(cells)}</row>')

    return (
        '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
        '<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">'
        '<sheetData>'
        f'{"".join(row_xml)}'
        '</sheetData>'
        '</worksheet>'
    )


def workbook_xml():
    sheets_xml = []
    for index, (name, _, _) in enumerate(SHEETS, start=1):
        escaped = saxutils.escape(name)
        sheets_xml.append(
            f'<sheet name="{escaped}" sheetId="{index}" r:id="rId{index}"/>'
        )
    return (
        '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
        '<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" '
        'xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">'
        f'<sheets>{"".join(sheets_xml)}</sheets>'
        '</workbook>'
    )


def workbook_rels_xml():
    relationships = []
    for index, _ in enumerate(SHEETS, start=1):
        relationships.append(
            '<Relationship '
            f'Id="rId{index}" '
            'Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" '
            f'Target="worksheets/sheet{index}.xml"/>'
        )
    relationships.append(
        '<Relationship Id="rId{}" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>'.format(len(SHEETS) + 1)
    )
    return (
        '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
        '<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">'
        f'{"".join(relationships)}'
        '</Relationships>'
    )


def content_types_xml():
    overrides = [
        '<Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>',
        '<Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>',
        '<Override PartName="/docProps/core.xml" ContentType="application/vnd.openxmlformats-package.core-properties+xml"/>',
        '<Override PartName="/docProps/app.xml" ContentType="application/vnd.openxmlformats-officedocument.extended-properties+xml"/>',
    ]
    for index, _ in enumerate(SHEETS, start=1):
        overrides.append(
            f'<Override PartName="/xl/worksheets/sheet{index}.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>'
        )
    return (
        '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
        '<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">'
        '<Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>'
        '<Default Extension="xml" ContentType="application/xml"/>'
        f'{"".join(overrides)}'
        '</Types>'
    )


def root_rels_xml():
    return (
        '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
        '<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">'
        '<Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>'
        '<Relationship Id="rId2" Type="http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties" Target="docProps/core.xml"/>'
        '<Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties" Target="docProps/app.xml"/>'
        '</Relationships>'
    )


def styles_xml():
    return (
        '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
        '<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">'
        '<fonts count="1"><font><sz val="11"/><name val="Calibri"/></font></fonts>'
        '<fills count="2"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill></fills>'
        '<borders count="1"><border><left/><right/><top/><bottom/><diagonal/></border></borders>'
        '<cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>'
        '<cellXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/></cellXfs>'
        '<cellStyles count="1"><cellStyle name="Normal" xfId="0" builtinId="0"/></cellStyles>'
        '</styleSheet>'
    )


def app_xml():
    titles = ''.join(f'<vt:lpstr>{saxutils.escape(name)}</vt:lpstr>' for name, _, _ in SHEETS)
    return (
        '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
        '<Properties xmlns="http://schemas.openxmlformats.org/officeDocument/2006/extended-properties" '
        'xmlns:vt="http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes">'
        '<Application>OpenCode</Application>'
        f'<HeadingPairs><vt:vector size="2" baseType="variant"><vt:variant><vt:lpstr>Worksheets</vt:lpstr></vt:variant><vt:variant><vt:i4>{len(SHEETS)}</vt:i4></vt:variant></vt:vector></HeadingPairs>'
        f'<TitlesOfParts><vt:vector size="{len(SHEETS)}" baseType="lpstr">{titles}</vt:vector></TitlesOfParts>'
        '</Properties>'
    )


def core_xml():
    return (
        '<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
        '<cp:coreProperties xmlns:cp="http://schemas.openxmlformats.org/package/2006/metadata/core-properties" '
        'xmlns:dc="http://purl.org/dc/elements/1.1/" '
        'xmlns:dcterms="http://purl.org/dc/terms/" '
        'xmlns:dcmitype="http://purl.org/dc/dcmitype/" '
        'xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">'
        '<dc:title>TowerBreaker MVP Workbook</dc:title>'
        '<dc:creator>OpenCode</dc:creator>'
        '</cp:coreProperties>'
    )


def main():
    output_path = os.path.join("Assets", "Data", "Design", "TowerBreaker-MVP.xlsx")
    os.makedirs(os.path.dirname(output_path), exist_ok=True)

    with zipfile.ZipFile(output_path, "w", compression=zipfile.ZIP_DEFLATED) as archive:
        archive.writestr("[Content_Types].xml", content_types_xml())
        archive.writestr("_rels/.rels", root_rels_xml())
        archive.writestr("docProps/app.xml", app_xml())
        archive.writestr("docProps/core.xml", core_xml())
        archive.writestr("xl/workbook.xml", workbook_xml())
        archive.writestr("xl/_rels/workbook.xml.rels", workbook_rels_xml())
        archive.writestr("xl/styles.xml", styles_xml())

        for index, (_, headers, rows) in enumerate(SHEETS, start=1):
            archive.writestr(f"xl/worksheets/sheet{index}.xml", worksheet_xml(headers, rows))

    print(output_path)


if __name__ == "__main__":
    main()
