using System.Text;
using Newtonsoft.Json;

namespace Ivaldi
{
    public class Config_Services
    {
        /* ===== QooApp的API的配置文件 =====*/
        public static string Web_Services_Folder_Path = Path.Combine(Program.Configs_Folder_Path, "Web_Services");
        public static string API_Config_File_Path = Path.Combine(Web_Services_Folder_Path, "QooApp_API_Config.json");
        public static async Task<QooApp_API_Config> Get_QooApp_Api_Config()
        {
            Log_Services.Print($"[INFO] 开始加载QooApp的API的配置文件");
            try
            {
                if (!File_Services.Check_File_Path(API_Config_File_Path, false)) throw new Exception("QooApp_API_Config.json 文件不存在");

                string qoo_api_config_stirng = await File.ReadAllTextAsync(API_Config_File_Path, Encoding.UTF8);
                QooApp_API_Config qoo_api_config = JsonConvert.DeserializeObject<QooApp_API_Config>(qoo_api_config_stirng) ?? new QooApp_API_Config();

                Log_Services.Print($"[INFO] 结束加载QooApp的API的配置文件");
                return qoo_api_config;
            }
            catch (Exception ex)
            {
                Log_Services.Print($"[ERROR] 加载QooApp的API的配置文件时发生错误: {ex.Message}");
                return new QooApp_API_Config();
            }
        }
        /* ===== BA资源服务器的配置文件 =====*/
        public static string Server_Info_Folder_Path = Path.Combine(Web_Services_Folder_Path, "Server_Info");
        public static string Server_Info_File_Path = "";
        public static async Task<Server_Info> Get_Server_Info(string Version)
        {
            Log_Services.Print($"[INFO] 开始加载{Version}版本的资源服务器的配置文件");
            Server_Info_File_Path = Path.Combine(Server_Info_Folder_Path, Version + ".json");
            try
            {
                if (!File_Services.Check_File_Path(Server_Info_File_Path, false)) throw new Exception($"{Path.GetFileName(Server_Info_File_Path)} 文件不存在");

                string server_info_string = await File.ReadAllTextAsync(Server_Info_File_Path, Encoding.UTF8);
                Server_Info server_info = JsonConvert.DeserializeObject<Server_Info>(server_info_string) ?? new Server_Info();

                Log_Services.Print($"[INFO] 结束加载{Version}版本的资源服务器的配置文件");
                return server_info;
            }
            catch (Exception ex)
            {
                Log_Services.Print($"[ERROR] 加载{Version}版本的资源服务器的配置文件时发生错误: {ex.Message}");
                return new Server_Info();
            }

        }
        public static async Task Write_Server_Info_Config(Server_Info server_info, string Version)
        {
            Log_Services.Print($"[INFO] 开始写入{Version}版本的BA资源服务器的配置文件");
            Server_Info_File_Path = Path.Combine(Server_Info_Folder_Path, Version + ".json");
            try
            {
                //File_Services.Check_File_Path(Server_Info_File_Path, true);

                string server_info_string = JsonConvert.SerializeObject(server_info, Formatting.Indented);
                using (StreamWriter writer = new StreamWriter(Server_Info_File_Path, false, Encoding.UTF8)) await writer.WriteAsync(server_info_string);
                
                Log_Services.Print($"[INFO] {Version}版本的BA资源服务器的配置文件已保存到 {Server_Info_File_Path}");
            }
            catch (Exception ex)
            {
                Log_Services.Print($"[ERROR] 写入{Version}版本的BA资源服务器的配置文件时发生错误: {ex.Message}");
            }
        }
    }
    public class QooApp_API_Config
    {
        public string API_Url_01 { get; set; } = "";
        public string API_Url_02 { get; set; } = "";
        public Dictionary<string, string> Common_Request_Headers { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Special_Request_Headers_02 { get; set; } = new Dictionary<string, string>();
    }
    public class Server_Info
    {
        public List<ConnectionGroup> ConnectionGroups { get; set; } = new List<ConnectionGroup>();
        public class ConnectionGroup
        {
            public string Name { get; set; } = "";
            public string ManagementDataUrl { get; set; } = "";
            public bool IsProductionAddressables { get; set; } = true;
            public string ApiUrl { get; set; } = "";
            public string GatewayUrl { get; set; } = "";
            public string KibanaLogUrl { get; set; } = "";
            public string ProhibitedWordBlackListUri { get; set; } = "";
            public string ProhibitedWordWhiteListUri { get; set; } = "";
            public string CustomerServiceUrl { get; set; } = "";
            public List<OverrideConnectionGroup> OverrideConnectionGroups { get; set; } = new List<OverrideConnectionGroup>();
            public string BundleVersion { get; set; } = "";
            public class OverrideConnectionGroup
            {
                public string Name { get; set; } = "";
                public string AddressablesCatalogUrlRoot { get; set; } = "";
            }
        }
    }
}