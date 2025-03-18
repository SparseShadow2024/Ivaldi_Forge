using System.Globalization;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using MemoryPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Ivaldi
{
    public class Web_Services
    {
        public static string Http_Proxy = "http://127.0.0.1:7891";
        private static readonly HttpClientHandler _proxyHandler = new()
        {
            Proxy = new WebProxy(Http_Proxy),
            UseProxy = true
        };
        private static readonly HttpClientHandler _noRedirectHandler = new()
        {
            AllowAutoRedirect = false
        };
        public static HttpClient ProxyClient { get; } = new(_proxyHandler)
        {
            Timeout = TimeSpan.FromMinutes(2)
        };
        public static HttpClient NoRedirectClient { get; } = new(_noRedirectHandler)
        {
            Timeout = TimeSpan.FromMinutes(2)
        };
        public static HttpClient DefaultClient { get; } = new()
        {
            Timeout = TimeSpan.FromMinutes(5)
        };

        /* ===== QooApp部分 ===== */
        public static string QooApp_Latest_Version = "";
        public static string QooApp_Latest_Update = "";
        public static string QooApp_APK_Download_Url_01 = "";
        public static string QooApp_APK_Download_Url_02 = "";
        public static string QooApp_APK_Download_Url_03 = "";
        public static string QooApp_Web_Url = "https://apps.qoo-app.com/app/12252";
        public static async Task Get_APK_Info_From_QooApp_Webside()
        {
            Log_Services.Print($"[INFO] 开始从 QooApp 网页获取信息");
            try
            {
                string html = await DefaultClient.GetStringAsync(QooApp_Web_Url);
                Log_Services.Print($"[INFO] 成功获取 HTML 内容");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                Log_Services.Print($"[INFO] 成功解析 HTML 内容");
                var versionNode = doc.DocumentNode.SelectSingleNode("//span[cite[text()='版本：']]/var");
                var version = versionNode?.InnerText.Trim() ?? "未获取到版本号";
                var updateTimeNode = doc.DocumentNode.SelectSingleNode("//span[cite[text()='更新時間：']]/time");
                var updateTime = updateTimeNode?.InnerText.Trim() ?? "未获取到更新时间";
                QooApp_Latest_Version = version;
                QooApp_Latest_Update = updateTime;
                Log_Services.Print($"[INFO] 从 QooApp 获得的最新版本号为: {QooApp_Latest_Version}");
                Log_Services.Print($"[INFO] 从 QooApp 获得的最近更新时间为: {QooApp_Latest_Update}");
            }
            catch (Exception ex)
            {
                Log_Services.Print($"[ERROR] 从 QooApp 网页获取信息时发生错误: {ex.Message}");
            }
        }
        public static async Task Get_APK_Download_Url_From_QooApp_API()
        {
            Log_Services.Print($"[INFO] 开始从 QooApp 的 API 获取 APK 的下载链接");
            try
            {
                QooApp_API_Config qoo_api_config = await Config_Services.Get_QooApp_Api_Config();
                if (qoo_api_config == new QooApp_API_Config()) return;

                HttpClient http_client = NoRedirectClient;

                Log_Services.Print($"[INFO] 开始获取主包下载链接");
                HttpRequestMessage http_request_message = new HttpRequestMessage(HttpMethod.Get, qoo_api_config.API_Url_01);

                Log_Services.Print($"[INFO] 开始构建请求头");
                foreach (var Temp_Header in qoo_api_config.Common_Request_Headers) http_request_message.Headers.Add(Temp_Header.Key, Temp_Header.Value);
                http_request_message.Content = new StringContent("");
                http_request_message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                foreach (var Temp_Header in qoo_api_config.Special_Request_Headers_02) http_request_message.Headers.Add(Temp_Header.Key, Temp_Header.Value);
                string current_date_string = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'", System.Globalization.CultureInfo.InvariantCulture);
                http_request_message.Headers.Add("if-modified-since", current_date_string);
                Log_Services.Print($"[INFO] 结束构建请求头");

                HttpResponseMessage http_response_message = await http_client.SendAsync(http_request_message);
                Log_Services.Print($"[INFO] 获取到响应信息");

                if (http_response_message.StatusCode == HttpStatusCode.Moved || http_response_message.StatusCode == HttpStatusCode.Redirect)
                {
                    string redirect_url_string = http_response_message.Headers.Location?.ToString() ?? "";
                    if (!string.IsNullOrEmpty(redirect_url_string))
                    {
                        QooApp_APK_Download_Url_01 = redirect_url_string;
                        Log_Services.Print($"[INFO] 获取到主包下载链接: {redirect_url_string}");
                    }
                }
                else throw new Exception($"获取响应信息失败, 状态码{http_response_message.StatusCode}");

                Log_Services.Print($"[INFO] 开始获取分割包下载链接与版本信息");
                http_request_message = new HttpRequestMessage(HttpMethod.Get, qoo_api_config.API_Url_02);

                Log_Services.Print($"[INFO] 开始构建请求头");
                foreach (var Temp_Header in qoo_api_config.Common_Request_Headers) http_request_message.Headers.Add(Temp_Header.Key, Temp_Header.Value);
                http_request_message.Content = new StringContent("");
                http_request_message.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                Log_Services.Print($"[INFO] 结束构建请求头");

                http_response_message = await http_client.SendAsync(http_request_message);
                Log_Services.Print($"[INFO] 获取到响应信息");

                string response_content_string = await http_response_message.Content.ReadAsStringAsync();
                if (response_content_string != null)
                {
                    Log_Services.Print($"[INFO] 开始反序列化响应信息");
                    JObject response_content_JSON = JObject.Parse(response_content_string);
                    Log_Services.Print($"[INFO] 结束反序列化响应信息");
#pragma warning disable CS8602
                    QooApp_APK_Download_Url_02 = response_content_JSON["data"]["splitApks"][0]["url"].ToString();
                    QooApp_APK_Download_Url_03 = response_content_JSON["data"]["splitApks"][1]["url"].ToString();
#pragma warning restore CS8602
                    Log_Services.Print($"[INFO] 获取到分割包下载链接1: {QooApp_APK_Download_Url_02}");
                    Log_Services.Print($"[INFO] 获取到分割包下载链接2: {QooApp_APK_Download_Url_03}");
                }
                else throw new Exception($"响应体为空");
            }
            catch (Exception ex)
            {
                Log_Services.Print($"[ERROR] 从 QooApp 的 API 获取 APK 的下载链接时发生错误: {ex.Message}");
                return;
            }
        }
        public static async Task Download_APK_From_QooApp(string Download_Url, string Saved_Path)
        {
            Log_Services.Print($"[INFO] 开始从 QooApp 下载 APK");
            if (File.Exists(Saved_Path))
            {
                Log_Services.Print($"[INFO] 文件{Path.GetFileName(Saved_Path)}已存在, 停止下载");
                return;
            }
            try
            {
                Log_Services.Print($"[INFO] 开始下载文件{Path.GetFileName(Download_Url)}");
                HttpResponseMessage http_response_message = await DefaultClient.GetAsync(Download_Url);

                byte[] apk_bytes = await http_response_message.Content.ReadAsByteArrayAsync();
                File.WriteAllBytes(Saved_Path, apk_bytes);

                Log_Services.Print($"[INFO] 结束下载文件{Path.GetFileName(Download_Url)}并保存在{Saved_Path}");
            }
            catch (Exception ex)
            {
                Log_Services.Print($"[ERROR] 从 QooApp 下载 APK 时发生错误: {ex.Message}");
                return;
            }
        }

        /* ===== APKPure部分 ===== */
        public static string APKPure_Version_Web_Url = "https://apkpure.com/blue-archive/com.YostarJP.BlueArchive/versions";
        public static List<SQL_Services.APK_Version_Info> Get_Version_And_ReleaseDate_From_APKPure_Webside()
        {
            Log_Services.Print($"[INFO] 开始从 APKPure 网页获取所有版本信息");
            List<SQL_Services.APK_Version_Info> apk_version_info_list = new List<SQL_Services.APK_Version_Info>();
            ChromeOptions chrome_options = new ChromeOptions();
            chrome_options.AddArgument($"--proxy-server={Http_Proxy}");
            Log_Services.Print($"[INFO] 成功设置代理");
            Log_Services.Print($"[INFO] 正在启动 ChromeDriver");
            IWebDriver i_web_driver = new ChromeDriver(chrome_options);
            Log_Services.Print($"[INFO] ?");
            try
            {
                i_web_driver.Navigate().GoToUrl(APKPure_Version_Web_Url);
                try
                {
                    while (true)
                    {
                        IWebElement showMoreButton = i_web_driver.FindElement(By.CssSelector("div.ver_show_more"));
                        string buttonType = showMoreButton.GetAttribute("data-dt-type");
                        if (buttonType == "show_less")
                        {
                            Log_Services.Print($"[INFO] 所有内容已加载，按钮状态已变为 Show Less");
                            break;
                        }

                        ((IJavaScriptExecutor)i_web_driver).ExecuteScript("arguments[0].click();", showMoreButton);

                        Log_Services.Print($"[INFO] 已点击 Show More 按钮");

                        Thread.Sleep(2000);
                    }
                }
                catch (Exception ex)
                {
                    Log_Services.Print($"[ERROR] 点击 Show More 按钮时出错: {ex.Message}");
                    return apk_version_info_list;
                }

                var versionItems = i_web_driver.FindElements(By.CssSelector("ul.ver-wrap > li"));

                foreach (var item in versionItems)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(item.Text)) continue;

                        string versionNumber = item.FindElement(By.CssSelector(".ver-item-n")).Text
                            .Replace("ブルーアーカイブ", "")
                            .Trim();

                        DateTime parsedDate;
                        IWebElement dateElement = item.FindElement(By.CssSelector(".update-on"));
                        string dateText = (string)((IJavaScriptExecutor)i_web_driver).ExecuteScript("return arguments[0].textContent", dateElement);
                        string formattedDate = "未格式化成功 / 未获取到更新时间";
                        string[] dateFormats = { "MMM dd, yyyy", "MMMM dd, yyyy", "MMM d, yyyy", "MMMM d, yyyy" };
                        if (DateTime.TryParseExact(dateText, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate)) formattedDate = parsedDate.ToString("yyyy-MM-dd");

                        SQL_Services.APK_Version_Info apk_version_info = new SQL_Services.APK_Version_Info();
                        apk_version_info.Version = versionNumber;
                        apk_version_info.ReleaseDate = formattedDate;
                        apk_version_info_list.Add(apk_version_info);
                        Log_Services.Print($"[INFO] Version: {apk_version_info.Version}, Date: {apk_version_info.ReleaseDate}");
                    }
                    catch (Exception ex)
                    {
                        Log_Services.Print($"[WARNING] 解析版本条目时出错: {ex.Message}");
                        return apk_version_info_list;
                    }
                }
                Log_Services.Print($"[INFO] 结束从 APKPure 网页获取所有版本信息");
                return apk_version_info_list;
            }
            catch (Exception ex)
            {
                Log_Services.Print($"[ERROR] 从 APKPure 网页获取所有版本信息时发生错误: {ex.Message}");
                return apk_version_info_list;
            }
            finally
            {
                i_web_driver.Quit();
                i_web_driver.Dispose();
                Log_Services.Print($"[INFO] 已销毁 ChromeDriver 会话");
            }
        }
        public static SQL_Services.APK_Version_Info Get_Latest_APK_Info_From_APKPure_Webside()
        {
            Log_Services.Print($"[INFO] 开始从 APKPure 网页获取最新的信息");
            string Url = "https://apkpure.com/blue-archive/com.YostarJP.BlueArchive/download";
            ChromeOptions chrome_options = new ChromeOptions();
            chrome_options.AddArgument($"--proxy-server={Http_Proxy}");
            Log_Services.Print($"[INFO] 成功设置代理");
            Log_Services.Print($"[INFO] 正在启动 ChromeDriver");
            IWebDriver i_web_driver = new ChromeDriver(chrome_options);
            try
            {
                i_web_driver.Navigate().GoToUrl(Url);
                
                Log_Services.Print($"[INFO] 成功访问目标网址");

                IWebElement i_web_element = i_web_driver.FindElement(By.XPath("//span[@class='version-name one-line']"));
                string version = i_web_element.Text.Trim() ?? "未获取到版本号";
                Log_Services.Print($"[INFO] 从 APKPure 获得的最新版本号为: {version}");

                i_web_element = i_web_driver.FindElement(By.XPath("//div[@class='desc'][text()='Update date']//preceding-sibling::div[@class='head']"));
                string update_date = i_web_element.Text.Trim();
                DateTime parsed_date;
                string formatted_update_date = "未格式化成功 / 未获取到更新时间";
                string[] Formats = { "MMM dd, yyyy", "MMMM dd, yyyy", "MMM d, yyyy", "MMMM d, yyyy" };
                if (DateTime.TryParseExact(update_date, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed_date)) formatted_update_date = parsed_date.ToString("yyyy-MM-dd");
                Log_Services.Print($"[INFO] 从 APKPure 获得的最近更新时间为: {formatted_update_date}");

                i_web_element = i_web_driver.FindElement(By.XPath("//a[contains(@class, 'info-tag')]"));
                string file_type = i_web_element.Text.Trim();
                Log_Services.Print($"[INFO] 从 APKPure 获得的最新版本的下载文件类型为: {file_type}");

                return new SQL_Services.APK_Version_Info()
                {
                    Version = version,
                    ReleaseDate = formatted_update_date,
                    DownloadFileType = file_type
                };
            }
            catch (Exception ex)
            {
                Log_Services.Print($"[ERROR] 从 APKPure 网页获取最新的信息时发生错误: {ex.Message}");
            }
            finally
            {
                i_web_driver.Quit();
                i_web_driver.Dispose();
                Log_Services.Print($"[INFO] 已销毁 ChromeDriver 会话");
            }
            return new SQL_Services.APK_Version_Info();
        }
        public static string Get_Download_File_Type_From_APKPure(string Version)
        {
            Log_Services.Print($"[INFO] 开始从 APKPure 网页获取下载文件格式");
            string Url = $"https://apkpure.com/blue-archive/com.YostarJP.BlueArchive/download/{Version}";
            ChromeOptions chrome_options = new ChromeOptions();
            chrome_options.AddArgument($"--proxy-server={Http_Proxy}");
            Log_Services.Print($"[INFO] 成功设置代理");
            Log_Services.Print($"[INFO] 正在启动 ChromeDriver");
            IWebDriver i_web_driver = new ChromeDriver(chrome_options);
            try
            {
                i_web_driver.Navigate().GoToUrl(Url);
                Log_Services.Print($"[INFO] 成功访问目标网址");
                IWebElement i_web_element = i_web_driver.FindElement(By.XPath("//a[contains(@class, 'info-tag')]"));
                string fileType = i_web_element.Text.Trim();
                Log_Services.Print($"[INFO] {Version} 版本的下载文件格式为: {fileType}");
                return fileType;
            }
            catch (Exception ex)
            {
                Log_Services.Print($"[ERROR] 从 APKPure 网页获取下载文件格式时发生错误: {ex.Message}");
            }
            finally
            {
                i_web_driver.Quit();
                i_web_driver.Dispose();
                Log_Services.Print($"[INFO] 已销毁 ChromeDriver 会话");
            }
            return "XAPK";
        }
        public static async Task<string> Download_APK_OR_XAPK_From_APKPure(string Version, string download_file_type)
        {
            Log_Services.Print($"[INFO] 开始从 APKPure 下载 {Version} 版本的 {download_file_type} 文件");
            if (File.Exists(Path.Combine(Program.APKs_Folder_Path, Version, "APKPure") + $"/ブルーアーカイブ_{Version}_APKPure.{download_file_type.ToLower()}"))
            {
                Log_Services.Print($"[INFO] 文件 ブルーアーカイブ_{Version}_APKPure.{download_file_type.ToLower()} 已存在, 停止下载");
                return Path.Combine(Program.APKs_Folder_Path, Version, "APKPure") + $"\\ブルーアーカイブ_{Version}_APKPure.{download_file_type.ToLower()}";
            }

            string Download_Url = $"https://d.apkpure.com/b/{download_file_type}/com.YostarJP.BlueArchive?versionCode={Version.Substring(Version.Length - 6)}&nc=arm64-v8a&sv=24";
            string Saved_Folder_Path = Path.Combine(Program.APKs_Folder_Path, Version, "APKPure");
            File_Services.Check_Folder_Path(Saved_Folder_Path);
            string Saved_File_Path;

            ChromeOptions options = new ChromeOptions();
            options.AddArgument($"--proxy-server={Http_Proxy}");
            options.AddUserProfilePreference("download.default_directory", Path.GetFullPath(Saved_Folder_Path));
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("download.directory_upgrade", true);
            options.AddUserProfilePreference("safebrowsing.enabled", true);

            using IWebDriver driver = new ChromeDriver(options);
            try
            {
                driver.Navigate().GoToUrl(Download_Url);
                Log_Services.Print($"[INFO] 正在访问下载地址: {Download_Url}");

                int Timeout = 5 * 60;
                bool isDownloaded = await Is_Downloaded(Saved_Folder_Path, TimeSpan.FromSeconds(Timeout));
                if (!isDownloaded) throw new TimeoutException($"文件下载超过{Timeout}秒且未找到下载的 {download_file_type} 文件");
                var Files = Directory.GetFiles(Saved_Folder_Path, $"*.{download_file_type.ToLower()}");
                if (Files.Length == 0) throw new FileNotFoundException("未找到下载的{download_file_type}文件");
                Saved_File_Path = Files[0];

                Log_Services.Print($"[INFO] 结束从 APKPure 下载 {Version} 版本的 {download_file_type} 文件");


                return Saved_File_Path;
            }
            catch (Exception ex)
            {
                Log_Services.Print($"[ERROR] 从 APKPure 下载 {Version} 版本的 XAPK 文件时发生错误: {ex.Message}");
                return "";
            }
            finally
            {
                driver.Quit();
            }
        }
        private static async Task<bool> Is_Downloaded(string Saved_Path, TimeSpan Timeout)
        {
            DateTime start_time = DateTime.Now;
            while (DateTime.Now - start_time < Timeout)
            {
                await Task.Delay(1000);

                bool has_Temp_File = Directory.GetFiles(Saved_Path, "*.crdownload").Any();
                bool has_APK_File = Directory.GetFiles(Saved_Path, "*.apk").Any();
                bool has_XAPK_File = Directory.GetFiles(Saved_Path, "*.xapk").Any();

                if ((!has_Temp_File && has_XAPK_File) || (!has_Temp_File && has_APK_File)) return true;
            }
            return false;
        }

        /* ===== SeverInfo部分 ===== */
        public static string GameData_Latest_Version = "";
        public static string GameData_Detail_Latest_Version = "";
        public static string AddressablesCatalogUrlRoot = "";
        public static async Task<Server_Info> Get_Server_Info(string ServerInfoDataUrl)
        {
            Log_Services.Print($"[INFO] 开始获取资源服务器信息");
            try
            {
                string json = await DefaultClient.GetStringAsync(ServerInfoDataUrl);
                Server_Info server_info = JsonConvert.DeserializeObject<Server_Info>(json) ?? new Server_Info();

                GameData_Latest_Version = server_info.ConnectionGroups[0].OverrideConnectionGroups[1].Name.ToString() ?? "未获取到最新游戏数据版本";
                AddressablesCatalogUrlRoot = server_info.ConnectionGroups[0].OverrideConnectionGroups[1].AddressablesCatalogUrlRoot.ToString() ?? "未获取到GameData根网址";
                GameData_Detail_Latest_Version = AddressablesCatalogUrlRoot.Substring(AddressablesCatalogUrlRoot.LastIndexOf('/') + 1);
                Log_Services.Print($"[INFO] 获取到的游戏数据版本: {GameData_Latest_Version}");
                Log_Services.Print($"[INFO] 获取到的详细游戏数据版本: {GameData_Detail_Latest_Version}");
                Log_Services.Print($"[INFO] 获取到的GameData根网址 {AddressablesCatalogUrlRoot}");

                Log_Services.Print($"[INFO] 结束获取资源服务器信息");
                return server_info;
            }
            catch (Exception ex)
            {
                Log_Services.Print($"[ERROR] 获取资源服务器信息时发生错误: {ex.Message}");
                return new Server_Info();
            }
        }

        /* ===== 通用下载函数 ===== */
        private static async Task Download_File(string Url, string Local_Path, int File_Type, long Expected_Crc, long Expected_Size, string Detail_Version)
        {
            if (File.Exists(Local_Path))
            {
                long localFileSize = new FileInfo(Local_Path).Length;
                if (localFileSize == Expected_Size)
                {
                    Log_Services.Print($"[INFO] 文件 {Path.GetFileName(Local_Path)} 已存在且大小一致, 跳过下载");
                    string cab = "NULL";
                    if (File_Type == 1) cab = AssetBundles_Services.Get_CAB(Local_Path);
                    Log_Services.Print($"{SQL_Services.Add_GameData_Info(new SQL_Services.File_Info
                    {
                        FileName = Path.GetFileName(Local_Path),
                        CAB = cab,
                        Type = File_Type,
                        Size = Expected_Size,
                        CRC = Expected_Crc,
                        Version = Detail_Version
                    }, true)}");
                    Log_Services.Print($"{Path.GetFileName(Local_Path)} {cab} {File_Type} {Expected_Size} {Expected_Crc} {Detail_Version}");
                    return;
                }
                else
                {
                    File.Delete(Local_Path);
                    Log_Services.Print($"[INFO] 文件 {Path.GetFileName(Local_Path)} 已存在但大小不一致, 已删除旧文件并重新下载");
                }
            }


            using (HttpResponseMessage response = await DefaultClient.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead))
            {
                if (!response.IsSuccessStatusCode) throw new Exception($"HTTP 错误代码{response.StatusCode}");

                long totalBytes = response.Content.Headers.ContentLength.GetValueOrDefault();

                long downloadedBytes = 0;
                long lastDownloadedBytes = 0;
                DateTime startTime = DateTime.Now;
                DateTime lastUpdateTime = DateTime.Now;

                using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                using (FileStream fileStream = new FileStream(Local_Path, FileMode.Create, FileAccess.Write, FileShare.None, 65536, true))  // 设置缓冲区为 64KB
                {
                    byte[] buffer = new byte[65536];
                    int bytesRead;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        downloadedBytes += bytesRead;

                        double progress = (double)downloadedBytes / totalBytes * 100;

                        TimeSpan elapsedTime = DateTime.Now - lastUpdateTime;
                        double speed = 0;
                        if (elapsedTime.TotalSeconds > 0)
                        {
                            speed = (downloadedBytes - lastDownloadedBytes) / elapsedTime.TotalSeconds;
                            lastUpdateTime = DateTime.Now;
                            lastDownloadedBytes = downloadedBytes;
                        }

                        //Log_Services.Print($"[INFO] 文件 {Path.GetFileName(Local_Path)} 下载进度: {progress:F2}% ({downloadedBytes} / {totalBytes} bytes) 速度: {speed / 1024:F2} KB/s");
                    }
                }

                Log_Services.Print($"[INFO] 文件 {Local_Path} 下载完成");


                string cab = "NULL";
                if (File_Type == 1)
                {
                    cab = AssetBundles_Services.Get_CAB(Local_Path);
                    //if (Local_Path)
                }

                SQL_Services.Add_GameData_Info(new SQL_Services.File_Info
                {
                    FileName = Path.GetFileName(Local_Path),
                    CAB = cab,
                    Type = File_Type,
                    Size = Expected_Size,
                    CRC = Expected_Crc,
                    Version = Detail_Version
                }, true);
            }
        }

        /* ===== AssetBundles ===== */
        public static string AssetBundles_Folder_Path = Path.Combine(Program.GameData_Folder_Path, "AssetBundles");
        public static async Task Download_AssetBundles(string AddressablesCatalogUrlRoot)
        {
            string Detail_Version = AddressablesCatalogUrlRoot.Substring(AddressablesCatalogUrlRoot.LastIndexOf('/') + 1);
            string AssetBundles_Detail_Version_Folder_Path = Path.Combine(AssetBundles_Folder_Path, Detail_Version);

            File_Services.Check_Folder_Path(AssetBundles_Detail_Version_Folder_Path);
            string bundleDownloadInfo_File_Path = Path.Combine(AssetBundles_Detail_Version_Folder_Path, "Catalog", "bundleDownloadInfo.json");

            if (!File.Exists(bundleDownloadInfo_File_Path))
            {
                Log_Services.Print($"[INFO] 开始下载 bundleDownloadInfo.json");
                string bundleDownloadInfo_JSON_Url = AddressablesCatalogUrlRoot + "/Android/bundleDownloadInfo.json";
                try
                {
                    using (HttpResponseMessage http_response_message = await DefaultClient.GetAsync(bundleDownloadInfo_JSON_Url))
                    {
                        if (!http_response_message.IsSuccessStatusCode) throw new Exception($"HTTP 错误代码{http_response_message.StatusCode}");

                        string content_string = await http_response_message.Content.ReadAsStringAsync();
                        File_Services.Check_File_Path(bundleDownloadInfo_File_Path, true);
                        await File.WriteAllTextAsync(bundleDownloadInfo_File_Path, content_string);
                        Log_Services.Print($"[INFO] bundleDownloadInfo.json 已保存到{bundleDownloadInfo_File_Path}");
                    }
                }
                catch (Exception ex)
                {
                    Log_Services.Print($"[ERROR] 下载 bundleDownloadInfo.json 时出现错误: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Log_Services.Print($"[ERROR] InnerException: {ex.InnerException.Message}");
                        if (ex.InnerException.InnerException != null)
                        {
                            Log_Services.Print($"[ERROR] InnerException InnerException: {ex.InnerException.InnerException.Message}");
                        }
                    }
                    return;
                }
            }

            string json = await File.ReadAllTextAsync(bundleDownloadInfo_File_Path);
            BundleDownloadInfo data = JsonConvert.DeserializeObject<BundleDownloadInfo>(json) ?? new BundleDownloadInfo();

            var bundleFiles = data.BundleFiles;
            int Total_AssetBundles_Num = data.BundleFiles.Count;
            int Cur_AssetBundles_Index = 0;
            int maxParallelDownloads = 10;

            Log_Services.Print($"[INFO] 正在启动 SemaphoreSlim");
            using (SemaphoreSlim semaphore_slim = new SemaphoreSlim(maxParallelDownloads))
            {
                var tasks = bundleFiles.Select(async bundleFile =>
                {
                    await semaphore_slim.WaitAsync();
                    try
                    {
                        Log_Services.Print($"{maxParallelDownloads - semaphore_slim.CurrentCount}");

                        int index = Interlocked.Increment(ref Cur_AssetBundles_Index);

                        string downloadUrl = $"{AddressablesCatalogUrlRoot}/Android/{bundleFile.Name}";
                        string localPath = Path.Combine(AssetBundles_Detail_Version_Folder_Path, bundleFile.Name);

                        File_Services.Check_Folder_Path(Path.GetDirectoryName(localPath) ?? "");

                        SQL_Services.File_Info file_Info = new SQL_Services.File_Info
                        {
                            FileName = bundleFile.Name,
                            CAB = "NULL",
                            Type = 1,
                            Size = bundleFile.Size,
                            CRC = bundleFile.Crc,
                            Version = Detail_Version
                        };
                        if (SQL_Services.Add_GameData_Info(file_Info, false))
                        {
                            Log_Services.Print($"[INFO] 开始下载第{index.ToString($"D{Total_AssetBundles_Num.ToString().Length}")}/{Total_AssetBundles_Num}个文件: {bundleFile.Name}");
                            await Download_File(downloadUrl, localPath, 1, bundleFile.Crc, bundleFile.Size, Detail_Version);
                        }
                        else
                        {
                            Log_Services.Print($"[INFO] {index.ToString($"D{Total_AssetBundles_Num.ToString().Length}")}/{Total_AssetBundles_Num}: {bundleFile.Name} 在数据库中已存在, 略过");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log_Services.Print($"[ERROR] 下载文件 {bundleFile.Name} 时出错: {ex.Message}");
                    }
                    finally
                    {
                        semaphore_slim.Release();
                    }
                });
                await Task.WhenAll(tasks);
            }
        }

        /* ===== MediaResources =====*/
        public static string MediaResources_Folder_Path = Path.Combine(Program.GameData_Folder_Path, "MediaResources");
        public static async Task Download_MediaResources(string Version, string AddressablesCatalogUrlRoot)
        {
            string Detail_Version = AddressablesCatalogUrlRoot.Substring(AddressablesCatalogUrlRoot.LastIndexOf('/') + 1);
            string MediaResources_Detail_Version_Folder_Path = Path.Combine(MediaResources_Folder_Path, Detail_Version);
            File_Services.Check_Folder_Path(MediaResources_Detail_Version_Folder_Path);
            string MediaCatalog_Bytes_Url = "";
            if (IsVersionInRange(Version, "1.25.178527", "1.42.270153"))
            {
                MediaCatalog_Bytes_Url = AddressablesCatalogUrlRoot + "/MediaResources/MediaCatalog.json";
            }
            if (IsVersionInRange(Version, "1.43.275921", "1.52.314950"))
            {
                MediaCatalog_Bytes_Url = AddressablesCatalogUrlRoot + "/MediaResources/MediaCatalog.bytes";
            }
            if (IsVersionInRange(Version, "1.53.322553", "9.99.999999"))
            {
                MediaCatalog_Bytes_Url = AddressablesCatalogUrlRoot + "/MediaResources/Catalog/MediaCatalog.bytes";
            }
            string MediaCatalog_File_Path = "";
            try
            {
                if (IsVersionInRange(Version, "1.43.275921", "9.99.999999"))
                {
                    MediaCatalog_File_Path = Path.Combine(MediaResources_Detail_Version_Folder_Path, "Catalog", "MediaCatalog.bytes");
                    Console.WriteLine($"{MediaCatalog_File_Path}");
                    if (!File.Exists(MediaCatalog_File_Path))
                    {
                        using (HttpClient httpclient = new HttpClient())
                        {
                            using (HttpResponseMessage response = await httpclient.GetAsync(MediaCatalog_Bytes_Url))
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    throw new Exception($"HTTP 错误代码{response.StatusCode}");
                                }
                                var content = await response.Content.ReadAsByteArrayAsync();
                                File_Services.Check_File_Path(MediaCatalog_File_Path, false);
                                await File.WriteAllBytesAsync(MediaCatalog_File_Path, content);
                                Log_Services.Print($"[INFO] MediaCatalog.bytes 已保存到{MediaCatalog_File_Path}");
                            }
                        }
                        byte[] bytes = File.ReadAllBytes(MediaCatalog_File_Path);
                        MediaCatalog data = MemoryPackSerializer.Deserialize<MediaCatalog>(bytes) ?? new MediaCatalog();
                        MediaCatalog_File_Path = Path.Combine(MediaResources_Detail_Version_Folder_Path, "Catalog", "MediaCatalog.json");
                        string MediaCatalog_File_JSON = JsonConvert.SerializeObject(data);
                        File.WriteAllText(MediaCatalog_File_Path, MediaCatalog_File_JSON);
                        Log_Services.Print($"[INFO] MediaCatalog.json 已保存到{MediaCatalog_File_Path}");
                    }
                    MediaCatalog_File_Path = Path.Combine(MediaResources_Detail_Version_Folder_Path, "Catalog", "MediaCatalog.json");
                }
                else
                {
                    MediaCatalog_File_Path = Path.Combine(MediaResources_Detail_Version_Folder_Path, "Catalog", "MediaCatalog.json");
                    if (!File.Exists(MediaCatalog_File_Path))
                    {
                        using (HttpClient httpclient = new HttpClient())
                        {
                            using (HttpResponseMessage response = await httpclient.GetAsync(MediaCatalog_Bytes_Url))
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    throw new Exception($"HTTP 错误代码{response.StatusCode}");
                                }
                                var content = await response.Content.ReadAsStringAsync();
                                File_Services.Check_File_Path(MediaCatalog_File_Path, false);
                                File.WriteAllText(MediaCatalog_File_Path, content);
                                Log_Services.Print($"[INFO] MediaCatalog.json 已保存到{MediaCatalog_File_Path}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log_Services.Print(ex.Message);
            }

            byte[] bytesJson = File.ReadAllBytes(MediaCatalog_File_Path);
            MediaCatalog jsonData = JsonConvert.DeserializeObject<MediaCatalog>(Encoding.UTF8.GetString(bytesJson)) ?? new MediaCatalog();
            var mediaFiles = jsonData.Table.Values.ToList();
            int Total_MediaResources_Num = mediaFiles.Count;
            int Cur_MediaResources_Index = 0;
            int maxParallelDownloads = 10;

            using (var semaphore = new SemaphoreSlim(maxParallelDownloads))
            {
                var tasks = mediaFiles.Select(async mediaItem =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        int index = Interlocked.Increment(ref Cur_MediaResources_Index);

                        string downloadUrl = $"{AddressablesCatalogUrlRoot}/MediaResources/{mediaItem.Path}";
                        string localPath = Path.Combine(MediaResources_Detail_Version_Folder_Path, mediaItem.Path.Replace("GameData\\", ""));

                        SQL_Services.File_Info file_info = new SQL_Services.File_Info
                        {
                            FileName = mediaItem.FileName,
                            CAB = "NULL",
                            Type = 2,
                            Size = mediaItem.Bytes,
                            CRC = mediaItem.Crc,
                            Version = Detail_Version
                        };
                        //Log_Services.Print($"[INFO] {file_Info.FileName}: CAB:{file_Info.CAB} Type:{file_Info.Type} Size:{file_Info.Size} CRC:{file_Info.CRC} Version:{file_Info.Version}");

                        if (SQL_Services.Add_GameData_Info(file_info, false))
                        {
                            File_Services.Check_Folder_Path(Path.GetDirectoryName(localPath) ?? "");
                            Log_Services.Print($"[INFO] 正在下载 {index.ToString($"D{Total_MediaResources_Num.ToString().Length}")}/{Total_MediaResources_Num} 个文件: {mediaItem.FileName}");
                            await Download_File(downloadUrl, localPath, 2, mediaItem.Crc, mediaItem.Bytes, AddressablesCatalogUrlRoot.Substring(AddressablesCatalogUrlRoot.LastIndexOf('/') + 1));

                            if (IsVersionInRange(Version, "1.53.322553", "9.99.999999"))
                            {
                                byte[] Media_File_Password_Bytes = Crypto_Services.Generate_Password(Path.GetFileName(localPath).ToLower(), 15);
                                string Media_File_Password_String = Convert.ToBase64String(Media_File_Password_Bytes);
                                Log_Services.Print($"[INFO] 当前压缩包密码为: {Media_File_Password_String}");
                                File_Services.Decompress_Zip_File_With_Password(localPath, Path.Combine(Path.GetDirectoryName(localPath) ?? "", Path.GetFileNameWithoutExtension(localPath)), Media_File_Password_String);
                            }
                        }
                        else
                        {
                            Log_Services.Print($"[INFO] {index.ToString($"D{Total_MediaResources_Num.ToString().Length}")}/{Total_MediaResources_Num}: {mediaItem.FileName} 在数据库中已存在, 略过");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log_Services.Print($"[ERROR] 下载文件 {mediaItem.FileName} 时出错: {ex.Message}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }).ToList();
                await Task.WhenAll(tasks);
            }
        }
        public static bool IsVersionInRange(string versionToCheck, string rangeStart, string rangeEnd)
        {
            Version checkVersion = new Version(versionToCheck);
            Version startVersion = new Version(rangeStart);
            Version endVersion = new Version(rangeEnd);

            return checkVersion >= startVersion && checkVersion <= endVersion;
        }

        /* ===== TableBundles =====*/
        public static string TableBundles_Folder_Path = Path.Combine(Program.GameData_Folder_Path, "TableBundles");
        public static async Task Download_TableBundles(string Version, string AddressablesCatalogUrlRoot)
        {
            string Detail_Version = AddressablesCatalogUrlRoot.Substring(AddressablesCatalogUrlRoot.LastIndexOf('/') + 1);
            string TableBundles_Detail_Version_Folder_Path = Path.Combine(TableBundles_Folder_Path, Detail_Version);
            File_Services.Check_Folder_Path(TableBundles_Detail_Version_Folder_Path);
            string TableCatalog_Bytes_Url = "";
            if (IsVersionInRange(Version, "1.25.178527", "1.43.275921"))
            {
                TableCatalog_Bytes_Url = AddressablesCatalogUrlRoot + "/TableBundles/TableCatalog.json";
            }
            if (IsVersionInRange(Version, "1.44.278134", "9.99.999999"))
            {
                TableCatalog_Bytes_Url = AddressablesCatalogUrlRoot + "/TableBundles/TableCatalog.bytes";
            }
            string TableCatalog_File_Path;

            try
            {
                if (IsVersionInRange(Version, "1.44.278134", "9.99.999999"))
                {
                    TableCatalog_File_Path = Path.Combine(TableBundles_Detail_Version_Folder_Path, "Catalog", "TableCatalog.bytes");
                    using (HttpResponseMessage response = await DefaultClient.GetAsync(TableCatalog_Bytes_Url))
                    {
                        if (!response.IsSuccessStatusCode) throw new Exception($"HTTP 错误代码{response.StatusCode}");
                        var content = await response.Content.ReadAsByteArrayAsync();
                        File_Services.Check_File_Path(TableCatalog_File_Path, false);
                        await File.WriteAllBytesAsync(TableCatalog_File_Path, content);
                        Log_Services.Print($"[INFO] TableCatalog.bytes 已保存到{TableCatalog_File_Path}");
                    }

                    byte[] bytes = File.ReadAllBytes(TableCatalog_File_Path);
                    TableCatalog data = MemoryPackSerializer.Deserialize<TableCatalog>(bytes) ?? new TableCatalog();
                    TableCatalog_File_Path = Path.Combine(TableBundles_Detail_Version_Folder_Path, "Catalog", "TableCatalog.json");
                    string TableCatalog_File_JSON = JsonConvert.SerializeObject(data);
                    File.WriteAllText(TableCatalog_File_Path, TableCatalog_File_JSON);
                    Log_Services.Print($"[INFO] TableCatalog.json 已保存到{TableCatalog_File_Path}");
                }
                else
                {
                    TableCatalog_File_Path = Path.Combine(TableBundles_Detail_Version_Folder_Path, "Catalog", "TableCatalog.json");
                    using (HttpClient httpclient = new HttpClient())
                    {
                        using (HttpResponseMessage response = await httpclient.GetAsync(TableCatalog_Bytes_Url))
                        {
                            if (!response.IsSuccessStatusCode)
                            {
                                throw new Exception($"HTTP 错误代码{response.StatusCode}");
                            }
                            var content = await response.Content.ReadAsStringAsync();
                            File_Services.Check_File_Path(TableCatalog_File_Path, false);
                            File.WriteAllText(TableCatalog_File_Path, content);
                            Log_Services.Print($"[INFO] TableCatalog.json 已保存到{TableCatalog_File_Path}");
                        }
                    }
                }

                byte[] bytesJson = File.ReadAllBytes(TableCatalog_File_Path);
                TableCatalog jsonData = JsonConvert.DeserializeObject<TableCatalog>(Encoding.UTF8.GetString(bytesJson)) ?? new TableCatalog();
                var tableFiles = jsonData.Table.Values.ToList();
                int Total_TableBundles_Num = tableFiles.Count;
                int Cur_TableBundles_Index = 0;
                int maxParallelDownloads = 10;
                Log_Services.Print($"[INFO] 开始下载共 {tableFiles.Count} 个 TableBundles 文件...");
                int startLine = Console.CursorTop; // 记录起始行

                using (var semaphore = new SemaphoreSlim(maxParallelDownloads))
                {
                    var tasks = tableFiles.Select(async tableItem =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            int index = Interlocked.Increment(ref Cur_TableBundles_Index);

                            string downloadUrl = $"{AddressablesCatalogUrlRoot}/TableBundles/{tableItem.Name}";
                            string localPath = Path.Combine(TableBundles_Detail_Version_Folder_Path, tableItem.Name);

                            if (tableItem.Includes != null)
                            {
                                string path = Path.GetDirectoryName(tableItem.Includes[0]) ?? "";
                                string[] segments = path.Split('\\');
                                for (int i = 0; i < segments.Length; i++) if (!string.IsNullOrEmpty(segments[i])) segments[i] = char.ToUpper(segments[i][0]) + segments[i].Substring(1).ToLower();
                                localPath = Path.Combine(TableBundles_Detail_Version_Folder_Path, string.Join("\\", segments), tableItem.Name);
                            }

                            SQL_Services.File_Info file_info = new SQL_Services.File_Info
                            {
                                FileName = tableItem.Name,
                                CAB = "NULL",
                                Type = 3,
                                Size = tableItem.Size,
                                CRC = tableItem.Crc,
                                Version = Detail_Version
                            };

                            if (SQL_Services.Add_GameData_Info(file_info, false))
                            {
                                File_Services.Check_Folder_Path(Path.GetDirectoryName(localPath) ?? "");
                                Log_Services.Print($"[INFO] 正在下载 {index.ToString($"D{Total_TableBundles_Num.ToString().Length}")}/{Total_TableBundles_Num} 个文件: {tableItem.Name}");
                                await Download_File(downloadUrl, localPath, 3, tableItem.Crc, tableItem.Size, Detail_Version);
                            }
                            else
                            {
                                Log_Services.Print($"[INFO] {index.ToString($"D{Total_TableBundles_Num.ToString().Length}")}/{Total_TableBundles_Num}: {tableItem.Name} 在数据库中已存在, 略过");
                            }
                        }
                        catch (Exception ex) { Log_Services.Print($"[ERROR] 下载文件 {tableItem.Name} 时出错: {ex.Message}"); }
                        finally { semaphore.Release(); }
                    }).ToList();
                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex) { Log_Services.Print(ex.Message); }
        }
    }
    public class BundleDownloadInfo
    {
        public List<BundleFile> BundleFiles { get; set; } = new List<BundleFile>();
        public class BundleFile
        {
            public string Name { get; set; } = "";
            public long Crc { get; set; }
            public long Size { get; set; }
        }
    }

    [MemoryPackable]
    public partial class MediaCatalog
    {
        public Dictionary<string, MediaCatalog_Per> Table = new Dictionary<string, MediaCatalog_Per>();
        [MemoryPackable]
        public partial class MediaCatalog_Per
        {
            public string Path { get; set; } = "";
            public string FileName { get; set; } = "";
            public long Bytes { get; set; }
            public long Crc { get; set; }
            public bool IsPrologue { get; set; }
            public bool IsSplitDownload { get; set; }
            public MediaType_ENUM MediaType { get; set; }
            public enum MediaType_ENUM
            {
                None,
                Audio,
                Video,
                Texture
            }
        }
    }
    [MemoryPackable]
    public partial class TableCatalog
    {
        public Dictionary<string, TableCatalog_Per> Table = new Dictionary<string, TableCatalog_Per>();
        [MemoryPackable]
        public partial class TableCatalog_Per
        {
            public string Name { get; set; } = "";
            public long Size { get; set; }
            public long Crc { get; set; }
            public bool isInbuild { get; set; }
            public bool isChanged { get; set; }
            public bool IsPrologue { get; set; }
            public bool IsSplitDownload { get; set; }
            public List<string> Includes { get; set; } = new List<string>();
        }
    }
}