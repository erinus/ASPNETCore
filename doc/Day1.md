# 一、學習操作容器
## Docker 安裝
1. Windows 10
   - Docker Desktop for Windows (use WSL2)
     - Windows Hypervisor Platform
     - Windows Subsystem for Linux
   - ~~Docker Desktop for Windows (use Hyper-V)~~
2. ~~Linux~~
3. ~~Mac~~
## Docker 概念
1. Image  
   將已安裝設定好的系統硬碟檔，透過 overlayfs 檔案系統可以達成差異備份、分層疊加及重複使用等功能
2. Registry  
   提供將各種系統硬碟檔進行保管並提供存取及版本控管的服務
3. Container  
   將系統硬碟檔交給本機系統核心獨立運行
## Docker 操作
### 從 DockerHub 找尋官方發佈 Image
1. 開啟 DockerHub 官網，搜尋 .NET SDK
2. 找到 .NET SDK
3. 尋找 Tags 取得可用的 Image 版本  
   建議只用 `VERIFIED PUBLISHER` 或 `OFFICIAL IMAGE`
4. 從 DockerHub 下載 Image
``` shell
> docker pull <repository>:<tag>
```
範例：下載 .NET Core SDK 3.1
``` shell
> docker pull mcr.microsoft.com/dotnet/sdk:3.1

# 執行過程輸出
3.1: Pulling from dotnet/sdk
756975cb9c7e: Pull complete
d77915b4e630: Pull complete
5f37a0a41b6b: Pull complete
96b2c1e36db5: Extracting [=======================>                           ]  24.12MB/51.83MB
51cf9699e842: Download complete
9e754a1351ff: Downloading [======================>                            ]  55.13MB/124MB
573fc7ecf76b: Verifying Checksum

# 執行完成輸出
3.1: Pulling from dotnet/sdk
756975cb9c7e: Pull complete
d77915b4e630: Pull complete
5f37a0a41b6b: Pull complete
96b2c1e36db5: Pull complete
51cf9699e842: Pull complete
9e754a1351ff: Pull complete
573fc7ecf76b: Pull complete
Digest: sha256:9393c7199cdc260e16417debe91dbb6615e58d5c9520122f8bfbbe56eaa02fca
Status: Downloaded newer image for mcr.microsoft.com/dotnet/sdk:3.1
mcr.microsoft.com/dotnet/sdk:3.1
```
### 列出 Image 清單
``` shell
> docker images

REPOSITORY                     TAG                 IMAGE ID            CREATED             SIZE
mcr.microsoft.com/dotnet/sdk   3.1                 f5db59f77c51        14 hours ago        711MB
```
### 移除 Image
``` shell
> docker rmi f5db59f77c51

Untagged: mcr.microsoft.com/dotnet/sdk:3.1
Untagged: mcr.microsoft.com/dotnet/sdk@sha256:9393c7199cdc260e16417debe91dbb6615e58d5c9520122f8bfbbe56eaa02fca
Deleted: sha256:f5db59f77c5126e65ec50adc078f2cf78f1b9ed437f6d542d9d547f1e1f594ef
Deleted: sha256:2671713ef626f998edce80663ae30056ad8c71785788f5b3cb67e3c70c1fe63d
Deleted: sha256:fe7297b3ddce1744d44b49ae9e749a18916c2a26286de8a5b21c67097ff969f5
Deleted: sha256:bc5478d0ea4978b60ae0a47e4c1fbf45de1419152c3e30601996ba8c27c7f9da
Deleted: sha256:be048ca7e7af3c225efaf44d9bf81c2d274d0860e563cd97e5f2630c958f24cd
Deleted: sha256:e93ebf51004cfe34370a8930dc7016c067a8c5cd4754b932dedca1739f0c6e29
Deleted: sha256:86569a54d5d4735f27748c0e5d9ead04f48d36bbaaeaa755a1e022a6eb32d650
Deleted: sha256:114ca5b7280f3b49e94a67659890aadde83d58a8bde0d9020b2bc8c902c3b9de
```
### 將 Image 運行為 Container
``` shell
> docker run --rm -it -d -e <key>=<value> -p <host-port>:<container-port> -v <host-path>:<container-path> -w <working-directory> --name <name> <repository>:<tag> <command>
```
1. 參數說明
   - --rm  
     執行結束後自動刪除容器
   - -it  
     提供互動式操作終端機
   - -d  
     指定背景執行容器
   - -e \<key\> = \<value\>  
     指定環境變數
   - -p \<host-port\>:\<container-port\>  
     指定埠號對應  
     容器內埠號映射至本機埠號
   - -v \<host-path\>:\<container-path\>   
     指定目錄對應  
     掛載本機目錄至容器
   - -w  
     指定容器預設工作目錄
   - --name \<name\>  
     指定容器名稱
   - \<command\>  
     容器運行後執行的命令
