# 太吾绘卷 Mod 开发模板与示范

本仓库是一个为《太吾绘卷》Mod 开发打造的模板与示范项目，旨在通过现代化的工具链和高度自动化的构建系统，解决传统 Mod 开发中的痛点，为开发者提供一个高效、稳定且易于协作的开发环境。

## 设计理念

这个项目不仅是一个 Mod 合集，更是一套经过精心设计的 **Mod 开发解决方案**，其核心设计思想在于：

- **标准化与自动化**：通过共享的 MSBuild 配置 (`.props`/`.targets`)，将 Mod 开发中的通用逻辑（如依赖打包、游戏 API 访问、项目引用）标准化，并实现构建过程的自动化。开发者因此可以专注于功能实现，而非繁琐的环境配置。

- **依赖隔离与稳定性**：每个 Mod 都是独立的项目，但其依赖的 `common` 库和部分 NuGet 包（如 `LF2.Transil`）会通过 `ILRepack` 在构建时自动内嵌到各自的程序集中。这从根本上解决了不同 Mod 之间因共享库版本冲突而导致的“DLL地狱”问题，确保了玩家环境的纯净与稳定。

- **强类型与开发效率**：利用 `Publicizer` 工具，我们将游戏内部的 `private` 和 `internal` API “公开化”，使得开发者可以在代码中直接、安全地调用这些非公开成员，并获得完整的智能提示（IntelliSense）和编译时类型检查。这极大地提升了开发效率，并减少了因拼写错误或 API 变更导致的运行时 Bug。

- **集中式依赖管理**：通过根目录的 **中央包管理 (CPM)** (`Directory.Packages.props`)，所有项目的 NuGet 依赖版本都在一个文件中统一定义。这确保了整个仓库中所有 Mod 和公共库使用的依赖版本一致，便于统一升级和维护。

## 目录结构

项目的结构经过精心组织，以实现关注点分离和最大化的代码复用。

```
.
├── Directory.Build.props           # MSBuild 全局属性，定义游戏库、UPM库等关键路径
├── Directory.Packages.props        # 中央包管理 (CPM)，统一定义所有 NuGet 依赖的版本
├── game-lib/                       # 存放游戏本体的程序集 (需手动复制)
├── global.json                     # 定义项目使用的 .NET SDK 版本
├── nuget.config                    # 配置额外的 NuGet 源 (本项目中为 GitHub Packages)
├── projects
│   ├── common/                     # 公共代码库，为所有 Mod 提供可复用的功能
│   │   ├── LF2.Backend.Helper/     # 后端 Mod 辅助功能
│   │   ├── LF2.Cecil.Helper/       # Mono.Cecil 辅助功能
│   │   ├── LF2.Frontend.Helper/    # 前端 Mod 辅助功能
│   │   ├── LF2.Game.Helper/        # 游戏通用辅助代码 (以源码方式共享)
│   │   └── LF2.Kit/                # 通用工具包
│   ├── mods/                       # 所有独立 Mod 的项目目录
│   │   ├── roll-protagonist/       # 真实 Mod 示例：开局Roll属性
│   │   │   ├── Config.Lua          # 游戏加载Mod所需的元数据配置文件
│   │   │   ├── mod.workflow.json   # CI 工作流配置，指定需要构建的项目
│   │   │   ├── RollProtagonist.Backend/  # 后端 Mod 项目 (C#)
│   │   │   ├── RollProtagonist.Common/   # 该 Mod 的前后端共享代码 (C#)
│   │   │   └── RollProtagonist.Frontend/ # 前端 Mod 项目 (C#)
│   │   └── ...
│   └── .editorconfig               # C# 代码风格配置
├── upm/                            # 存放 UPM (Unity Package Manager) 依赖 (需手动复制)
└── .vscode/                        # 推荐的 VSCode 开发配置
```

## 环境准备

以下步骤主要基于 VS Code 或其他命令行环境。

### 1. 安装 .NET SDK

确保已安装 `global.json` 文件中指定的 .NET SDK 版本 (`9.0.200` 或更高)。你可以使用 `dotnet --version` 命令检查当前版本。

