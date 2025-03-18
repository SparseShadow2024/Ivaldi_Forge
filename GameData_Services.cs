using System.Text;
using System.Data.SQLite;
using Google.FlatBuffers;
using Ivaldi.FlatBuffers.Common;
using Ivaldi.FlatBuffers.MemoryLobbyExcel;
using Ivaldi.FlatBuffers.ScenarioCharacterNameExcel;
using Ivaldi.FlatBuffers.CharacterExcel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Ivaldi.FlatBuffers.AnimatorData;

namespace Ivaldi
{
    public class GameData_Services
    {
        public static List<MemoryLobbyExcel_DB> Get_Original_MemoryLobbyExcel(string Detail_Version)
        {
            Log_Services.Print("开始从 ExcelDB.db 的 MemoryLobbyDBSchema 生成 MemoryLobbyExcel 类", info: true);
            string ExcelDB_File_Path = Path.Combine(Program.GameData_Folder_Path, "TableBundles", Detail_Version, "ExcelDB.db");
            string Query = "SELECT * FROM MemoryLobbyDBSchema";
            List<MemoryLobbyExcel_DB> memory_lobby_excel_db_list = new List<MemoryLobbyExcel_DB>();
            using (var connection = new SQLiteConnection($"Data Source={ExcelDB_File_Path}"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(Query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        byte[] bytes = (byte[])reader["Bytes"];
                        ByteBuffer buffer = new ByteBuffer(bytes);
                        MemoryLobbyExcel memory_lobby_excel = MemoryLobbyExcel.GetRootAsMemoryLobbyExcel(buffer);
                        MemoryLobbyExcel_DB memory_lobby_excel_db = new MemoryLobbyExcel_DB();

                        memory_lobby_excel_db.Id = memory_lobby_excel.Id;
                        memory_lobby_excel_db.ProductionStep = memory_lobby_excel.ProductionStep;
                        memory_lobby_excel_db.LocalizeEtcId = memory_lobby_excel.LocalizeEtcId;
                        memory_lobby_excel_db.CharacterId = memory_lobby_excel.CharacterId;
                        memory_lobby_excel_db.PrefabName = memory_lobby_excel.PrefabName;
                        memory_lobby_excel_db.MemoryLobbyCategory = memory_lobby_excel.MemoryLobbyCategory;
                        memory_lobby_excel_db.SlotTextureName = memory_lobby_excel.SlotTextureName;
                        memory_lobby_excel_db.RewardTextureName = memory_lobby_excel.RewardTextureName;
                        memory_lobby_excel_db.BGMId = memory_lobby_excel.BGMId;
                        memory_lobby_excel_db.AudioClipJp = memory_lobby_excel.AudioClipJp;
                        memory_lobby_excel_db.AudioClipKr = memory_lobby_excel.AudioClipKr;

                        memory_lobby_excel_db_list.Add(memory_lobby_excel_db);
                    }
                }
            }
            return memory_lobby_excel_db_list;
        }
        public class MemoryLobbyExcel_DB
        {
            public long Id { get; set; }
            public ProductionStep ProductionStep { get; set; }
            public uint LocalizeEtcId { get; set; }
            public long CharacterId { get; set; }
            public string PrefabName { get; set; } = "";
            public MemoryLobbyCategory MemoryLobbyCategory { get; set; }
            public string SlotTextureName { get; set; } = "";
            public string RewardTextureName { get; set; } = "";
            public long BGMId { get; set; }
            public string AudioClipJp { get; set; } = "";
            public string AudioClipKr { get; set; } = "";
        }
        public static List<ScenarioCharacterNameExcel_DB> Get_Original_ScenarioCharacterNameExcel(string Detail_Version)
        {
            Log_Services.Print("开始从 ExcelDB.db 的 ScenarioCharacterNameDBSchema 生成 ScenarioCharacterNameExcel 类", info: true);
            string ExcelDB_File_Path = File_Services.Get_File_Path("ExcelDB.db", Path.Combine(Program.GameData_Folder_Path, "TableBundles", Detail_Version));
            string Query = "SELECT * FROM ScenarioCharacterNameDBSchema";
            List<ScenarioCharacterNameExcel_DB> scenairo_character_name_excel_db_list = new List<ScenarioCharacterNameExcel_DB>();
            using (var connection = new SQLiteConnection($"Data Source={ExcelDB_File_Path}"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(Query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        byte[] bytes = (byte[])reader["Bytes"];
                        ByteBuffer buffer = new ByteBuffer(bytes);
                        ScenarioCharacterNameExcel scenairo_character_name_excel = ScenarioCharacterNameExcel.GetRootAsScenarioCharacterNameExcel(buffer);
                        ScenarioCharacterNameExcel_DB scenairo_character_name_excel_db = new ScenarioCharacterNameExcel_DB();

                        scenairo_character_name_excel_db.CharacterName = scenairo_character_name_excel.CharacterName;
                        scenairo_character_name_excel_db.ProductionStep = scenairo_character_name_excel.ProductionStep;
                        scenairo_character_name_excel_db.NameKR = scenairo_character_name_excel.NameKR;
                        scenairo_character_name_excel_db.NicknameKR = scenairo_character_name_excel.NicknameKR;
                        scenairo_character_name_excel_db.NameJP = scenairo_character_name_excel.NameJP;
                        scenairo_character_name_excel_db.NicknameJP = scenairo_character_name_excel.NicknameJP;
                        scenairo_character_name_excel_db.Shape = scenairo_character_name_excel.Shape;
                        scenairo_character_name_excel_db.SpinePrefabName = scenairo_character_name_excel.SpinePrefabName;
                        scenairo_character_name_excel_db.SmallPortrait = scenairo_character_name_excel.SmallPortrait;

                        scenairo_character_name_excel_db_list.Add(scenairo_character_name_excel_db);
                    }
                }
            }
            return scenairo_character_name_excel_db_list;
        }
        public class ScenarioCharacterNameExcel_DB
        {
            public uint CharacterName { get; set; }
            public ProductionStep ProductionStep { get; set; }
            public string NameKR { get; set; } = "";
            public string NicknameKR { get; set; } = "";
            public string NameJP { get; set; } = "";
            public string NicknameJP { get; set; } = "";
            public ScenarioCharacterShapes Shape { get; set; }
            public string SpinePrefabName { get; set; } = "";
            public string SmallPortrait { get; set; } = "";
        }
        public static List<CharacterDialogExcel_DB> Get_Original_CharacterDialogExcel(string Detail_Version)
        {
            Log_Services.Print("开始从 ExcelDB.db 的 CharacterDialogDBSchema 生成 CharacterDialogExcel 类", info: true);
            string ExcelDB_File_Path = File_Services.Get_File_Path("ExcelDB.db", Path.Combine(Program.GameData_Folder_Path, "TableBundles", Detail_Version));
            string Query = "SELECT * FROM CharacterDialogDBSchema";
            List<CharacterDialogExcel_DB> character_dialog_excel_db_list = new List<CharacterDialogExcel_DB>();
            using (var connection = new SQLiteConnection($"Data Source={ExcelDB_File_Path}"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(Query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        byte[] bytes = (byte[])reader["Bytes"];
                        ByteBuffer buffer = new ByteBuffer(bytes);
                        CharacterDialogExcel character_dialog_excel = CharacterDialogExcel.GetRootAsCharacterDialogExcel(buffer);
                        CharacterDialogExcel_DB character_dialog_excel_db = new CharacterDialogExcel_DB();

                        character_dialog_excel_db.CharacterId = character_dialog_excel.CharacterId;
                        character_dialog_excel_db.CostumeUniqueId = character_dialog_excel.CostumeUniqueId;
                        character_dialog_excel_db.DisplayOrder = character_dialog_excel.DisplayOrder;
                        character_dialog_excel_db.ProductionStep = character_dialog_excel.ProductionStep;
                        character_dialog_excel_db.DialogCategory = character_dialog_excel.DialogCategory;
                        character_dialog_excel_db.DialogCondition = character_dialog_excel.DialogCondition;
                        character_dialog_excel_db.Anniversary = character_dialog_excel.Anniversary;
                        character_dialog_excel_db.StartDate = character_dialog_excel.StartDate;
                        character_dialog_excel_db.EndDate = character_dialog_excel.EndDate;
                        character_dialog_excel_db.GroupId = character_dialog_excel.GroupId;
                        character_dialog_excel_db.DialogType = character_dialog_excel.DialogType;
                        character_dialog_excel_db.ActionName = character_dialog_excel.ActionName;
                        character_dialog_excel_db.Duration = character_dialog_excel.Duration;
                        character_dialog_excel_db.AnimationName = character_dialog_excel.AnimationName;
                        character_dialog_excel_db.LocalizeKR = character_dialog_excel.LocalizeKR;
                        character_dialog_excel_db.LocalizeJP = character_dialog_excel.LocalizeJP;
                        character_dialog_excel_db.VoiceIdLength = character_dialog_excel.VoiceIdLength;
                        character_dialog_excel_db.ApplyPosition = character_dialog_excel.ApplyPosition;
                        character_dialog_excel_db.PosX = character_dialog_excel.PosX;
                        character_dialog_excel_db.PosY = character_dialog_excel.PosY;
                        character_dialog_excel_db.CollectionVisible = character_dialog_excel.CollectionVisible;
                        character_dialog_excel_db.CVCollectionType = character_dialog_excel.CVCollectionType;
                        character_dialog_excel_db.UnlockFavorRank = character_dialog_excel.UnlockFavorRank;
                        character_dialog_excel_db.UnlockEquipWeapon = character_dialog_excel.UnlockEquipWeapon;
                        character_dialog_excel_db.LocalizeCVGroup = character_dialog_excel.LocalizeCVGroup;

                        character_dialog_excel_db_list.Add(character_dialog_excel_db);
                    }
                }
            }
            return character_dialog_excel_db_list;
        }
        public class CharacterDialogExcel_DB
        {
            public long CharacterId { get; set; }
            public long CostumeUniqueId { get; set; }
            public long DisplayOrder { get; set; }
            public ProductionStep ProductionStep { get; set; }
            public DialogCategory DialogCategory { get; set; }
            public DialogCondition DialogCondition { get; set; }
            public Anniversary Anniversary { get; set; }
            public string StartDate { get; set; } = "";
            public string EndDate { get; set; } = "";
            public long GroupId { get; set; }
            public DialogType DialogType { get; set; }
            public string ActionName { get; set; } = "";
            public long Duration { get; set; }
            public string AnimationName { get; set; } = "";
            public string LocalizeKR { get; set; } = "";
            public string LocalizeJP { get; set; } = "";
            public int VoiceIdLength { get; set; }
            public bool ApplyPosition { get; set; }
            public float PosX { get; set; }
            public float PosY { get; set; }
            public bool CollectionVisible { get; set; }
            public CVCollectionType CVCollectionType { get; set; }
            public long UnlockFavorRank { get; set; }
            public bool UnlockEquipWeapon { get; set; }
            public string LocalizeCVGroup { get; set; } = "";
        }
        public static List<CharacterExcel_DB> Get_Original_CharacterExcelTable(string Detail_Version)
        {
            Log_Services.Print("开始从 characterexceltable.bytes 生成 CharacterExcel 类", info: true);
            string Bytes_File_Path = File_Services.Get_File_Path("characterexceltable.bytes", Path.Combine(Program.GameData_Folder_Path, "TableBundles", Detail_Version));
            byte[] charavter_excel_table_bytes = Get_Excel_Table_Bytes(Bytes_File_Path, "CharacterExcelTable");
            ByteBuffer byte_buffer = new ByteBuffer(charavter_excel_table_bytes);
            CharacterExcelTable character_excel_table = CharacterExcelTable.GetRootAsCharacterExcelTable(byte_buffer);
            List<CharacterExcel_DB> processed_character_excel_list = new List<CharacterExcel_DB>();

            for (int i = 0; i < character_excel_table.DataListLength; i++)
            {
                CharacterExcel character_excel = (CharacterExcel)character_excel_table.DataList(i);
                CharacterExcel_DB processed_character_excel = new CharacterExcel_DB();

                processed_character_excel.Id = BitConverter.ToInt64(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.Id)));
                processed_character_excel.DevName = Encoding.Unicode.GetString(Process_Excel_Table_Property("Character", Convert.FromBase64String(character_excel.DevName)));
                processed_character_excel.CostumeGroupId = BitConverter.ToInt64(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.CostumeGroupId)));
                processed_character_excel.IsPlayable = character_excel.IsPlayable;
                processed_character_excel.ProductionStep = (ProductionStep)BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes((Int32)character_excel.ProductionStep)));
                processed_character_excel.CollectionVisible = character_excel.CollectionVisible;
                processed_character_excel.ReleaseDate = Encoding.Unicode.GetString(Process_Excel_Table_Property("Character", Convert.FromBase64String(character_excel.ReleaseDate)));
                processed_character_excel.CollectionVisibleStartDate = Encoding.Unicode.GetString(Process_Excel_Table_Property("Character", Convert.FromBase64String(character_excel.CollectionVisibleStartDate)));
                processed_character_excel.CollectionVisibleEndDate = Encoding.Unicode.GetString(Process_Excel_Table_Property("Character", Convert.FromBase64String(character_excel.CollectionVisibleEndDate)));
                processed_character_excel.IsPlayableCharacter = character_excel.IsPlayableCharacter;
                processed_character_excel.LocalizeEtcId = BitConverter.ToUInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.LocalizeEtcId)));
                processed_character_excel.Rarity = (Rarity)BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes((Int32)character_excel.Rarity)));
                processed_character_excel.IsNPC = character_excel.IsNPC;
                processed_character_excel.TacticEntityType = (TacticEntityType)BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes((Int32)character_excel.TacticEntityType)));
                processed_character_excel.CanSurvive = character_excel.CanSurvive;
                processed_character_excel.IsDummy = character_excel.IsDummy;
                processed_character_excel.SubPartsCount = BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.SubPartsCount)));
                processed_character_excel.TacticRole = (TacticRole)BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes((Int32)character_excel.TacticRole)));
                processed_character_excel.WeaponType = (WeaponType)BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes((Int32)character_excel.WeaponType)));
                processed_character_excel.TacticRange = (TacticRange)BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes((Int32)character_excel.TacticRange)));
                processed_character_excel.BulletType = (BulletType)BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes((Int32)character_excel.BulletType)));
                processed_character_excel.ArmorType = (ArmorType)BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes((Int32)character_excel.ArmorType)));
                processed_character_excel.AimIKType = (AimIKType)BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes((Int32)character_excel.AimIKType)));
                processed_character_excel.School = (School)BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes((Int32)character_excel.School)));
                processed_character_excel.Club = (Club)BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes((Int32)character_excel.Club)));
                processed_character_excel.DefaultStarGrade = BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.DefaultStarGrade)));
                processed_character_excel.MaxStarGrade = BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.MaxStarGrade)));
                processed_character_excel.StatLevelUpType = (StatLevelUpType)BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes((Int32)character_excel.StatLevelUpType)));
                processed_character_excel.SquadType = (SquadType)BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes((Int32)character_excel.SquadType)));
                processed_character_excel.Jumpable = character_excel.Jumpable;
                processed_character_excel.PersonalityId = BitConverter.ToInt64(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.PersonalityId)));
                processed_character_excel.CharacterAIId = BitConverter.ToInt64(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.CharacterAIId)));
                processed_character_excel.ExternalBTId = BitConverter.ToInt64(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.ExternalBTId)));
                processed_character_excel.MainCombatStyleId = BitConverter.ToInt64(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.MainCombatStyleId)));
                processed_character_excel.CombatStyleIndex = BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.CombatStyleIndex)));
                processed_character_excel.ScenarioCharacter = Encoding.Unicode.GetString(Process_Excel_Table_Property("Character", Convert.FromBase64String(character_excel.ScenarioCharacter)));
                processed_character_excel.SpawnTemplateId = BitConverter.ToUInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.SpawnTemplateId)));
                processed_character_excel.FavorLevelupType = BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.FavorLevelupType)));
                processed_character_excel.EquipmentSlotLength = BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.EquipmentSlotLength)));
                processed_character_excel.WeaponLocalizeId = BitConverter.ToUInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.WeaponLocalizeId)));
                processed_character_excel.DisplayEnemyInfo = character_excel.DisplayEnemyInfo;
                processed_character_excel.BodyRadius = BitConverter.ToInt64(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.BodyRadius)));
                processed_character_excel.RandomEffectRadius = BitConverter.ToInt64(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.RandomEffectRadius)));
                processed_character_excel.HPBarHide = character_excel.HPBarHide;
                processed_character_excel.HpBarHeight = (float)(BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes((Int32)(Single)character_excel.HpBarHeight))) * 0.00001);
                processed_character_excel.HighlightFloaterHeight = (float)(BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes((Int32)(Single)character_excel.HighlightFloaterHeight))) * 0.00001);
                processed_character_excel.EmojiOffsetX = (float)(BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes((Int32)(Single)character_excel.EmojiOffsetX))) * 0.00001);
                processed_character_excel.EmojiOffsetY = (float)(BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes((Int32)(Single)character_excel.EmojiOffsetY))) * 0.00001);
                processed_character_excel.MoveStartFrame = BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.MoveStartFrame)));
                processed_character_excel.MoveEndFrame = BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.MoveEndFrame)));
                processed_character_excel.JumpMotionFrame = BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.JumpMotionFrame)));
                processed_character_excel.AppearFrame = BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.AppearFrame)));
                processed_character_excel.CanMove = character_excel.CanMove;
                processed_character_excel.CanFix = character_excel.CanFix;
                processed_character_excel.CanCrowdControl = BitConverter.ToBoolean(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.CanCrowdControl)));
                processed_character_excel.CanBattleItemMove = BitConverter.ToBoolean(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.CanBattleItemMove)));
                processed_character_excel.IsAirUnit = BitConverter.ToBoolean(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.IsAirUnit)));
                processed_character_excel.AirUnitHeight = BitConverter.ToInt64(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.AirUnitHeight)));
                processed_character_excel.TagsLength = BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.TagsLength)));
                processed_character_excel.SecretStoneItemId = BitConverter.ToInt64(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.SecretStoneItemId)));
                processed_character_excel.SecretStoneItemAmount = BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.SecretStoneItemAmount)));
                processed_character_excel.CharacterPieceItemId = BitConverter.ToInt64(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.CharacterPieceItemId)));
                processed_character_excel.CharacterPieceItemAmount = BitConverter.ToInt32(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.CharacterPieceItemAmount)));
                processed_character_excel.CombineRecipeId = BitConverter.ToInt64(Process_Excel_Table_Property("Character", BitConverter.GetBytes(character_excel.CombineRecipeId)));
                processed_character_excel_list.Add(processed_character_excel);
            }
            return processed_character_excel_list;
        }
        public static void Write_Processed_CharacterExcelTable_To_Json(string File_Path, List<CharacterExcel_DB> processed_character_excel_list)
        {
            string json = JsonConvert.SerializeObject(processed_character_excel_list, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            Converters = { new StringEnumConverter() } // 枚举转字符串
                        });
            File.WriteAllText(File_Path, json.ToString());
        }
        public class CharacterExcel_DB
        {
            public long Id { get; set; }
            public string DevName { get; set; } = "";
            public long CostumeGroupId { get; set; }
            public bool IsPlayable { get; set; }
            public ProductionStep ProductionStep { get; set; }
            public bool CollectionVisible { get; set; }
            public string ReleaseDate { get; set; } = "";
            public string CollectionVisibleStartDate { get; set; } = "";
            public string CollectionVisibleEndDate { get; set; } = "";
            public bool IsPlayableCharacter { get; set; }
            public uint LocalizeEtcId { get; set; }
            public Rarity Rarity { get; set; }
            public bool IsNPC { get; set; }
            public TacticEntityType TacticEntityType { get; set; }
            public bool CanSurvive { get; set; }
            public bool IsDummy { get; set; }
            public int SubPartsCount { get; set; }
            public TacticRole TacticRole { get; set; }
            public WeaponType WeaponType { get; set; }
            public TacticRange TacticRange { get; set; }
            public BulletType BulletType { get; set; }
            public ArmorType ArmorType { get; set; }
            public AimIKType AimIKType { get; set; }
            public School School { get; set; }
            public Club Club { get; set; }
            public int DefaultStarGrade { get; set; }
            public int MaxStarGrade { get; set; }
            public StatLevelUpType StatLevelUpType { get; set; }
            public SquadType SquadType { get; set; }
            public bool Jumpable { get; set; }
            public long PersonalityId { get; set; }
            public long CharacterAIId { get; set; }
            public long ExternalBTId { get; set; }
            public long MainCombatStyleId { get; set; }
            public int CombatStyleIndex { get; set; }
            public string ScenarioCharacter { get; set; } = "";
            public uint SpawnTemplateId { get; set; }
            public int FavorLevelupType { get; set; }
            public int EquipmentSlotLength { get; set; }
            public uint WeaponLocalizeId { get; set; }
            public bool DisplayEnemyInfo { get; set; }
            public long BodyRadius { get; set; }
            public long RandomEffectRadius { get; set; }
            public bool HPBarHide { get; set; }
            public float HpBarHeight { get; set; }
            public float HighlightFloaterHeight { get; set; }
            public float EmojiOffsetX { get; set; }
            public float EmojiOffsetY { get; set; }
            public int MoveStartFrame { get; set; }
            public int MoveEndFrame { get; set; }
            public int JumpMotionFrame { get; set; }
            public int AppearFrame { get; set; }
            public bool CanMove { get; set; }
            public bool CanFix { get; set; }
            public bool CanCrowdControl { get; set; }
            public bool CanBattleItemMove { get; set; }
            public bool IsAirUnit { get; set; }
            public long AirUnitHeight { get; set; }
            public int TagsLength { get; set; }
            public long SecretStoneItemId { get; set; }
            public int SecretStoneItemAmount { get; set; }
            public long CharacterPieceItemId { get; set; }
            public int CharacterPieceItemAmount { get; set; }
            public long CombineRecipeId { get; set; }
        }
        public static byte[] Get_Excel_Table_Bytes(string File_Path, string FlatBuffers_Name)
        {
            byte[] excel_table_bytes = File.ReadAllBytes(File_Path);
            return Crypto_Services.XOR(excel_table_bytes, Crypto_Services.Generate_Password(FlatBuffers_Name, excel_table_bytes.Length));
        }
        public static byte[] Process_Excel_Table_Property(string Table_Name, byte[] Property_Value)
        {
            byte[] password = Crypto_Services.Generate_Password(Table_Name, 8);
            return Crypto_Services.Advanced_XOR(Property_Value, password);
        }
        public static void Get_Original_AnimatorDataTable(string Detail_Version){
            string Bytes_File_Path = File_Services.Get_File_Path("animatordatatable.bytes", Path.Combine(Program.GameData_Folder_Path, "TableBundles", Detail_Version));
            byte[] charavter_excel_table_bytes = Get_Excel_Table_Bytes(Bytes_File_Path, "AnimatorDataTable");
            ByteBuffer byte_buffer = new ByteBuffer(charavter_excel_table_bytes);
            AnimatorDataTable animator_data_table = AnimatorDataTable.GetRootAsAnimatorDataTable(byte_buffer);
            var result = new
            {
                DataList = Enumerable.Range(0, animator_data_table.DataListLength)
                    .Select(i =>
                    {
                        AnimatorData animator_data = (AnimatorData)animator_data_table.DataList(i);
                        return new
                        {
                            animator_data.Name,
                            animator_data.DefaultStateName,
                            DataList = Enumerable.Range(0, animator_data.DataListLength)
                                .Select(j =>
                                {
                                    AniStateData state_data = (AniStateData)animator_data.DataList(j);
                                    return new
                                    {
                                        state_data.StateName,
                                        state_data.StatePrefix,
                                        state_data.StateNameWithPrefix,
                                        state_data.Tag,
                                        state_data.SpeedParameterName,
                                        state_data.SpeedParamter,
                                        state_data.StateSpeed,
                                        state_data.ClipName,
                                        state_data.Length,
                                        state_data.FrameRate,
                                        state_data.IsLooping,
                                        Events = Enumerable.Range(0, state_data.EventsLength)
                                            .Select(k =>
                                            {
                                                AniEventData event_data = (AniEventData)state_data.Events(k);
                                                return new
                                                {
                                                    event_data.Name,
                                                    event_data.Time,
                                                    event_data.IntParam,
                                                    event_data.FloatParam,
                                                    event_data.StringParam
                                                };
                                            }).ToList()
                                    };
                                }).ToList()
                        };
                    }).ToList()
            };
            string json = JsonConvert.SerializeObject(result, Formatting.Indented);
            File.WriteAllText(@"C:\Users\SparseShadow\Desktop\out.json", json);
        }
    }
}