2. 執行方式
   - 有使用 --rm  
     做為單次執行容器使用；通常是為了透過容器進行簡單的建置、編譯或產出檔案等流程
     ``` shell
     > docker run --rm -it mcr.microsoft.com/dotnet/sdk:3.1 dotnet --version

     3.1.404
     ```
   - 不使用 --rm 保留容器做為後續重複使用；通常是因為容器運行後還需要進行不少設定，為避免重複浪費每次容器執行後需要的準備時間，於是保留容器以利下次使用
     ``` shell
     > docker run -it -d mcr.microsoft.com/dotnet/sdk:3.1 tail -f /dev/null

     47d8ccd084030fa254a2afc2f41c8365232f212fb92f689acfeaff415e6fc136

     ❯ docker ps
     CONTAINER ID        IMAGE                              COMMAND                  CREATED             STATUS              PORTS                                              NAMES
     47d8ccd08403        mcr.microsoft.com/dotnet/sdk:3.1   "tail -f /dev/null"      8 seconds ago       Up 7 seconds                                                           xxxxx_yyyyy
     ```
### 透過已運行的 Container 執行指令
``` shell
> docker exec -it <container-id> <command>
```
``` shell
> docker exec -it 47d8ccd08403 dotnet --version

3.1.404
```
### 啟動 Container
``` shell
> docker start <container-id>
```
### 停止 Container
``` shell
> docker stop <container-id>
```
### 移除 Container
``` shell
> docker rm <container-id>
```
# 二、建立開發環境
## 檢查作業系統
1. Windows 10 Professional
2. ~~Ubuntu Desktop 18.04~~
3. ~~Mac OS X~~
## 安裝開發軟體
1. 安裝 [.NET Core SDK 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)
2. Visual Studio Code
   - C# Extension
   - Docker Extension
3. WSL2
4. Windows Terminal
   - oh-my-posh
     提供 Powershell 樣式
   - gsudo  
     提供 Windows Terminal 可用系統管理員身份執行
5. Git
6. HeidiSQL
7. Docker Desktop for Windows
   - Enable WSL2 Integration
     Settings > Resources > WSL Integration
## 準備容器影像
### 下載容器影像
從 DockerHub 上的 .NET Core SDK 官方頁面按照指示下載
``` shell
> docker pull mcr.microsoft.com/dotnet/sdk:3.1
```
### 建立測試專案
執行以下指令後會在 `src` 目錄下出現 `Controllers`, `Properties`, ... 目錄，以及 `app.csproj`, `appsettings.json`, `appsettings.Development.json`, `Program.cs`, `Startup.cs`, ... 檔案
``` shell
> docker run --rm -it -v ${PWD}/src:/app -w /app mcr.microsoft.com/dotnet/sdk:3.1 dotnet new webapi

Getting ready...
The template "ASP.NET Core Web API" was created successfully.

Processing post-creation actions...
Running 'dotnet restore' on /app/app.csproj...
  Determining projects to restore...
  Restored /app/app.csproj (in 205 ms).

Restore succeeded.
```
### 測試專案運作
``` shell
> docker run --rm -it -v ${PWD}/src:/app -w /app -p 5000:5000 mcr.microsoft.com/dotnet/sdk:3.1 dotnet watch run --urls "http://+:5000"

watch : Polling file watcher is enabled
watch : Started
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://[::]:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /app
```
開啟瀏覽器連線 http://localhost:5000/WeatherForecast 測試
### 產生開發憑證
執行以下指令後會在 `src` 目錄下出現 `dev.pfx` 憑證檔案
``` shell
> docker run --rm -it -v ${PWD}/src:/app mcr.microsoft.com/dotnet/sdk:3.1 dotnet dev-certs https --export-path /app/dev.pfx
```
### 安裝憑證檔案
``` shell
> CERTUTIL -addstore -enterprise -f -v root src/dev.pfx
```
### 測試憑證運作
``` shell
> docker run --rm -it -v ${PWD}/src:/app -w /app -p 5000:5000 -p 5001:5001 mcr.microsoft.com/dotnet/sdk:3.1 dotnet watch run --urls "http://+:5000;https://+:5001"

watch : Polling file watcher is enabled
watch : Started
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://[::]:5000
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: https://[::]:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /app
```
開啟瀏覽器連線 https://localhost:5001/WeatherForecast 測試
### 設定開發軟體
1. Visual Studio Code
   - 執行 View > Command Palette (Ctrl + Shift + P) > `.NET: Generate Assets for Build and Debug`，然後會看到專案下出現 `.vscode` 目錄，內有 `launch.json` 和 `tasks.json`
   - 執行 View > Command Palette (Ctrl + Shift + P) > `Docker: Add Docker Files to Workspace...`，選擇 `.NET: ASP.NET Core`，再選擇 `Linux`，提示使用 `80, 443` 二個埠號，按下 `[ENTER]` 確認，最後選擇 `No`
   - 執行 View > Command Palette (Ctrl + Shift + P) > `Docker: Initialize for Docker debugging`，選擇 `.NET: ASP.NET Core`，再選擇 `Linux`
   - 右下角提示 `Docker launch configurations and/or tasks already exist. Do you want to overwrite them?`，選擇 `Overwrite`
   - 執行 View > Run (Ctrl + Shift + D)，選擇 `Docker .NET Core Launch`
   - 執行 Run > Start Debugging (F5)
   - 右下角提示 `The ASP.NET Core HTTPS development certificate is not trusted. To trust the certificate, run "dotnet dev-certs https --trust", or click "Trust" below.`，選擇 `Trust`，並同意安裝憑證
