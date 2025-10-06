# 📜 太吾绘卷 Mod 开发模板与示范

本仓库是一个为《太吾绘卷》Mod 开发打造的模板与示范项目，旨在通过现代化的工具链和高度自动化的构建系统，解决传统 Mod 开发中的痛点，为开发者提供一个高效、稳定且易于协作的开发环境。

## ✨ 设计理念

这个项目不仅是一个 Mod 合集，更是一套经过精心设计的 **Mod 开发解决方案**，其核心设计思想在于：

- **🎯 标准化与自动化**：通过共享的 MSBuild 配置 (`.props`/`.targets`)，将 Mod 开发中的通用逻辑（如依赖打包、游戏 API 访问、项目引用）标准化，并实现构建过程的自动化。开发者因此可以专注于功能实现，而无需在繁琐的环境配置上耗费心力。

- **📦 依赖隔离与稳定性**：每个 Mod 都是独立的项目，但其依赖的 `common` 库和部分 NuGet 包（如 `LF2.Transil`）会通过 `ILRepack` 在构建时自动内嵌到各自的程序集中。这从根本上解决了不同 Mod 之间因共享库版本冲突而导致的“DLL地狱”问题，确保了玩家环境的纯净与稳定。

- **🔗 强类型与开发效率**：利用 `Publicizer` 工具，我们将游戏内部的 `private` 和 `internal` API “公开化”，使得开发者可以在代码中直接、安全地调用这些非公开成员，并获得完整的智能提示（IntelliSense）和编译时类型检查。这极大地提升了开发效率，并减少了因拼写错误或 API 变更导致的运行时 Bug。

- **📚 集中式依赖管理**：通过根目录的 **中央包管理 (CPM)** (`Directory.Packages.props`)，所有项目的 NuGet 依赖版本都在一个文件中统一定义。这确保了整个仓库中所有 Mod 和公共库使用的依赖版本一致，便于统一升级和维护。

## 📁 目录结构

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
│   │   ├── roll-protagonist/       # 真实 Mod 示例：开局“Roll”点
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

## 🚀 如何开始

开发环境的配置主要包含 **安装 .NET SDK** 和 **还原项目依赖** 两个步骤。

### 1. 安装 .NET SDK

首先，请确保已安装 `global.json` 文件中指定的 .NET SDK 版本（当前为 `9.0.200` 或更高）。你可以在终端中运行 `dotnet --version` 来检查。

### 2. 还原项目依赖

本项目的依赖分为两部分：从 GitHub Packages 下载的 **NuGet 包** 和存放于 GitHub Releases 的 **游戏核心库**。为了能顺利下载它们，你需要先配置 GitHub 身份认证。

