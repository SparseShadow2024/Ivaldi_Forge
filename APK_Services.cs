using System.Text;
using Newtonsoft.Json.Linq;

namespace Ivaldi
{
    public class APK_Services
    {
        public static string GameMainConfig_File_Path = "";
        public static void Find_GameMainConfig(string UnityDataAssetPack_Folder_Path)
        {
            string Data_Folder_Path = Path.Combine(UnityDataAssetPack_Folder_Path, "assets", "bin", "Data");
            GameMainConfig_File_Path = Path.Combine(Data_Folder_Path, "GameMainConfig");
            Log_Services.Print($"[INFO] 开始在 {Data_Folder_Path} 中检索 GameMainConfig 的文件位置");
            if (File.Exists(GameMainConfig_File_Path)){
                Log_Services.Print($"GameMainConfig 已存在, 停止检索", info: true);
                return;
            }
            if (Directory.Exists(Data_Folder_Path))
            {
                var files = Directory.GetFiles(Data_Folder_Path);
                byte[] TargetBytes = { 0x47, 0x61, 0x6D, 0x65, 0x4D, 0x61, 0x69, 0x6E, 0x43, 0x6F, 0x6E, 0x66, 0x69, 0x67, 0x00, 0x00, 0x92, 0x03, 0x00, 0x00 };
                byte[] VariableBytes = { 0x92, 0x82 };
                foreach (var file in files)
                {
                    byte[] OriginalBytes = File.ReadAllBytes(file);
                    for (int i = 0; i < OriginalBytes.Length - TargetBytes.Length + 1; i++)
                    {
                        byte[] Temp_Bytes = TargetBytes;
                        foreach (byte b in VariableBytes)
                        {
                            Temp_Bytes[16] = b;
                            if (OriginalBytes.Skip(i).Take(TargetBytes.Length).SequenceEqual(Temp_Bytes))
                            {
                                byte[] result = OriginalBytes.Skip(i + 20).ToArray();
                                Log_Services.Print($"[INFO] 在文件 {Path.GetFileName(file)} 中找到了目标字段");
                                File.WriteAllBytes(GameMainConfig_File_Path, result);
                                Log_Services.Print($"[INFO] GameMainConfig 已生成在 {GameMainConfig_File_Path}");
                                return;
                            }
                        }
                    }
                }
                Log_Services.Print($"[ERROR] 没有找到目标字段");
            }
            else
            {
                Log_Services.Print($"[ERROR] Data 文件夹不存在");
            }
        }
        public static string ServerInfoDataUrl = "";
        public static string Get_ServerInfoDataUrl_From_GameMainConfig()
        {
            Log_Services.Print($"[INFO] 开始从 GameMainConfig 解码资源服务器地址");
            try
            {
                byte[] GameMainConfig_File_Bytes = File.ReadAllBytes(GameMainConfig_File_Path);
                byte[] GameMainConfig_File_Password = Crypto_Services.Generate_Password("GameMainConfig", 8);
                string GameMainConfig_File_String = Encoding.Unicode.GetString(Crypto_Services.XOR(GameMainConfig_File_Bytes, GameMainConfig_File_Password));
                GameMainConfig_File_String = GameMainConfig_File_String.Substring(0, GameMainConfig_File_String.LastIndexOf('}') + 1);
                JObject GameMainConfig_File_Json = JObject.Parse(GameMainConfig_File_String);
#pragma warning disable CS8602
                string ServerInfoDataUrl_String = GameMainConfig_File_Json["X04YXBFqd3ZpTg9cKmpvdmpOElwnamB2eE4cXDZqc3ZgTg=="].ToString();
#pragma warning restore CS8602
                byte[] ServerInfoDataUrl_Bytes = Convert.FromBase64String(ServerInfoDataUrl_String);
                byte[] ServerInfoDataUrl_Password = Crypto_Services.Generate_Password("ServerInfoDataUrl", 8);
                ServerInfoDataUrl = Encoding.Unicode.GetString(Crypto_Services.XOR(ServerInfoDataUrl_Bytes, ServerInfoDataUrl_Password));
                Log_Services.Print($"[INFO] 从 GameMainConfig 解码的资源服务器地址为 {ServerInfoDataUrl}");
                return ServerInfoDataUrl;
            }
            catch (Exception ex)
            {
                Log_Services.Print($"[INFO] 从 GameMainConfig 解码资源服务器地址时发生错误: {ex.Message}");
                return "";
            }
        }
    }
}