## ASP.NET Core
### 指令操作
1. 查詢指令
   ``` shell
   > dotnet -h
   ```
   ``` shell
   > docker run --rm -it -v ${PWD}/src:/app -w /app mcr.microsoft.com/dotnet/sdk:3.1 dotnet -h

   .NET Core SDK (3.1.404)
   Usage: dotnet [runtime-options] [path-to-application] [arguments]

   Execute a .NET Core application.

   runtime-options:
     --additionalprobingpath <path>   Path containing probing policy and assemblies to probe for.
     --additional-deps <path>         Path to additional deps.json file.
     --fx-version <version>           Version of the installed Shared Framework to use to run the application.
     --roll-forward <setting>         Roll forward to framework version  (LatestPatch, Minor, LatestMinor, Major, LatestMajor, Disable).

   path-to-application:
     The path to an application .dll file to execute.

   Usage: dotnet [sdk-options] [command] [command-options] [arguments]

   Execute a .NET Core SDK command.

   sdk-options:
     -d|--diagnostics  Enable diagnostic output.
     -h|--help         Show command line help.
     --info            Display .NET Core information.
     --list-runtimes   Display the installed runtimes.
     --list-sdks       Display the installed SDKs.
     --version         Display .NET Core SDK version in use.

   SDK commands:
     add               Add a package or reference to a .NET project.
     build             Build a .NET project.
     build-server      Interact with servers started by a build.
     clean             Clean build outputs of a .NET project.
     help              Show command line help.
     list              List project references of a .NET project.
     msbuild           Run Microsoft Build Engine (MSBuild) commands.
     new               Create a new .NET project or file.
     nuget             Provides additional NuGet commands.
     pack              Create a NuGet package.
     publish           Publish a .NET project for deployment.
     remove            Remove a package or reference from a .NET project.
     restore           Restore dependencies specified in a .NET project.
     run               Build and run a .NET project output.
     sln               Modify Visual Studio solution files.
     store             Store the specified assemblies in the runtime package store.
     test              Run unit tests using the test runner specified in a .NET project.
     tool              Install or manage tools that extend the .NET experience.
     vstest            Run Microsoft Test Engine (VSTest) commands.

   Additional commands from bundled tools:
     dev-certs         Create and manage development certificates.
     fsi               Start F# Interactive / execute F# scripts.
     sql-cache         SQL Server cache command-line tools.
     user-secrets      Manage development user secrets.
     watch             Start a file watcher that runs a command when files change.

   Run 'dotnet [command] --help' for more information on a command.
   ```
2. 建立專案
   ``` shell
   > dotnet new <template>
   ```
