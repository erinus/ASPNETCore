# 一、學習操作容器
## Docker 安裝
1. Windows 10
   - Docker Desktop for Windows (use WSL2)
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
   - -v \<host-path\>:\<container-path\>  
     掛載本機目錄至容器
   - -w  
     指定容器預設工作目錄
   - --name \<name\>  
     指定容器名稱
   - <command>  
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
     47d8ccd08403        mcr.microsoft.com/dotnet/sdk:3.1   "tail -f /dev/null"      8 seconds ago       Up 7 seconds                                                           trusting_rhodes
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
> docker rm <container-id>
```
### 移除 Container
``` shell
> docker stop <container-id>
```
# 二、建立開發環境
## 檢查作業系統
1. Windows 10 Professional
2. ~~Ubuntu Desktop 18.04~~
3. ~~Mac OS X~~
## 安裝開發軟體
1. Visual Studio Code
   - C# Extension
   - Docker Extension
2. WSL2
3. Windows Terminal
   - oh-my-posh
   - gsudo
4. Git
5. HeidiSQL
6. Docker Desktop for Windows
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
   - 執行 View > Command Palette (Ctrl + Shift + P) > `Docker: Initialize for Docker debugging`，選擇 `.NET: ASP.NET Core`，再選擇 `Linux`
   - 執行 View > Run，選擇 `Docker .NET Core Launch`
## 了解 ASP.NET Core