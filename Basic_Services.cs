using System.Text.RegularExpressions;
using System.Drawing;
using System.Data.SQLite;
using Console = Colorful.Console;
using ICSharpCode.SharpZipLib.Zip;
using AssetsTools.NET.Extra;
using static Ivaldi.GameData_Services;

namespace Ivaldi
{
    public class Log_Services
    {
        public const int Console_Width = 184;
        public const int Console_Height = 50;
        public static int Console_Frame_Per_Width = 4;
        public static int Console_Frame_Per_Height = 2;
        public static void Init()
        {
            Console.BackgroundColor = Color.White;
            Console.SetWindowSize(Console_Width, Console_Height);
            Task.Run(() => Keep_Console_Window_Size());
            Print_Banner();
        }
        public static async Task Keep_Console_Window_Size()
        {
            while (true)
            {
                try { if (Console.WindowWidth != Console_Width || Console.WindowHeight != Console_Height) Console.SetWindowSize(Console_Width, Console_Height); }
                catch (Exception ex) { Print($"[ERROR] 设置窗口大小时错误: {ex.Message}"); }
                await Task.Delay(100);
            }
        }
        public static void Print_Banner()
        {
            string[] Banner = {
            "                                                                                                      @                                                           ",
            "                                                                                                     @                                                            ",
            "                                                                  @@@                               @                                                             ",
            "                                                                     @@@@    @@@@@                 @@                                                             ",
            "                                                                        @@@@@@      @@@@@@@       @@                                                              ",
            "                                                                       @@     @@@@@          @@@@@@                                                               ",
            "                                                                      @@@        @@@@@@        @@@@@@@                                                            ",
            "                                                                       @@            @@@@@@@  @@@     @@@                                                         ",
            "                                                                        @@                @@@@@@          @@                                                      ",
            "                                                                          @@                @@@@@@@@        @@@                                                   ",
            "                                                                            @@@            @@@    @@@@@@      @@@                                                 ",
            "                                                                              @@@         @@@         @@@@@     @@@                                               ",
            "                                                                                  @      @@@               @@@@@ @@@                                              ",
            "       @@@@@                                     @@@@@@             @@@@@   @@@@    @@@ @@@@@@@@@@@@@           @@@@                                              ",
            "      @@@@@@                                     @@@@@             @@@@@   @@@@@   @@@ @@@@@@@@@@@@@@            @@@@                                             ",
            "      @@@@@                                     @@@@@              @@@@@          @@@ @@  @@@@@@@@@@                 @@@                                          ",
            "     @@@@@   @@@@@     @@@@@@ @@@@@@@@@@@@@@   @@@@@@   @@@@@@@@@@@@@@@  @@@@@   @@@ @@              @@@@@@@@@@@@@@@    @@@@@@@@  @@@@@@@@@@@@@@@   @@@@@@@@@@@@@@",
            "    @@@@@@   @@@@@   @@@@@@  @@@@@@@@@@@@@@@   @@@@@  @@@@@@@@@@@@@@@@   @@@@@  @@@ @@ @@@@@@@@@@   @@@@@@@@@@@@@@@@  @@@@@@@@@@ @@@@@@@@@@@@@@@  @@@@@@@@@@@@@@@@",
            "    @@@@@    @@@@@  @@@@@@            @@@@@@  @@@@@  @@@@@@     @@@@@   @@@@@   @@ @@ @@@@@@@@@@   @@@@@@     @@@@@@  @@@@@     @@@@@     @@@@@@ @@@@@      @@@@@ ",
            "   @@@@@    @@@@@  @@@@@    @@@@@@@@@@@@@@@  @@@@@   @@@@@      @@@@@  @@@@@   @@ @@ @@@@@@@@@@    @@@@@      @@@@@  @@@@@     @@@@@      @@@@@  @@@@@@@@@@@@@@@  ",
            "  @@@@@     @@@@@ @@@@@    @@@@@@@@@@@@@@@  @@@@@@  @@@@@      @@@@@  @@@@@@  @@@@@               @@@@@      @@@@@  @@@@@      @@@@@@@@@@@@@@@  @@@@@@@@@@@@@@@   ",
            " @@@@@@     @@@@@@@@@@    @@@@@     @@@@@  @@@@@@  @@@@@      @@@@@   @@@@@  @@@@@               @@@@@      @@@@@@ @@@@@@     @@@@@@@@@@@@@@@  @@@@@              ",
            " @@@@@      @@@@@@@@     @@@@@@@@@@@@@@@   @@@@@   @@@@@@@@@@@@@@@@  @@@@@   @ @@@               @@@@@@@@@@@@@@@@  @@@@@                @@@@@  @@@@@@@@@@@@@@@    ",
            "@@@@@       @@@@@@@      @@@@@@@@@@@@@@@  @@@@@    @@@@@@@@@@@@@@@  @@@@@   @@@@@@               @@@@@@@@@@@@@@   @@@@@      @@@@@@@@@@@@@@@   @@@@@@@@@@@@@@     ",
            "                                                                             @                                              @@@@@@@@@@@@@@@                       ",
            "                                                                            @                                               @@@@@@@@@@@                           ",
            "                                                                           @                                                                                      "
            };
            int Banner_Width = Banner[0].Length;
            int Banner_Height = Banner.Length;
            Color Start_Color = Color.FromArgb(83, 167, 216);
            Color End_Color = Color.FromArgb(1, 92, 146);
            int Cur_Step = 1;
            for (int i = 0; i < Console_Frame_Per_Height; i++) Console.WriteLine();
            for (int i = 0; i < Banner_Height; i++)
            {
                if ((i + 1) % 6 == 0) Cur_Step = i;
                for (int j = 0; j < (((Console_Width - Banner_Width) / 2) > Console_Frame_Per_Width ? ((Console_Width - Banner_Width) / 2) : Console_Frame_Per_Width); j++) Console.Write(" ");
                Console.WriteLine(Banner[i], Get_Gradient_Color(Start_Color, End_Color, Cur_Step, Banner_Height));
            }
            for (int i = 0; i < Console_Frame_Per_Height; i++) Console.WriteLine();
        }
        public static void Print(string message, bool newLine = true, bool enter = true, bool info = false, bool waring = false, bool error = false, bool isDownload = false, int MaxParallelDownloads = 0, int CurTask = 0)
        {
            message = message.Replace(Program.EXE_File_Path + "\\", "");
            if (newLine)
            {
                for (int i = 0; i < Console_Frame_Per_Width; i++) Console.Write(" ");
            }
            if (info) Console.Write("[INFO] ");
            if (waring) Console.Write("[WARNING] ");
            if (error) Console.Write("[ERROR] ");

            if (isDownload)
            {
                Console.WriteLine($"线程{CurTask}: ");
                //Console.SetCursorPosition()
            }

            string pattern = @"#([A-Za-z\*]+)\((.*?)\)";
            var matches = Regex.Matches(message, pattern);
            int lastIndex = 0;
            foreach (Match match in matches)
            {
                Console.Write(message.Substring(lastIndex, match.Index - lastIndex));
                string colorName = match.Groups[1].Value.ToUpper();
                string content = match.Groups[2].Value;
                Color color = Get_Color_By_Name(colorName);
                Console.Write(content, color);
                lastIndex = match.Index + match.Length;
            }
            Console.Write(message.Substring(lastIndex), Color.Black);
            if (enter) Console.WriteLine();
        }
        public static Color Get_Gradient_Color(Color Start_Color, Color End_Color, int Cur_Step, int Total_Steps)
        {
            int r = Start_Color.R + (int)((End_Color.R - Start_Color.R) * (Cur_Step / (float)Total_Steps));
            int g = Start_Color.G + (int)((End_Color.G - Start_Color.G) * (Cur_Step / (float)Total_Steps));
            int b = Start_Color.B + (int)((End_Color.B - Start_Color.B) * (Cur_Step / (float)Total_Steps));
            return Color.FromArgb(r, g, b);
        }
        private static Color Get_Color_By_Name(string colorName)
        {
            switch (colorName)
            {
                case "BLACK": return Color.Black;
                default: return Color.Black;
            }
        }
    }
    public class File_Services
    {
        public static bool Check_File_Path(string File_Path, bool Need_Creat)
        {
            Check_Folder_Path(Path.GetDirectoryName(File_Path) ?? "");
            Log_Services.Print($"[INFO] 开始检查文件 {Path.GetFileName(File_Path)} - ", true, false);
            if (!File.Exists(File_Path))
            {
                try
                {
                    Log_Services.Print("不存在", false, true);
                    if (Need_Creat)
                    {
                        var File_Stream = File.Create(File_Path);
                        Log_Services.Print($"[INFO] 文件 {Path.GetFileName(File_Path)} 已创建");
                        File_Stream.Dispose();
                        return true;
                    }
                    else return false;
                }
                catch (Exception ex)
                {
                    Log_Services.Print($"[ERROR] 检查文件 {Path.GetFileName(File_Path)} 时发生错误: {ex.Message}");
                    return false;
                }
            }
            else
            {
                Log_Services.Print("已存在", false, true);
                return true;
            }
        }
        public static void Check_Folder_Path(string Folder_Path)
        {
            Log_Services.Print($"[INFO] 开始检查路径 {Folder_Path} - ", true, false);
            if (!Directory.Exists(Folder_Path))
            {
                try
                {
                    Log_Services.Print("不存在", false, true);
                    Directory.CreateDirectory(Folder_Path);
                    Log_Services.Print($"[INFO] 路径 {Folder_Path} 已创建");
                }
                catch (Exception ex)
                {
                    Log_Services.Print($"[ERROR] 创建路径 {Folder_Path} 时错误: {ex.Message}");
                    return;
                }
            }
            else
            {
                Log_Services.Print("已存在", false, true);
            }
        }
        public static bool Is_Folder_Empty(string Folder_Path)
        {
            string[] entries = Directory.GetFileSystemEntries(Folder_Path);
            return entries.Length == 0;
        }
        public static void Decompress_Zip_File_With_Password(string Zip_File_Path, string Decompressed_Folder_Path, string Password)
        {
            Log_Services.Print($"[INFO] 开始解压文件 {Zip_File_Path} 到 {Decompressed_Folder_Path}");
            /*
            if (Directory.Exists(Decompressed_Folder_Path) && !Is_Folder_Empty(Decompressed_Folder_Path))
            {
                Log_Services.Print($"[WARNING] 解压目标文件夹 {Decompressed_Folder_Path} 已存在且不为空, 停止解压");
                return;
            }
            */

            using (ICSharpCode.SharpZipLib.Zip.ZipFile zipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(Zip_File_Path))
            {
                zipFile.Password = Password;
                foreach (ZipEntry entry in zipFile)
                {
                    if (!entry.IsFile) continue;

                    string filePath = Path.Combine(Decompressed_Folder_Path, entry.Name);

                    Check_Folder_Path(Decompressed_Folder_Path);

                    using (FileStream fs = File.Create(filePath))
                    using (Stream zipStream = zipFile.GetInputStream(entry)) zipStream.CopyTo(fs);
                }
                Log_Services.Print($"[INFO] 成功使用密码 {Password} 解压了文件 {Path.GetFileName(Zip_File_Path)}");
            }
        }
        public static string Decompress_Zip_File(string Zip_File_Path, string Decompressed_Folder_Path)
        {
            Log_Services.Print($"[INFO] 开始解压文件 {Zip_File_Path} 到 {Decompressed_Folder_Path}");

            if (Directory.Exists(Decompressed_Folder_Path) && !Is_Folder_Empty(Decompressed_Folder_Path))
            {
                Log_Services.Print($"[WARNING] 解压目标文件夹 {Decompressed_Folder_Path} 已存在且不为空, 停止解压");
                return Decompressed_Folder_Path;
            }

            try
            {
                Check_Folder_Path(Decompressed_Folder_Path);
                System.IO.Compression.ZipFile.ExtractToDirectory(Zip_File_Path, Decompressed_Folder_Path);
                Log_Services.Print($"[INFO] 解压文件 {Zip_File_Path} 成功");
                return Decompressed_Folder_Path;
            }
            catch (Exception ex)
            {
                Log_Services.Print($"[ERROR] 解压文件时出错, 错误信息: {ex.Message}");
                return "";
            }
        }
        public static string Get_File_Path(string File_Name, string Folder_Path)
        {
            string file_path = "";
            foreach (var file_paths in Directory.EnumerateFiles(Folder_Path, File_Name, SearchOption.AllDirectories))
            {
                file_path = file_paths;
                continue;
            }
            return file_path;
        }
        public static List<string> Get_File_Paths_With_Regex(string File_Regex, string Folder_Path)
        {
            List<string> matched_file_paths = new List<string>();
            try
            {
                string[] files = Directory.GetFiles(Folder_Path, "*", SearchOption.AllDirectories);
                Regex regex = new Regex(File_Regex, RegexOptions.IgnoreCase);
                foreach (string file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file); // 只获取文件名，不含扩展名
                    if (regex.IsMatch(fileName))
                    {
                        matched_file_paths.Add(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("遍历文件时出错: " + ex.Message);
            }
            return matched_file_paths;
        }
        public static void Copy_File(string From_Path, string To_Path)
        {
            try
            {
                File.Copy(From_Path, To_Path, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("复制文件时出错: " + ex.Message);
            }
        }
    }
    public class SQL_Services
    {
        public static string Ivaldi_Forge_File_Path = Program.Configs_Folder_Path + "/Ivaldi_Forge.db";
        public static string DB_Connection_String = $"Data Source={Ivaldi_Forge_File_Path};Version=3;";
        public static void Init()
        {
            File_Services.Check_File_Path(Ivaldi_Forge_File_Path, true);
            using (var connection = new SQLiteConnection(DB_Connection_String))
            {
                connection.Open();
                string createTableQuery =
                  @"CREATE TABLE IF NOT EXISTS GameData_Info (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FileName TEXT NOT NULL,
                    CAB TEXT NOT NULL,
                    Type INTEGER NOT NULL,
                    Size INTEGER NOT NULL,
                    CRC TEXT NOT NULL,
                    Version TEXT NOT NULL
                );";
                var command = new SQLiteCommand(createTableQuery, connection);
                command.ExecuteNonQuery();

                createTableQuery =
                  @"CREATE TABLE IF NOT EXISTS APK_Version_Info (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Version TEXT UNIQUE,
                    ReleaseDate TEXT,
                    DownloadFileType TEXT
                );";
                command = new SQLiteCommand(createTableQuery, connection);
                command.ExecuteNonQuery();

                createTableQuery =
                  @"CREATE TABLE IF NOT EXISTS CharacterExcel (
                    Id INTEGER PRIMARY KEY NOT NULL,
                    DevName TEXT,
                    CostumeGroupId INTEGER,
                    IsPlayable INTEGER,
                    ProductionStep INTEGER,
                    CollectionVisible INTEGER,
                    ReleaseDate TEXT,
                    CollectionVisibleStartDate TEXT,
                    CollectionVisibleEndDate TEXT,
                    IsPlayableCharacter INTEGER,
                    LocalizeEtcId INTEGER,
                    Rarity INTEGER,
                    IsNPC INTEGER,
                    TacticEntityType INTEGER,
                    CanSurvive INTEGER,
                    IsDummy INTEGER,
                    SubPartsCount INTEGER,
                    TacticRole INTEGER,
                    WeaponType INTEGER,
                    TacticRange INTEGER,
                    BulletType INTEGER,
                    ArmorType INTEGER,
                    AimIKType INTEGER,
                    School INTEGER,
                    Club INTEGER,
                    DefaultStarGrade INTEGER,
                    MaxStarGrade INTEGER,
                    StatLevelUpType INTEGER,
                    SquadType INTEGER,
                    Jumpable INTEGER,
                    PersonalityId INTEGER,
                    CharacterAIId INTEGER,
                    ExternalBTId INTEGER,
                    MainCombatStyleId INTEGER,
                    CombatStyleIndex INTEGER,
                    ScenarioCharacter TEXT,
                    SpawnTemplateId INTEGER,
                    FavorLevelupType INTEGER,
                    EquipmentSlotLength INTEGER,
                    WeaponLocalizeId INTEGER,
                    DisplayEnemyInfo INTEGER,
                    BodyRadius INTEGER,
                    RandomEffectRadius INTEGER,
                    HPBarHide INTEGER,
                    HpBarHeight REAL,
                    HighlightFloaterHeight REAL,
                    EmojiOffsetX REAL,
                    EmojiOffsetY REAL,
                    MoveStartFrame INTEGER,
                    MoveEndFrame INTEGER,
                    JumpMotionFrame INTEGER,
                    AppearFrame INTEGER,
                    CanMove INTEGER,
                    CanFix INTEGER,
                    CanCrowdControl INTEGER,
                    CanBattleItemMove INTEGER,
                    IsAirUnit INTEGER,
                    AirUnitHeight INTEGER,
                    TagsLength INTEGER,
                    SecretStoneItemId INTEGER,
                    SecretStoneItemAmount INTEGER,
                    CharacterPieceItemId INTEGER,
                    CharacterPieceItemAmount INTEGER,
                    CombineRecipeId INTEGER
                );";
                command = new SQLiteCommand(createTableQuery, connection);
                command.ExecuteNonQuery();

                createTableQuery =
                  @"CREATE TABLE IF NOT EXISTS MemoryLobbyExcel (
                    Id INTEGER PRIMARY KEY NOT NULL,
                    ProductionStep INTEGER,
                    LocalizeEtcId INTEGER,
                    CharacterId INTEGER,
                    PrefabName TEXT,
                    MemoryLobbyCategory INTEGER,
                    SlotTextureName TEXT,
                    RewardTextureName TEXT,
                    BGMId INTEGER,
                    AudioClipJp TEXT,
                    AudioClipKr TEXT
                );";
                command = new SQLiteCommand(createTableQuery, connection);
                command.ExecuteNonQuery();

                createTableQuery =
                  @"CREATE TABLE IF NOT EXISTS ScenarioCharacterNameExcel (
                    CharacterName INTEGER PRIMARY KEY NOT NULL,
                    ProductionStep INTEGER,
                    NameKR TEXT,
                    NicknameKR TEXT,
                    NameJP TEXT,
                    NicknameJP TEXT,
                    Shape INTEGER,
                    SpinePrefabName TEXT,
                    SmallPortrait TEXT
                );";
                command = new SQLiteCommand(createTableQuery, connection);
                command.ExecuteNonQuery();