3. 新增套件
   ``` shell
   # 安裝最新版本套件
   > dotnet add package <package-name>
   # 安裝指定版本套件
   > dotnet add package <package-name> -v <package-version>
   ```
   ``` shell
   > docker run --rm -it -v ${PWD}/src:/app -w /app mcr.microsoft.com/dotnet/sdk:3.1 dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson -v 3.1.10

     Determining projects to restore...
     Writing /tmp/tmpBQox0c.tmp
   info: Adding PackageReference for package 'Microsoft.AspNetCore.Mvc.NewtonsoftJson' into project '/app/app.csproj'.
   info: Restoring packages for /app/app.csproj...
   info:   GET https://api.nuget.org/v3-flatcontainer/microsoft.aspnetcore.mvc.newtonsoftjson/index.json
   info:   OK https://api.nuget.org/v3-flatcontainer/microsoft.aspnetcore.mvc.newtonsoftjson/index.json 724ms
   info:   GET https://api.nuget.org/v3-flatcontainer/microsoft.aspnetcore.mvc.newtonsoftjson/3.1.10/microsoft.aspnetcore.mvc.newtonsoftjson.3.1.10.nupkg
   info:   OK https://api.nuget.org/v3-flatcontainer/microsoft.aspnetcore.mvc.newtonsoftjson/3.1.10/microsoft.aspnetcore.mvc.newtonsoftjson.3.1.10.nupkg 732ms
   info:   GET https://api.nuget.org/v3-flatcontainer/microsoft.aspnetcore.jsonpatch/index.json
   info:   GET https://api.nuget.org/v3-flatcontainer/newtonsoft.json/index.json
   info:   GET https://api.nuget.org/v3-flatcontainer/newtonsoft.json.bson/index.json
   info:   OK https://api.nuget.org/v3-flatcontainer/microsoft.aspnetcore.jsonpatch/index.json 717ms
   info:   GET https://api.nuget.org/v3-flatcontainer/microsoft.aspnetcore.jsonpatch/3.1.10/microsoft.aspnetcore.jsonpatch.3.1.10.nupkg
   info:   OK https://api.nuget.org/v3-flatcontainer/newtonsoft.json/index.json 761ms
   info:   GET https://api.nuget.org/v3-flatcontainer/newtonsoft.json/12.0.2/newtonsoft.json.12.0.2.nupkg
   info:   OK https://api.nuget.org/v3-flatcontainer/newtonsoft.json.bson/index.json 765ms
   info:   GET https://api.nuget.org/v3-flatcontainer/newtonsoft.json.bson/1.0.2/newtonsoft.json.bson.1.0.2.nupkg
   info:   OK https://api.nuget.org/v3-flatcontainer/microsoft.aspnetcore.jsonpatch/3.1.10/microsoft.aspnetcore.jsonpatch.3.1.10.nupkg 722ms
   info:   GET https://api.nuget.org/v3-flatcontainer/microsoft.csharp/index.json
   info:   OK https://api.nuget.org/v3-flatcontainer/newtonsoft.json.bson/1.0.2/newtonsoft.json.bson.1.0.2.nupkg 720ms
   info:   OK https://api.nuget.org/v3-flatcontainer/newtonsoft.json/12.0.2/newtonsoft.json.12.0.2.nupkg 741ms
   info:   OK https://api.nuget.org/v3-flatcontainer/microsoft.csharp/index.json 730ms
   info:   GET https://api.nuget.org/v3-flatcontainer/microsoft.csharp/4.7.0/microsoft.csharp.4.7.0.nupkg
   info:   OK https://api.nuget.org/v3-flatcontainer/microsoft.csharp/4.7.0/microsoft.csharp.4.7.0.nupkg 712ms
   info: Installing Microsoft.CSharp 4.7.0.
   info: Installing Microsoft.AspNetCore.JsonPatch 3.1.10.
   info: Installing Newtonsoft.Json 12.0.2.
   info: Installing Newtonsoft.Json.Bson 1.0.2.
   info: Installing Microsoft.AspNetCore.Mvc.NewtonsoftJson 3.1.10.
   info: Package 'Microsoft.AspNetCore.Mvc.NewtonsoftJson' is compatible with all the specified frameworks in project '/app/app.csproj'.
   info: PackageReference for package 'Microsoft.AspNetCore.Mvc.NewtonsoftJson' version '3.1.10' added to file '/app/app.csproj'.
   info: Committing restore...
   info: Writing assets file to disk. Path: /app/obj/project.assets.json
   log : Restored /app/app.csproj (in 6.81 sec).
   ```
4. 刪除套件
   ``` shell
   > dotnet remove package <package-name>
   ```
   ``` shell
   > docker run --rm -it -v ${PWD}/src:/app -w /app mcr.microsoft.com/dotnet/sdk:3.1 dotnet remove package Microsoft.AspNetCore.Mvc.NewtonsoftJson

   info: Removing PackageReference for package 'Microsoft.AspNetCore.Mvc.NewtonsoftJson' from project '/app/app.csproj'.
   ```
