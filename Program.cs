using Ivaldi;
public class Program
{
    public static string EXE_File_Path = "D:\\Projects\\C#\\Ivaldi_Forge";
    public static string Contents_Folder_Path = "";
    public static string APKs_Folder_Path = "";
    public static string GameData_Folder_Path = "";
    public static string Configs_Folder_Path = "";
    public static string Students_Folder_Path = "";
    static async Task Main(string[] args)
    {
        await Init();
        Log_Services.Print($"程序终止运行, 请按任意键退出...", info: true);
        Console.ReadKey();
    }
    static async Task Init()
    {
        Log_Services.Init();

        Log_Services.Print($"[INFO] 开始初始化基础本地路径");

        EXE_File_Path = "D:\\Projects\\C#\\Ivaldi_Forge";
        Log_Services.Print($"[INFO] 程序根目录: {EXE_File_Path}");
        File_Services.Check_Folder_Path(EXE_File_Path);

        Contents_Folder_Path = Path.Combine(EXE_File_Path, "Contents");
        Log_Services.Print($"[INFO] Contents 文件夹目录: {Contents_Folder_Path}");
        File_Services.Check_Folder_Path(Contents_Folder_Path);

        APKs_Folder_Path = Path.Combine(Contents_Folder_Path, "APKs");
        Log_Services.Print($"[INFO] APKs 文件夹目录: {Contents_Folder_Path}");
        File_Services.Check_Folder_Path(APKs_Folder_Path);

        GameData_Folder_Path = Path.Combine(Contents_Folder_Path, "GameData");
        Log_Services.Print($"[INFO] GameData 文件夹目录: {Contents_Folder_Path}");
        File_Services.Check_Folder_Path(GameData_Folder_Path);

        Configs_Folder_Path = Path.Combine(EXE_File_Path, "Configs");
        Log_Services.Print($"[INFO] Configs 文件夹目录: {Contents_Folder_Path}");
        File_Services.Check_Folder_Path(Configs_Folder_Path);

        Students_Folder_Path = Path.Combine(EXE_File_Path, "Students");
        Log_Services.Print($"[INFO] Students 文件夹目录: {Students_Folder_Path}");
        File_Services.Check_Folder_Path(Students_Folder_Path);

        Log_Services.Print($"[INFO] 结束初始化基础本地路径");

        SQL_Services.Init();

        //GameData_Services.Get_Original_AnimatorDataTable("r77_whvezlrs0633nggajegb_2");
        //return;

        // ===== 检查在 APKPure 的最新版本 =====
        SQL_Services.APK_Version_Info latest_apk_version_info = Web_Services.Get_Latest_APK_Info_From_APKPure_Webside();
        // ===== 从数据库读取APK版本信息 =====
        List<SQL_Services.APK_Version_Info> apk_version_info_list = SQL_Services.Get_APK_Version_Info();
        if (apk_version_info_list == new List<SQL_Services.APK_Version_Info>())
        {
            Log_Services.Print("数据库中没有任何版本信息", info: true);
            apk_version_info_list = Web_Services.Get_Version_And_ReleaseDate_From_APKPure_Webside();
            foreach (SQL_Services.APK_Version_Info apk_version_info in apk_version_info_list) SQL_Services.Add_APK_Version_Info(apk_version_info);
        }
        else
        {
            Log_Services.Print($"在线最新版本为 {latest_apk_version_info.Version}, 本地最新版本为 {apk_version_info_list.Last().Version}", info: true);
            if (new Version(latest_apk_version_info.Version) > new Version(apk_version_info_list.Last().Version))
            {
                Log_Services.Print($"[INFO] 发现新版本 {latest_apk_version_info.Version}");

                string AddressablesCatalogUrlRoot = await Download_All(latest_apk_version_info);
                await Analyze_MemoryLobby(latest_apk_version_info.ReleaseDate, AddressablesCatalogUrlRoot);
                SQL_Services.Add_APK_Version_Info(latest_apk_version_info);
            }
            if (new Version(latest_apk_version_info.Version) == new Version(apk_version_info_list.Last().Version))
            {
                APK_Services.Find_GameMainConfig(Path.Combine(APKs_Folder_Path, latest_apk_version_info.Version, "APKPure","UnityDataAssetPack"));
                string server_info_data_url = APK_Services.Get_ServerInfoDataUrl_From_GameMainConfig();
                Server_Info server_info = await Web_Services.Get_Server_Info(server_info_data_url);
                await Config_Services.Write_Server_Info_Config(server_info, latest_apk_version_info.Version);
                string AddressablesCatalogUrlRoot = server_info.ConnectionGroups[0].OverrideConnectionGroups[1].AddressablesCatalogUrlRoot.ToString();
                string Detail_Version = AddressablesCatalogUrlRoot.Substring(AddressablesCatalogUrlRoot.LastIndexOf('/') + 1);
                Log_Services.Print($"最在线新细分版本为 {Detail_Version}",info: true);
                if (!Path.Exists(Path.Combine(GameData_Folder_Path, "AssetBundles", Detail_Version)))
                {
                    Log_Services.Print($"[INFO] 发现新版本{Detail_Version}");
                }
                else
                {
                    Log_Services.Print($"[INFO] 当前版本为最新版本, 不做处理");
                }
                return;
            }
        }
    }
    public static async Task<string> Download_All(SQL_Services.APK_Version_Info apk_version_info)
    {
        Log_Services.Print($"开始下载 {apk_version_info.Version} 版本相关的所有的文件", info: true);
        string version = apk_version_info.Version;
        string download_file_type = apk_version_info.DownloadFileType;
        string saved_file_path;
        string decompressed_XAPK_folder_path;
        string decompressed_APK_folder_path;

        if (download_file_type == "XAPK")
        {
            saved_file_path = await Web_Services.Download_APK_OR_XAPK_From_APKPure(version, download_file_type);
            decompressed_XAPK_folder_path = File_Services.Decompress_Zip_File(saved_file_path, Path.Combine(Path.GetDirectoryName(saved_file_path) ?? "", Path.GetFileNameWithoutExtension(saved_file_path)));
            File_Services.Decompress_Zip_File(Path.Combine(decompressed_XAPK_folder_path, "com.YostarJP.BlueArchive.apk"), Path.Combine(Path.GetDirectoryName(decompressed_XAPK_folder_path) ?? "", "com.YostarJP.BlueArchive"));
            string decompressed_UnityDataAssetPack_folder_path;
            if (Web_Services.IsVersionInRange(version, "1.25.178527", "1.42.270153"))
            {
                decompressed_UnityDataAssetPack_folder_path = File_Services.Decompress_Zip_File(Path.Combine(decompressed_XAPK_folder_path, "Android", "obb", "com.YostarJP.BlueArchive", $"main.{version.Substring(version.Length - 6)}.com.YostarJP.BlueArchive.obb"), Path.Combine(Path.GetDirectoryName(decompressed_XAPK_folder_path) ?? "", "obb"));
            }
            else
            {
                File_Services.Decompress_Zip_File(Path.Combine(decompressed_XAPK_folder_path, "config.arm64_v8a.apk"), Path.Combine(Path.GetDirectoryName(decompressed_XAPK_folder_path) ?? "", "config.arm64_v8a"));
                decompressed_UnityDataAssetPack_folder_path = File_Services.Decompress_Zip_File(Path.Combine(decompressed_XAPK_folder_path, "UnityDataAssetPack.apk"), Path.Combine(Path.GetDirectoryName(decompressed_XAPK_folder_path) ?? "", "UnityDataAssetPack"));
            }
            APK_Services.Find_GameMainConfig(decompressed_UnityDataAssetPack_folder_path);
        }

        if (download_file_type == "APK")
        {
            saved_file_path = await Web_Services.Download_APK_OR_XAPK_From_APKPure(version, download_file_type);
            decompressed_APK_folder_path = File_Services.Decompress_Zip_File(saved_file_path, Path.Combine(Path.GetDirectoryName(saved_file_path) ?? "", "com.YostarJP.BlueArchive.apk"));
            APK_Services.Find_GameMainConfig(decompressed_APK_folder_path);
        }

        string server_info_data_url = APK_Services.Get_ServerInfoDataUrl_From_GameMainConfig();
        Server_Info server_info = await Web_Services.Get_Server_Info(server_info_data_url);
        await Config_Services.Write_Server_Info_Config(server_info, version);
        string AddressablesCatalogUrlRoot = server_info.ConnectionGroups[0].OverrideConnectionGroups[1].AddressablesCatalogUrlRoot.ToString();


        Log_Services.Print($"按任意键开始下载 GameData ...", info: true);
        Console.ReadKey();

        Console.Clear();
        Log_Services.Print_Banner();

        await Web_Services.Download_AssetBundles(AddressablesCatalogUrlRoot);
        await Web_Services.Download_MediaResources(version, AddressablesCatalogUrlRoot);
        await Web_Services.Download_TableBundles(version, AddressablesCatalogUrlRoot);

        Log_Services.Print($"结束下载 {apk_version_info.Version} 版本相关的所有的文件", info: true);
        return AddressablesCatalogUrlRoot;
    }
    public static async Task Analyze_MemoryLobby(string ReleaseDate, string AddressablesCatalogUrlRoot)
    {
        Log_Services.Print($"开始分析 {ReleaseDate} 版本的记忆大厅信息", info: true);

        string Detail_Version = AddressablesCatalogUrlRoot.Substring(AddressablesCatalogUrlRoot.LastIndexOf('/') + 1);

        string Excel_Zip_File_Path = File_Services.Get_File_Path("Excel.zip", Path.Combine(GameData_Folder_Path, "TableBundles", Detail_Version));
        string Excel_Zip_File_Password = Convert.ToBase64String(Crypto_Services.Generate_Password("Excel.zip", 15));
        File_Services.Decompress_Zip_File_With_Password(Excel_Zip_File_Path, Path.GetDirectoryName(Excel_Zip_File_Path) ?? "", Excel_Zip_File_Password);

        List<GameData_Services.CharacterExcel_DB> processed_character_excel_list = GameData_Services.Get_Original_CharacterExcelTable(Detail_Version);
        SQL_Services.Add_CharacterExcel(processed_character_excel_list);

        List<GameData_Services.MemoryLobbyExcel_DB> memory_lobby_excel_db_list = GameData_Services.Get_Original_MemoryLobbyExcel(Detail_Version);
        SQL_Services.Add_MemoryLobbyExcel(memory_lobby_excel_db_list);

        List<GameData_Services.ScenarioCharacterNameExcel_DB> scenairo_character_name_excel_db_list = GameData_Services.Get_Original_ScenarioCharacterNameExcel(Detail_Version);
        SQL_Services.Add_ScenarioCharacterNameExcel(scenairo_character_name_excel_db_list);

        List<GameData_Services.CharacterDialogExcel_DB> character_dialog_excel_db_list = GameData_Services.Get_Original_CharacterDialogExcel(Detail_Version);
        SQL_Services.Add_CharacterDialogExcel(character_dialog_excel_db_list);

        Student_Services.Latest_MemoryLobby_Info latest_memory_lobby_info = new Student_Services.Latest_MemoryLobby_Info();
        latest_memory_lobby_info.Detail_Version = Detail_Version;
        latest_memory_lobby_info.MemoryLobby_Info_List = Student_Services.Get_Latest_MemoryLobby_Info(ReleaseDate);
        await Student_Services.Write_Latest_MemoryLobby_Info(latest_memory_lobby_info);
        Student_Services.Copy_Latest_MemoryLobby_Related_Files(latest_memory_lobby_info);
    }
}