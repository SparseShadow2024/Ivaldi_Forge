using Newtonsoft.Json;
using System.Data.SQLite;

namespace Ivaldi
{
    class Student_Services
    {
        public static void Creat_All_Student_Folder()
        {
            List<Student_Info> student_info_list = new List<Student_Info>();
            string Query = "SELECT * FROM CharacterExcel WHERE CollectionVisible = 1";
            using (var connection = new SQLiteConnection(SQL_Services.DB_Connection_String))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = Query;
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Student_Info student_info = new Student_Info();
                            student_info.CharacterId = (long)reader["Id"];
                            student_info.CharacterName = reader["DevName"].ToString().Replace("_default", "");
                            string Character_Folder_path = Path.Combine(Program.Students_Folder_Path, student_info.CharacterName);
                            File_Services.Check_Folder_Path(Character_Folder_path);
                            string Character_MemoryLobby_Folder_path = Path.Combine(Character_Folder_path, "MemoryLobby");

                            File_Services.Check_Folder_Path(Character_MemoryLobby_Folder_path);

                            string File_Regex = $@"_mx-spinelobbies-{student_info.CharacterName.ToLower()}_home";
                            List<string> matched_file_paths = File_Services.Get_File_Paths_With_Regex(File_Regex, Path.Combine(Program.GameData_Folder_Path, "AssetBundles"));

                            foreach (string matched_file_path in matched_file_paths)
                            {
                                string Detail_Version = Path.GetFileName(Path.GetDirectoryName(matched_file_path.Replace(Path.Combine(Program.GameData_Folder_Path, "AssetBundles"), ""))) ?? "";
                                File_Services.Copy_File(matched_file_path, Path.Combine(Character_MemoryLobby_Folder_path, "[" + Detail_Version + "] " + Path.GetFileName(matched_file_path)));
                            }

                            student_info_list.Add(student_info);
                        }
                    }
                }
            }
        }
        public class Student_Info
        {
            public long CharacterId { get; set; }
            public string CharacterName { get; set; } = "";
        }
        public static MemoryLobby_Info Get_MemoryLobby_Info(long Character_ID)
        {
            Log_Services.Print($"开始获取角色ID为{Character_ID}的记忆大厅相关信息: ", info: true);
            MemoryLobby_Info memory_lobby_info = new MemoryLobby_Info();
            using (var connection = new SQLiteConnection(SQL_Services.DB_Connection_String))
            {
                connection.Open();

                string Query = "SELECT * FROM CharacterExcel WHERE Id = @CharacterId AND IsPlayableCharacter = 1";
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = Query;
                    cmd.Parameters.AddWithValue("@CharacterId", Character_ID);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            memory_lobby_info.CharacterId = Character_ID;
                            memory_lobby_info.CharacterName = reader["DevName"].ToString() ?? "";
                        }
                    }
                }

                Query = "SELECT * FROM MemoryLobbyExcel WHERE CharacterId = @CharacterId";
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = Query;
                    cmd.Parameters.AddWithValue("@CharacterId", memory_lobby_info.CharacterId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            memory_lobby_info.BGMId = (long)reader["BGMId"];
                        }
                        else
                        {
                            Log_Services.Print($"角色ID为{memory_lobby_info.CharacterId}的记忆大厅不存在背景音乐, 停止获取记忆大厅相关信息", waring: true);
                            memory_lobby_info.CharacterName = "ERROR";
                            return memory_lobby_info;
                        }
                    }
                }
                Log_Services.Print($"角色ID: {memory_lobby_info.CharacterId} 角色名称: {memory_lobby_info.CharacterName} 背景音乐ID: {memory_lobby_info.BGMId}", info: true);

                Query = "SELECT * FROM CharacterDialogExcel WHERE CharacterId = @CharacterId AND DialogCategory = 8 ORDER BY DisplayOrder ASC";
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = Query;
                    cmd.Parameters.AddWithValue("@CharacterId", memory_lobby_info.CharacterId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        string previousAnimationName = "";
                        while (reader.Read())
                        {
                            string animationName = reader["AnimationName"].ToString() ?? "";
                            string localizeJP = (reader["LocalizeJP"].ToString() ?? "").Replace("\n", "||");
                            if (!string.IsNullOrEmpty(animationName)) previousAnimationName = animationName;
                            else if (previousAnimationName != null) animationName = previousAnimationName;
                            Log_Services.Print($"AnimationName: {animationName}", info: true);
                            Log_Services.Print($"LocalizeJP: {localizeJP}", info: true);
                            memory_lobby_info.Texts.Add(new KeyValuePair<string, string>(animationName, localizeJP));
                        }
                    }
                }
            }
            Write_MemoryLobby_Info(memory_lobby_info);
            Log_Services.Print($"结束获取角色ID为{Character_ID}的记忆大厅相关信息", info: true);
            return memory_lobby_info;
        }
        public static void Write_MemoryLobby_Info(MemoryLobby_Info memory_lobby_info)
        {
            Log_Services.Print($"开始写入角色ID为{memory_lobby_info.CharacterId}的记忆大厅JSON文件", info: true);
            string json = JsonConvert.SerializeObject(memory_lobby_info, Formatting.Indented);
            string Character_Folder_path = Path.Combine(Program.Students_Folder_Path, memory_lobby_info.CharacterName);
            string Character_MemoryLobby_Folder_path = Path.Combine(Character_Folder_path, "MemoryLobby");
            File.WriteAllText(Path.Combine(Character_MemoryLobby_Folder_path, "Info.json"), json);
            Log_Services.Print($"结束写入角色ID为{memory_lobby_info.CharacterId}的记忆大厅JSON文件", info: true);
        }
        public static string Latest_MemoryLobby_Folder_Path = Path.Combine(Program.Students_Folder_Path, "_Latest_MemoryLobby");
        public static List<MemoryLobby_Info> Get_Latest_MemoryLobby_Info(string ReleaseDate)
        {
            Log_Services.Print($"开始获取最新的记忆大厅相关信息: ", info: true);
            List<MemoryLobby_Info> memory_lobby_info_list = new List<MemoryLobby_Info>();

            using (var connection = new SQLiteConnection(SQL_Services.DB_Connection_String))
            {
                connection.Open();

                List<long> latest_character_id_list = new List<long>();

                string Query = "SELECT * FROM CharacterExcel WHERE ReleaseDate >= @ReleaseDate AND IsPlayableCharacter = 1";
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = Query;
                    cmd.Parameters.AddWithValue("@ReleaseDate", ReleaseDate);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            latest_character_id_list.Add((long)reader["Id"]);
                        }
                    }
                }

                foreach (long latest_character_id in latest_character_id_list)
                {
                    MemoryLobby_Info memory_lobby_info = Get_MemoryLobby_Info(latest_character_id);
                    if (memory_lobby_info.CharacterName != "ERROR") memory_lobby_info_list.Add(memory_lobby_info);
                }
            }
            return memory_lobby_info_list;
        }
        public static async Task Write_Latest_MemoryLobby_Info(Latest_MemoryLobby_Info latest_memory_lobby_info)
        {
            Log_Services.Print($"开始写入最新的记忆大厅相关信息的JSON文件", info: true);
            string json = JsonConvert.SerializeObject(latest_memory_lobby_info, Formatting.Indented);
            await File.WriteAllTextAsync(Path.Combine(Latest_MemoryLobby_Folder_Path, "Latest_MemoryLobby_Info.json"), json);
            Log_Services.Print($"结束写入最新的记忆大厅相关信息的JSON文件", info: true);
        }
        public static void Copy_Latest_MemoryLobby_Related_Files(Latest_MemoryLobby_Info latest_memory_lobby_info)
        {
            foreach (MemoryLobby_Info memory_lobby_info in latest_memory_lobby_info.MemoryLobby_Info_List)
            {
                string File_Regex = $@"_mx-spinelobbies-{memory_lobby_info.CharacterName.ToLower()}_home";
                List<string> matched_file_paths = File_Services.Get_File_Paths_With_Regex(File_Regex, Path.Combine(Program.GameData_Folder_Path, "AssetBundles", latest_memory_lobby_info.Detail_Version));
                File_Services.Check_Folder_Path(Path.Combine(Latest_MemoryLobby_Folder_Path, memory_lobby_info.CharacterName));
                foreach (string matched_file_path in matched_file_paths)
                {
                    File_Services.Copy_File(matched_file_path, Path.Combine(Latest_MemoryLobby_Folder_Path, memory_lobby_info.CharacterName, Path.GetFileName(matched_file_path)));
                }
                
                File_Regex = $@"{memory_lobby_info.CharacterName.ToLower()}_memoriallobby_";
                matched_file_paths = File_Services.Get_File_Paths_With_Regex(File_Regex, Path.Combine(Program.GameData_Folder_Path, "MediaResources"));
                foreach (string matched_file_path in matched_file_paths)
                {
                    File_Services.Copy_File(matched_file_path, Path.Combine(Latest_MemoryLobby_Folder_Path, memory_lobby_info.CharacterName, Path.GetFileName(matched_file_path)));
                }

                File_Regex = $@"Theme_{memory_lobby_info.BGMId}";
                matched_file_paths = File_Services.Get_File_Paths_With_Regex(File_Regex, Path.Combine(Program.GameData_Folder_Path, "MediaResources"));
                foreach (string matched_file_path in matched_file_paths)
                {
                    File_Services.Copy_File(matched_file_path, Path.Combine(Latest_MemoryLobby_Folder_Path, memory_lobby_info.CharacterName, Path.GetFileName(matched_file_path)));
                }
            }
        }
        public class Latest_MemoryLobby_Info
        {
            public string Detail_Version = "";
            public List<MemoryLobby_Info> MemoryLobby_Info_List = new List<MemoryLobby_Info>();
        }
        public class MemoryLobby_Info
        {
            public long CharacterId { get; set; }
            public string CharacterName { get; set; } = "";
            public long BGMId { get; set; }
            public List<KeyValuePair<string, string>> Texts { get; set; } = new List<KeyValuePair<string, string>>();
        }
    }
}