5. 執行應用
   ``` shell
   > dotnet run
   > dotnet run --urls <urls>
   ```
   ``` shell
   > docker run --rm -it -v ${PWD}/src:/app -w /app -p 5000:5000 -p 5001:5001 mcr.microsoft.com/dotnet/sdk:3.1 dotnet run --urls "http://+:5000;https://+:5001"

   info: Microsoft.Hosting.Lifetime[0]
         Now listening on: http://[::]:5000
   info: Microsoft.Hosting.Lifetime[0]
         Now listening on: https://[::]:5001
   info: Microsoft.Hosting.Lifetime[0]
         Application started. Press Ctrl+C to shut down.
   info: Microsoft.Hosting.Lifetime[0]
         Hosting environment: Development
   info: Microsoft.Hosting.Lifetime[0]
         Content root path: /app
   ```
5. 恢復套件
   ``` shell
   > dotnet restore
   ```
   ``` shell
   > docker run --rm -it -v ${PWD}/src:/app -w /app mcr.microsoft.com/dotnet/sdk:3.1 dotnet restore

     Determining projects to restore...
     All projects are up-to-date for restore.
   ```
6. 執行應用（監看模式）
   ``` shell
   > dotnet watch run
   > dotnet watch run --urls <urls>
   ```
   ``` shell
   > docker run --rm -it -v ${PWD}/src:/app -w /app -p 5000:5000 -p 5001:5001 mcr.microsoft.com/dotnet/sdk:3.1 dotnet watch run --urls "http://+:5000;https://+:5001"

   watch : Polling file watcher is enabled
   watch : Started
   info: Microsoft.Hosting.Lifetime[0]
         Now listening on: http://[::]:5000
   info: Microsoft.Hosting.Lifetime[0]
         Now listening on: https://[::]:5001
   info: Microsoft.Hosting.Lifetime[0]
         Application started. Press Ctrl+C to shut down.
   info: Microsoft.Hosting.Lifetime[0]
         Hosting environment: Development
   info: Microsoft.Hosting.Lifetime[0]
         Content root path: /app
   info: Microsoft.Hosting.Lifetime[0]
         Application is shutting down...
   watch : Exited
   watch : File changed: /app/Program.cs
   watch : Started
   info: Microsoft.Hosting.Lifetime[0]
         Now listening on: http://[::]:5000
   info: Microsoft.Hosting.Lifetime[0]
         Now listening on: https://[::]:5001
   info: Microsoft.Hosting.Lifetime[0]
         Application started. Press Ctrl+C to shut down.
   info: Microsoft.Hosting.Lifetime[0]
         Hosting environment: Development
   info: Microsoft.Hosting.Lifetime[0]
         Content root path: /app
   ```
7. 建置應用
   ``` shell
   > dotnet build
   > dotnet build -c <configuration>
   ```
   ``` shell
   > docker run --rm -it -v ${PWD}/src:/app -w /app mcr.microsoft.com/dotnet/sdk:3.1 dotnet build

   Microsoft (R) Build Engine version 16.7.1+52cd83677 for .NET
   Copyright (C) Microsoft Corporation. All rights reserved.

     Determining projects to restore...
     All projects are up-to-date for restore.
     app -> /app/bin/Debug/netcoreapp3.1/app.dll

   Build succeeded.
       0 Warning(s)
       0 Error(s)

   Time Elapsed 00:00:02.89

   > docker run --rm -it -v ${PWD}/src:/app -w /app mcr.microsoft.com/dotnet/sdk:3.1 dotnet build -c Release

   Microsoft (R) Build Engine version 16.7.1+52cd83677 for .NET
   Copyright (C) Microsoft Corporation. All rights reserved.

     Determining projects to restore...
     All projects are up-to-date for restore.
     app -> /app/bin/Release/netcoreapp3.1/app.dll

   Build succeeded.
       0 Warning(s)
       0 Error(s)

   Time Elapsed 00:00:04.12
   ```
