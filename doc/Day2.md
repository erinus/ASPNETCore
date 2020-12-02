# Entity Framework Core
## 安裝套件
`存取 MySQL / MariaDB`
``` shell
> docker run --rm -it -v ${PWD}/src:/app -w /app mcr.microsoft.com/dotnet/sdk:3.1 dotnet add package MySql.Data -v 8.0.22
> docker run --rm -it -v ${PWD}/src:/app -w /app mcr.microsoft.com/dotnet/sdk:3.1 dotnet add package MySql.Data.EntityFrameworkCore -v 8.0.22
```
`讀取 appsettings.json`
``` shell
> docker run --rm -it -v ${PWD}/src:/app -w /app mcr.microsoft.com/dotnet/sdk:3.1 dotnet add package Microsoft.Extensions.Configuration.Json -v 3.1.10
```
## 問題說明
   - 如果發現引入套件後，程式可正常編譯執行，但在 Visual Studio Code 中，程式碼仍出現未引入套件或不存在函數之類的錯誤提示，請執行 View > Command Palette > `.NET: Restore Project`，使 OmniSharp 藉由還原套件過程重建組件類別資訊索引
   - 依據 [MySQL Entity Framework Core Support](https://dev.mysql.com/doc/connector-net/en/connector-net-entityframework-core.html) 指出 MySql.Data.EntityFrameworkCore 套件必須到 8.0.20 版才開始支援 .NET Core 3.1
