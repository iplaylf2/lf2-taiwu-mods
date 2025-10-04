# 太吾绘卷 Mods

这是一个用于开发和管理《太吾绘卷》Mods 的 Monorepo 仓库。

本仓库旨在提供一个功能强大且易于维护的 Mod 开发环境，其核心特性包括：
- 使用 **中央包管理 (CPM)** 统一管理 NuGet 依赖。
- 通过 **自动化的程序集处理** 解决 Mod 间的依赖冲突，并提供对游戏内部 API 的强类型访问。


## 目录结构

```
.
├── Directory.Build.props           # MSBuild 全局属性，定义了游戏库等关键路径
├── Directory.Packages.props        # 中央包管理 (CPM)，定义所有 NuGet 依赖版本
├── game-lib                        # 存放游戏本体的程序集 (需要手动复制)
├── global.json                     # 定义项目使用的 .NET SDK 版本
├── nuget.config                    # 配置额外的 NuGet 源 (例如 GitHub Packages)
├── projects
│   ├── common                      # 公共代码库
│   │   ├── LF2.Backend.Helper      # 游戏后端辅助库
│   │   ├── LF2.Cecil.Helper        # Mono.Cecil 辅助库，用于 IL 编织
│   │   ├── LF2.Frontend.Helper     # 游戏前端辅助库
│   │   ├── LF2.Game.Helper         # 游戏通用辅助库
│   │   └── LF2.Kit                 # 通用工具库
│   ├── mods                        # Mod 项目目录
│   │   ├── any-mod                 # 示例 Mod 目录
│   │   │   ├── Config.lua          # Mod 的说明和配置文件
│   │   │   ├── mod.workflow.json   # Mod 的 CI 工作流元数据 (不影响本地使用)
│   │   │   ├── AnyMod.Backend      # 后端 Mod 项目
│   │   │   └── AnyMod.Frontend     # 前端 Mod 项目
│   │   └── ...
│   └── .editorconfig               # C# 代码风格配置
├── upm                             # 存放 UPM (Unity Package Manager) 依赖 (需要手动复制)
└── .vscode                         # 推荐的 VSCode 开发配置
    ├── extensions.json
    └── settings.json
```

## 环境准备与初始化

### 1. 安装 .NET SDK

确保已安装 `global.json` 文件中指定的 .NET SDK 版本。

### 2. 准备游戏文件

为了正常编译，你需要将游戏的部分程序集（DLL 文件）复制到 `game-lib` 目录中，并保持其原有的相对路径。

例如，将《太吾绘卷》游戏根目录下的 `The Scroll of Taiwu_Data/Managed` 文件夹中的所有 `.dll` 文件复制到本项目的 `game-lib/The Scroll of Taiwu_Data/Managed` 目录中。

### 3. 配置 GitHub NuGet 源

本项目依赖了一些托管在 GitHub Packages 上的公开 NuGet 包。即便这些包是公开的，`dotnet` 命令行工具在还原时也需要通过身份验证。

1.  **创建 Personal Access Token (PAT)**
    - 前往 GitHub 的 [Personal access tokens](https://github.com/settings/tokens) 页面。
    - 点击 **Generate new token (classic)**，授予 `read:packages` 权限。
    - 生成并复制这个 Token，请妥善保管。

2.  **还原依赖**
    在项目根目录执行以下命令，请将 `xxx` 替换为你的 GitHub 用户名和刚刚创建的 PAT。

    ```bash
    GITHUB_USERNAME="xxx" GITHUB_TOKEN="xxx" dotnet restore
    ```

### 4. 构建项目

完成以上步骤后，执行以下命令来构建所有 Mod：

```bash
dotnet build
```

构建产物将位于各个 Mod 项目的 `bin/` 目录下。

## MSBuild 构建系统说明

本仓库的构建过程由一系列 MSBuild `*.props` 和 `*.targets` 文件驱动，它们不仅自动化了大部分繁琐的工作，也简化了重复的开发与依赖配置。

### 核心构建工具

- **[ILRepack.Lib.MSBuild.Task](https://github.com/ravibpatel/ILRepack.Lib.MSBuild.Task)**: 此工具负责将项目引用的所有第三方 DLL（不包括游戏自身的 DLL）合并到最终生成的 Mod 程序集中。这极大地简化了 Mod 的分发，并从根本上解决了不同 Mod 之间因共享库版本不同而引发的冲突。
  - 如果你希望某个特定的引用不被合并，可以在 `.csproj` 文件中为对应的 `<Reference>` 或 `<PackageReference>` 添加元数据 `<LF2KeepItAsIs>true</LF2KeepItAsIs>`。
  - *示例*: `projects/mods/uni-task-support/UniTaskSupport.Frontend/UniTaskSupport.Frontend.csproj`

- **[Publicizer](https://github.com/krafs/Publicizer)**: 此工具的作用是让 C# 编译器能够像访问 `public` 成员一样访问程序集中的 `private` 和 `internal` 成员。在本项目中，它被用来处理游戏的核心库，使得我们可以在 Mod 代码中直接、安全地调用游戏内部的非公开 API，从而获得完整的智能提示和编译时类型检查，这对于提升开发效率和 Mod 稳定性至关重要。

### 主要 MSBuild 变量

这些变量定义在 `Directory.Build.props` 中，用于控制构建流程以及项目的工作环境。

| 变量名          | 说明                                                                                             |
| --------------- | ------------------------------------------------------------------------------------------------ |
| `LF2GameLib`    | 指向 `game-lib` 目录，用于引用游戏程序集。                                                       |
| `LF2Upm`        | 指向 `upm` 目录，用于引用 UPM 包。                                                               |
| `LF2Common`     | 指向 `projects/common` 目录，用于引用公共库项目。                                                |
| `LF2Mods`       | 指向 `projects/mods` 目录。                                                                      |
| `LF2IsBackend`  | 当项目位于 `mods` 目录下且项目名以 `.Backend` 结尾时，此属性自动为 `true`。                      |
| `LF2IsFrontend` | 当项目位于 `mods` 目录下且项目名以 `.Frontend` 结尾时，此属性自动为 `true`。                     |
| `LF2IsModEntry` | 当 `LF2IsBackend` 或 `LF2IsFrontend` 为 `true` 时，此属性自动为 `true`，用于标识 Mod 的入口项目。 |
| `LF2KeepItAsIs` | 在项目文件中设置为 `true` 时，可防止 `ILRepack` 合并指定的程序集。                               |