1.  **配置 GitHub 认证**
    -   创建一个拥有 `read:packages` 权限的 [Personal Access Token (PAT)](https://github.com/settings/tokens)。
    -   将你的用户名和 PAT 配置为环境变量 `GITHUB_USERNAME` 和 `GITHUB_TOKEN`。

2.  **执行还原**
    完成认证配置后，在项目根目录运行以下命令：
    ```bash
    dotnet restore
    ```
    此命令会自动执行以下操作：
    -   **还原 NuGet 包**: 下载所有项目所需的 NuGet 包。
    -   **恢复二进制依赖**: 自动从 GitHub Releases 下载并解压 `game-lib` (游戏核心库) 和 `upm` (Unity 库)。此过程默认从主仓库 `iplaylf2/lf2-taiwu-mods` 下载，若需指定私有仓库，请设置 `LF2_DEPS_REPO` 环境变量 (格式为 `owner/repo`)。如果需要强制更新，可以运行 `dotnet build -t:LF2ForceRestoreBinaryDependencies`。

完成以上步骤后，你的开发环境便已就绪。

> **💡 Visual Studio 用户提示**
> 对于 Visual Studio 用户，大部分流程是自动化的：
> - **依赖还原**: 打开 `lf2-taiwu-mods.slnx` 解决方案时，Visual Studio 会自动执行 `restore` 操作。
> - **GitHub 认证**: 你可以在 `工具 > NuGet 包管理器 > 程序包管理器设置` 中添加 GitHub 包源并配置凭据，无需设置环境变量。

## ✨ 新增 Mod 开发流程

得益于本框架的自动化配置，创建一个新 Mod 的过程被大大简化。

### 1. 创建项目

首先，在 `projects/mods/` 目录下为你的新 Mod 创建一个文件夹（如 `MyNewMod`），然后在其中创建对应的项目文件夹（如 `MyNewMod.Backend`）。

在项目文件夹中，创建一个最简化的 C# 项目文件 `MyNewMod.Backend.csproj`：
```xml
<Project Sdk="Microsoft.NET.Sdk">
</Project>
```

### 2. 理解自动化配置

本框架的自动化配置建立在一个简单而强大的设计理念之上：**约定优于配置 (Convention over Configuration)**。

你无需维护复杂的配置文件，构建系统会根据你的 **项目命名** 自动推断其类型和需求。具体而言，仅需遵循简单的命名约定（如项目名以 `.Backend` 或 `.Frontend` 结尾），便能激活整套自动化流程。这一定位信号会自动触发一系列精密的配置，为你处理好所有繁琐的“脏活累活”，例如：

- **环境设定**：根据项目是前端 (`.Frontend`) 还是后端 (`.Backend`)，自动设置正确的 .NET **目标框架** (`netstandard2.1` 或 `net6.0`)。
- **依赖注入**：自动引用所有相关的 **游戏核心程序集** 和 **UPM 库**，无需手动添加 `Reference`。
- **API 解锁**：自动配置 **Publicizer**，将游戏内部的非公开 API（如 `Assembly-CSharp`、`GameData`）暴露给你，实现强类型安全调用。
- **核心工具集**：自动引入 `ILRepack`（依赖内嵌）、`LF2.Transil`（Harmony Patcher）等必不可少的开发工具。
- **公共代码共享**：自动将 `LF2.Game.Helper` 等通用辅助库的源码链接到你的项目中，实现代码复用。
- **代码风格统一**：自动集成 **Roslynator** 等代码分析器并开启构建时检查，确保所有代码遵循项目预设的 `.editorconfig` 规范，提升代码质量与一致性。

这种设计的核心在于，你只需要通过命名来“声明”你的意图，框架便会“响应”并完成所有标准化配置。这让你能完全专注于 Mod 的业务逻辑，而不必关心底层构建细节。

### 3. 开始编码

现在，你可以开始在项目中添加 C# 代码（如 `ModEntry.cs`），并遵循游戏官方的 Mod 开发文档编写逻辑了。


## ⚙️ MSBuild 构建系统详解

本仓库的强大之处在于其高度定制化的 MSBuild 构建流程。通过一系列 `*.props` 和 `*.targets` 文件，我们实现了开发与依赖管理的自动化。

### 🛠️ 核心构建工具

- **[ILRepack.Lib.MSBuild.Task](https://github.com/ravibpatel/ILRepack.Lib.MSBuild.Task)**: 负责将项目引用的所有第三方 DLL（游戏自身的 DLL 除外）合并到最终生成的 Mod 程序集中。
  - 如果你希望某个特定的引用 **不被合并**，可以在 `.csproj` 文件中为对应的 `<Reference>` 或 `<PackageReference>` 添加元数据 `<LF2KeepItAsIs>true</LF2KeepItAsIs>`。此逻辑在 `projects/mods/ILRepack.targets` 中实现。

- **[Publicizer](https://github.com/krafs/Publicizer)**: 此工具能够让 C# 编译器像访问 `public` 成员一样访问程序集中的 `private` 和 `internal` 成员。在 `projects/mods/Directory.Build.props` 中，它被配置为自动 Publicize 前后端的多个核心游戏程序集。

### 🔍 关键 MSBuild 变量与逻辑

这些逻辑主要定义在 `projects/mods/Directory.Build.props` 中，是整个自动化构建的基石。

| 变量名 🏷️ | 触发条件 ⚡ | 作用 🎯 |
| :--- | :--- | :--- |
| `LF2IsBackend` | 项目名以 `.Backend` 结尾 | 1. 标识为后端项目。<br>2. 设置 `TargetFramework` 为 `net6.0`。<br>3. 自动引用 `game-lib/Backend/` 下的 DLL。 |
| `LF2IsFrontend` | 项目名以 `.Frontend` 结尾 | 1. 标识为前端项目。<br>2. 设置 `TargetFramework` 为 `netstandard2.1`。<br>3. 自动引用 `game-lib/The Scroll of Taiwu_Data/Managed/` 和 `upm/UniTask/` 下的 DLL。 |
| `LF2IsModEntry` | `LF2IsBackend` 或 `LF2IsFrontend` 为 `true` | 1. 标识为 Mod 入口项目。<br>2. 自动为项目添加 `ILRepack`、`Publicizer`、`LF2.Transil` 的包引用。<br>3. 自动包含 `LF2.Game.Helper` 的源码。 |
| `LF2KeepItAsIs` | 在 `.csproj` 中为程序集引用添加的元数据 | 在 `ILRepack` 阶段，防止被标记的程序集被合并进主 DLL，使其保持为独立文件。 |