                createTableQuery =
                  @"CREATE TABLE IF NOT EXISTS CharacterDialogExcel (
                    CharacterId INTEGER NOT NULL,
                    CostumeUniqueId INTEGER NOT NULL,
                    DisplayOrder INTEGER,
                    ProductionStep INTEGER,
                    DialogCategory INTEGER,
                    DialogCondition INTEGER,
                    Anniversary INTEGER,
                    StartDate TEXT,
                    EndDate TEXT,
                    GroupId INTEGER,
                    DialogType INTEGER,
                    ActionName TEXT,
                    Duration INTEGER,
                    AnimationName TEXT,
                    LocalizeKR TEXT,
                    LocalizeJP TEXT,
                    VoiceIdLength INTEGER,
                    ApplyPosition INTEGER,
                    PosX REAL,
                    PosY REAL,
                    CollectionVisible INTEGER,
                    CVCollectionType INTEGER,
                    UnlockFavorRank INTEGER,
                    UnlockEquipWeapon INTEGER,
                    LocalizeCVGroup TEXT
                );";
                command = new SQLiteCommand(createTableQuery, connection);
                command.ExecuteNonQuery();
            }
        }
        public static bool Add_GameData_Info(File_Info file_info, bool isDownloadOver)
        {
            using (var connection = new SQLiteConnection(DB_Connection_String))
            {
                connection.Open();

                var command = new SQLiteCommand("SELECT COUNT(*) FROM GameData_Info WHERE FileName = @FileName AND Type = @Type AND Size = @Size AND CRC = @CRC", connection);
                command.Parameters.AddWithValue("@FileName", file_info.FileName);
                command.Parameters.AddWithValue("@Type", file_info.Type);
                command.Parameters.AddWithValue("@Size", file_info.Size);
                command.Parameters.AddWithValue("@CRC", file_info.CRC);

                long exactMatchCount = (long)command.ExecuteScalar();

                if (exactMatchCount > 0) return false;
                if (!isDownloadOver) return true;

                command = new SQLiteCommand("INSERT OR IGNORE INTO GameData_Info (ID, FileName, CAB, Type, Size, CRC, Version) VALUES ((SELECT IFNULL(MAX(ID) + 1, 1) FROM GameData_Info), @FileName, @CAB, @Type, @Size, @CRC, @Version)", connection);
                command.Parameters.AddWithValue("@FileName", file_info.FileName);
                command.Parameters.AddWithValue("@CAB", file_info.CAB);
                command.Parameters.AddWithValue("@Type", file_info.Type);
                command.Parameters.AddWithValue("@Size", file_info.Size);
                command.Parameters.AddWithValue("@CRC", file_info.CRC);
                command.Parameters.AddWithValue("@Version", file_info.Version);
                command.ExecuteNonQuery();

                return true;
            }
        }
        public class File_Info
        {
            public string FileName { get; set; } = "";
            public string CAB { get; set; } = "";
            public int Type;
            public long Size { get; set; }
            public long CRC { get; set; }
            public string Version { get; set; } = "";
        }
        public static void Add_APK_Version_Info(APK_Version_Info apk_version_info)
        {
            Log_Services.Print($"[INFO] 开始向数据库写入一条APK信息");
            string Query = "INSERT OR IGNORE INTO APK_Version_Info (ID, Version, ReleaseDate, DownloadFileType) " +
                           "VALUES ((SELECT IFNULL(MAX(ID) + 1, 1) FROM APK_Version_Info), @Version, @ReleaseDate, @DownloadFileType);";
            using (var connection = new SQLiteConnection(DB_Connection_String))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = Query;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SQLiteParameter("@Version", apk_version_info.Version));
                    cmd.Parameters.Add(new SQLiteParameter("@ReleaseDate", apk_version_info.ReleaseDate));
                    cmd.Parameters.Add(new SQLiteParameter("@DownloadFileType", apk_version_info.DownloadFileType));
                    Log_Services.Print($"[INFO] 新的APK版本为: {apk_version_info.Version}, 更新日期为: {apk_version_info.ReleaseDate}, 下载文件类型为: {apk_version_info.DownloadFileType}");
                    cmd.ExecuteNonQuery();
                    Log_Services.Print($"[INFO] 结束向数据库写入一条APK信息");
                }
            }
        }
        public static List<APK_Version_Info> Get_APK_Version_Info()
        {
            Log_Services.Print($"[INFO] 开始从数据库加载所有版本的信息");
            List<APK_Version_Info> apk_version_info_list = new List<APK_Version_Info>();
            string Query = "SELECT Version, ReleaseDate, DownloadFileType FROM APK_Version_Info;";
            using (var connection = new SQLiteConnection(DB_Connection_String))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = Query;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            APK_Version_Info apk_version_info = new APK_Version_Info();
                            apk_version_info.Version = reader["Version"].ToString() ?? "";
                            apk_version_info.ReleaseDate = reader["ReleaseDate"].ToString() ?? "";
                            apk_version_info.DownloadFileType = reader["DownloadFileType"].ToString() ?? "";
                            apk_version_info_list.Add(apk_version_info);
                        }
                    }
                }
            }
            return apk_version_info_list;
        }
        public class APK_Version_Info
        {
            public string Version { get; set; } = "";
            public string ReleaseDate { get; set; } = "";
            public string DownloadFileType { get; set; } = "";
        }
        public static void Add_CharacterExcel(List<CharacterExcel_DB> processed_character_excel_list)
        {
            Log_Services.Print($"开始向数据库写入 CharacteExcel 表", info: true);
            using (var connection = new SQLiteConnection(DB_Connection_String))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM CharacterDialogExcel";
                    command.ExecuteNonQuery();
                }
                Log_Services.Print($"清除了原有 CharacteExcel 表的数据", info: true);

                using (var transaction = connection.BeginTransaction())
                {
                    var type = typeof(GameData_Services.CharacterExcel_DB);
                    var properties = type.GetProperties();
                    var columnNames = properties.Select(p => p.Name).ToList();

                    var insertQuery = $@"
                        INSERT OR REPLACE INTO CharacterExcel
                        ({string.Join(", ", columnNames)})
                        VALUES ({string.Join(", ", columnNames.Select(c => "@" + c))})
                    ";

                    using (var cmd = new SQLiteCommand(insertQuery, connection, transaction))
                    {
                        foreach (var item in processed_character_excel_list)
                        {
                            cmd.Parameters.Clear();
                            foreach (var prop in properties)
                            {
                                var value = prop.GetValue(item);
                                var paramName = "@" + prop.Name;

                                if (prop.PropertyType.IsEnum) cmd.Parameters.AddWithValue(paramName, (int)value);
                                else if (prop.PropertyType == typeof(bool)) cmd.Parameters.AddWithValue(paramName, (bool)value ? 1 : 0);
                                else cmd.Parameters.AddWithValue(paramName, value ?? DBNull.Value);
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
        }
        public static void Add_MemoryLobbyExcel(List<MemoryLobbyExcel_DB> memory_lobby_excel_db_list)
        {
            Log_Services.Print($"开始向数据库写入 MemoryLobbyExcel 表", info: true);
            using (var connection = new SQLiteConnection(DB_Connection_String))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM MemoryLobbyExcel";
                    command.ExecuteNonQuery();
                }
                Log_Services.Print($"清除了原有 MemoryLobbyExcel 表的数据", info: true);

                using (var transaction = connection.BeginTransaction())
                {
                    var type = typeof(GameData_Services.MemoryLobbyExcel_DB);
                    var properties = type.GetProperties();
                    var columnNames = properties.Select(p => p.Name).ToList();

                    var insertQuery = $@"
                        INSERT OR REPLACE INTO MemoryLobbyExcel
                        ({string.Join(", ", columnNames)})
                        VALUES ({string.Join(", ", columnNames.Select(c => "@" + c))})
                    ";

                    using (var cmd = new SQLiteCommand(insertQuery, connection, transaction))
                    {
                        foreach (var item in memory_lobby_excel_db_list)
                        {
                            cmd.Parameters.Clear();
                            foreach (var prop in properties)
                            {
                                var value = prop.GetValue(item);
                                var paramName = "@" + prop.Name;

                                if (prop.PropertyType.IsEnum) cmd.Parameters.AddWithValue(paramName, (int)value);
                                else cmd.Parameters.AddWithValue(paramName, value ?? DBNull.Value);
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
        }
        public static void Add_ScenarioCharacterNameExcel(List<ScenarioCharacterNameExcel_DB> scenairo_character_name_excel_db_list)
        {
            Log_Services.Print($"开始向数据库写入 ScenarioCharacterNameExcel 表", info: true);
            using (var connection = new SQLiteConnection(DB_Connection_String))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM ScenarioCharacterNameExcel";
                    command.ExecuteNonQuery();
                }
                Log_Services.Print($"清除了原有 ScenarioCharacterNameExcel 表的数据", info: true);

                using (var transaction = connection.BeginTransaction())
                {
                    var type = typeof(ScenarioCharacterNameExcel_DB);
                    var properties = type.GetProperties();
                    var columnNames = properties.Select(p => p.Name).ToList();

                    var insertQuery = $@"
                        INSERT OR REPLACE INTO ScenarioCharacterNameExcel
                        ({string.Join(", ", columnNames)})
                        VALUES ({string.Join(", ", columnNames.Select(c => "@" + c))})
                    ";

                    using (var cmd = new SQLiteCommand(insertQuery, connection, transaction))
                    {
                        foreach (var item in scenairo_character_name_excel_db_list)
                        {
                            cmd.Parameters.Clear();
                            foreach (var prop in properties)
                            {
                                var value = prop.GetValue(item);
                                var paramName = "@" + prop.Name;

                                if (prop.PropertyType.IsEnum)
                                {
                                    cmd.Parameters.AddWithValue(paramName, (int)value);
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue(paramName, value ?? DBNull.Value);
                                }
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
        }
        public static void Add_CharacterDialogExcel(List<CharacterDialogExcel_DB> character_dialog_excel_db_list)
        {
            Log_Services.Print($"开始向数据库写入 CharacterDialogExcel 表", info: true);
            using (var connection = new SQLiteConnection(DB_Connection_String))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM CharacterDialogExcel";
                    command.ExecuteNonQuery();
                }
                Log_Services.Print($"清除了原有 CharacterDialogExcel 表的数据", info: true);
                
                using (var transaction = connection.BeginTransaction())
                {
                    var type = typeof(CharacterDialogExcel_DB);
                    var properties = type.GetProperties();
                    var columnNames = properties.Select(p => p.Name).ToList();

                    var insertQuery = $@"
                        INSERT OR REPLACE INTO CharacterDialogExcel
                        ({string.Join(", ", columnNames)})
                        VALUES ({string.Join(", ", columnNames.Select(c => "@" + c))})
                    ";

                    using (var cmd = new SQLiteCommand(insertQuery, connection, transaction))
                    {
                        foreach (var item in character_dialog_excel_db_list)
                        {
                            cmd.Parameters.Clear();
                            foreach (var prop in properties)
                            {
                                var value = prop.GetValue(item);
                                var paramName = "@" + prop.Name;

                                if (prop.PropertyType.IsEnum) cmd.Parameters.AddWithValue(paramName, (int)value);
                                else if (prop.PropertyType == typeof(bool)) cmd.Parameters.AddWithValue(paramName, (bool)value ? 1 : 0);
                                else cmd.Parameters.AddWithValue(paramName, value ?? DBNull.Value);
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
        }
    }
    public class AssetBundles_Services
    {
        public static string Get_CAB(string File_Path)
        {
            AssetsManager assets_manager = new AssetsManager();
            BundleFileInstance bundle_file_instance = assets_manager.LoadBundleFile(File_Path, true);
            List<string> bundle_file_names = bundle_file_instance.file.GetAllFileNames();
            foreach (var bundle_file_name in bundle_file_names)
            {
                if (!bundle_file_name.EndsWith(".resS"))
                {
                    assets_manager.UnloadBundleFile(bundle_file_instance);
                    return bundle_file_name;
                }
            }
            return "";
        }
    }
}