8. 發佈應用
   ``` shell
   > dotnet publish -c <configuration>
   ```
   ``` shell
   > docker run --rm -it -v ${PWD}/src:/app -w /app mcr.microsoft.com/dotnet/sdk:3.1 dotnet publish -c Release

   Microsoft (R) Build Engine version 16.7.1+52cd83677 for .NET
   Copyright (C) Microsoft Corporation. All rights reserved.

     Determining projects to restore...
     Restored /app/app.csproj (in 234 ms).
     app -> /app/bin/Release/netcoreapp3.1/app.dll
     app -> /app/bin/Release/netcoreapp3.1/publish/
   ```
### 基礎架構
#### `Program.cs`
``` csharp
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
```
1. CreateDefaultBuilder  
   引入要啟用的服務及組態設定等，並在 Host 建立前進行前置設定
2. UseStartup  
   指定 WebHostBuilder 在建立 WebHost 過程中，提供開發者指定介入啟動設定的類別 Startup
3. Build
   產生 Host 實體
4. Run  
   執行 Host
``` csharp
public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseIIS()
                        .UseIISIntegration()
                        .UseKestrel()
                        .UseStartup<Startup>()
                        .UseUrls(new string[] { "http://+:5000", "https://+:5001" })
                        .UseWebRoot(@"/var/www");
                });
    }
```
1. UseIIS  
   以 OutOfProcess 模式運行在 IIS (w3wp.exe) 外部，接收 IIS 轉發過來的請求，處理完後回應給 IIS，由 IIS 再回傳給請求方  
   ![OutOfProcess Model](https://weblog.west-wind.com/images/2019/ASP.NET-Core-Hosting-on-IIS-with-ASP.NET-Core-2.2/OutOfProcessIISHosting.png)
2. UseIISIntegration  
   以 InProcess 模式運行在 IIS (w3wp.exe) 內部，接收 IIS 轉送過來的請求，處理完後由 IIS 再回傳給請求方  
   ![InProcess Model](https://weblog.west-wind.com/images/2019/ASP.NET-Core-Hosting-on-IIS-with-ASP.NET-Core-2.2/InProcessHostingDiagram.png)
3. UseKestrel
   以獨立模式（Self-Host）運行，且可跨平台運行的網頁伺服器  
   Used as Edge Server  
   ![Used as Edge Server](https://docs.microsoft.com/zh-tw/aspnet/core/fundamentals/servers/kestrel/_static/kestrel-to-internet2.png?view=aspnetcore-5.0)  
   Used in Reverse Proxy  
   ![Used in Reverse Proxy](https://docs.microsoft.com/zh-tw/aspnet/core/fundamentals/servers/kestrel/_static/kestrel-to-internet.png?view=aspnetcore-5.0)
4. UseUrls  
   指定要監聽的 host 和 port
5. UseWebRoot
   指定本應用根目錄 WebRoot
#### `Startup.cs`
``` csharp
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddResponseCaching();

        services.AddControllers();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCors();

        app.UseResponseCaching();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```
1. UseHttpsRedirection  
   強制將 HTTP 連線轉向 HTTPS 連線
2. UseRouting  
   加入路由中介層
3. UseCors  
   加入跨域請求支援，位置必須置於 UseRouting 後，在 UseResponseCaching 前
4. UseResponseCaching  
   加入回應快取支援
5. UseAuthentication  
   加入認證機制，判定身份
6. UseAuthorization（3.0 新增）  
   加入授權機制，判定使用行為權限
### 建立 API
#### `Models\Stock.cs`
``` csharp
using System;

namespace app.Models
{
    public class Stock
    {
        // 日期
        public DateTime Date { get; set; }

        // 股票代碼
        public string Code { get; set; }

        // 開盤價
        public double Open { get; set; }

        // 最高價
        public double High { get; set; }

        // 最低價
        public double Low { get; set; }

        // 收盤價
        public double Close { get; set; }
    }
}
```
#### `Controllers\StockController.cs`
``` csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace app.Controllers
{
    using app.Models;

    [ApiController]
    [Route("[controller]")]
    public class StockController : ControllerBase
    {
        private readonly ILogger<StockController> _logger;

        public StockController(ILogger<StockController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public Stock Get()
        {
            var rng = new Random();
            return new Stock
            {
                Date = DateTime.Now.Date,
                Code = "2330",
                Open = 490.1,
                High = 491.1,
                Low = 489.1,
                Close = 490.1
            };
        }
    }
}
```
開啟瀏覽器連線 https://localhost:5001/Stock 測試，取得 JSON 回應如下
``` json
{
    "date":"2020-12-01T00:00:00+00:00",
    "code":"2330",
    "open":490.1,
    "high":491.1,
    "low":489.1,
    "close":490.1
}
```