### 2. 准备游戏文件

为了让 MSBuild 能够找到游戏的程序集 (DLLs)，你需要将编写 Mod 所需的游戏文件复制到指定目录。

- **前端依赖**: 从《太吾绘卷》游戏根目录下 `The Scroll of Taiwu_Data/Managed/` 文件夹中，复制你开发需要引用的 `.dll` 文件到本项目的 `game-lib/The Scroll of Taiwu_Data/Managed/` 目录中。
- **后端依赖**: 从《太吾绘卷》游戏根目录下 `Backend/` 文件夹中，复制你开发需要引用的 `.dll` 文件到本项目的 `game-lib/Backend/` 目录中。
- **UniTask 依赖**: 如果你的 Mod 需要 `UniTask`，请从 Unity 编辑器中获取对应的包，并将其复制到本项目的 `upm/UniTask/` 目录中。

### 3. 配置 GitHub NuGet 源

本项目依赖了托管在 GitHub Packages 上的 NuGet 包，因此 `dotnet` 在执行 `restore` 时需要通过身份验证。`nuget.config` 文件已预先配置为使用环境变量进行认证。

1.  **创建 Personal Access Token (PAT)**
    - 前往 GitHub 的 [Personal access tokens](https://github.com/settings/tokens) 页面。
    - 点击 **Generate new token (classic)**，仅需授予 `read:packages` 权限。
    - 生成并复制这个 Token，请妥善保管。

2.  **设置环境变量**
    在你的开发环境中设置以下两个环境变量。请将 `xxx_user` 替换为你的 GitHub 用户名，`xxx_pat` 替换为上一步创建的 PAT。

    - **Linux/macOS**:
      ```bash
      export GITHUB_USERNAME="xxx_user"
      export GITHUB_TOKEN="xxx_pat"
      ```

    - **Windows (PowerShell)**:
      ```powershell
      $env:GITHUB_USERNAME="xxx_user"
      $env:GITHUB_TOKEN="xxx_pat"
      ```
> **关于 Visual Studio 用户**
> 如果你使用 Visual Studio，可以通过其内置的 NuGet 包管理器 UI 来添加凭据，过程可能更简单。但其原理与设置环境变量是相同的：都是为了向 NuGet 提供有效的 GitHub 用户名和 PAT，以便通过身份验证并成功还原包。

### 4. 还原依赖与构建

完成以上步骤后，在项目根目录执行以下命令来还原 NuGet 包并构建所有 Mod：

```bash
dotnet build
```

构建产物将位于各个 Mod 项目的 `bin/` 目录下。

## 新增 Mod 开发流程

得益于本框架的自动化配置，创建一个新的 Mod 项目非常简单。

### 1. 创建目录和项目文件

- 在 `projects/mods/` 目录下，为你的新 Mod 创建一个主文件夹，例如 `MyNewMod`。
- 在 `MyNewMod` 文件夹内，根据你的需要创建项目文件夹，例如 `MyNewMod.Backend`。
- 在 `MyNewMod.Backend` 文件夹中，创建一个名为 `MyNewMod.Backend.csproj` 的项目文件。

### 2. 编辑项目文件

一个最基础的项目文件只需要包含 Sdk 声明：

```xml
<Project Sdk="Microsoft.NET.Sdk">
</Project>
```

**但这仅适用于不依赖任何 `common` 共享库的 Mod。**

在实际开发中，你很可能需要引用 `projects/common/` 目录下的各种辅助库。你可以通过添加 `<ProjectReference>` 来实现这一点。一个更真实的项目文件如下所示：

```xml
<Project Sdk="Microsoft.NET.Sdk">
    <ItemGroup>
        <!-- 引用此 Mod 自身的前后端共享项目 -->
        <ProjectReference Include="$(LF2Mods)/MyNewMod/MyNewMod.Common/MyNewMod.Common.csproj" />
        <!-- 引用 monorepo 中的公共辅助库 -->
        <ProjectReference Include="$(LF2Common)/LF2.Kit/LF2.Kit.csproj" />
        <ProjectReference Include="$(LF2Common)/LF2.Backend.Helper/LF2.Backend.Helper.csproj" />
    </ItemGroup>
</Project>
```

因为项目的文件路径和命名（例如以 `.Backend` 结尾）符合 `projects/mods/Directory.Build.props` 中定义的规则，构建系统会自动为你完成以下所有配置：

- **构建与打包**:
  - 设置正确的 .NET TargetFramework (`net6.0` 或 `netstandard2.1`)。
  - 配置 `ILRepack` 以便在构建时自动合并第三方依赖项。
- **游戏与公共库引用**:
  - 自动引用 `LF2.Transil` 等核心 NuGet 包。
  - 添加对相应游戏核心程序集（前端或后端）的引用。
  - 启用 `Publicizer` 来安全地访问游戏的内部 API。
  - 自动包含 `LF2.Game.Helper` 的源码。
- **代码质量与开发效率**:
  - 自动启用最新的 C# 语言特性，鼓励使用现代语法。
  - 默认开启一系列代码健壮性检查（如空引用分析）。
  - 统一应用仓库的代码风格、格式化规则，并在构建时进行检查。
  - 集成代码分析器，在开发过程中提供实时的辅助与重构建议。

### 3. 后续步骤

1.  **添加到解决方案**: 在你的 IDE 中（或通过 `dotnet sln add` 命令）将这个新项目添加到解决方案中，方便管理和编码。
2.  **添加代码**: 在项目中创建你的 C# 代码文件（例如 `ModEntry.cs`）。
3.  **开始开发**: 遵循游戏官方的 Mod 开发文档，开始编写你的 Mod 逻辑。


## MSBuild 构建系统详解

本仓库的强大之处在于其高度定制化的 MSBuild 构建流程。通过一系列 `*.props` 和 `*.targets` 文件，我们实现了开发、依赖和部署的自动化。

### 核心构建工具

- **[ILRepack.Lib.MSBuild.Task](https://github.com/ravibpatel/ILRepack.Lib.MSBuild.Task)**: 负责将项目引用的所有第三方 DLL（游戏自身的 DLL 除外）合并到最终生成的 Mod 程序集中。
  - 如果你希望某个特定的引用 **不被合并**，可以在 `.csproj` 文件中为对应的 `<Reference>` 或 `<PackageReference>` 添加元数据 `<LF2KeepItAsIs>true</LF2KeepItAsIs>`。此逻辑在 `projects/mods/ILRepack.targets` 中实现。

- **[Publicizer](https://github.com/krafs/Publicizer)**: 此工具能够让 C# 编译器像访问 `public` 成员一样访问程序集中的 `private` 和 `internal` 成员。在 `projects/mods/Directory.Build.props` 中，它被配置为自动 Publicize 前后端的多个核心游戏程序集。

### 关键 MSBuild 变量与逻辑

这些逻辑主要定义在 `projects/mods/Directory.Build.props` 中，是整个自动化构建的基石。

| 变量名 | 触发条件 | 作用 |
| :--- | :--- | :--- |
| `LF2IsBackend` | 项目名以 `.Backend` 结尾 | 1. 标识为后端项目。<br>2. 设置 `TargetFramework` 为 `net6.0`。<br>3. 自动引用 `game-lib/Backend/` 下的 DLL。 |
| `LF2IsFrontend` | 项目名以 `.Frontend` 结尾 | 1. 标识为前端项目。<br>2. 设置 `TargetFramework` 为 `netstandard2.1`。<br>3. 自动引用 `game-lib/The Scroll of Taiwu_Data/Managed/` 和 `upm/UniTask/` 下的 DLL。 |
| `LF2IsModEntry` | `LF2IsBackend` 或 `LF2IsFrontend` 为 `true` | 1. 标识为 Mod 入口项目。<br>2. 自动为项目添加 `ILRepack`、`Publicizer`、`LF2.Transil` 的包引用。<br>3. 自动包含 `LF2.Game.Helper` 的源码。 |
| `LF2KeepItAsIs` | 在 `.csproj` 中为程序集引用添加的元数据 | 在 `ILRepack` 阶段，防止被标记的程序集被合并进主 DLL，使其保持为独立文件。 |
