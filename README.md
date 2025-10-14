# 📜 太吾绘卷 Mod 开发模板与示范

本仓库是一个为《太吾绘卷》Mod 开发打造的模板与示范项目，旨在通过现代化的工具链和高度自动化的构建系统，为开发者提供一个高效、稳定且易于协作的开发环境。

## ✨ 核心特性

- ✅ **自动化项目配置**: 只需按约定命名并创建项目，框架便会自动配置目标平台、依赖引用等所有繁琐细节。
- ✅ **依赖自动内嵌**: 告别“DLL冲突”，你发布的 Mod 将是单个独立的程序集文件。
- ✅ **完整智能提示**: 游戏私有 API 也能获得完整的代码提示，如同调用原生函数般流畅自然。
- ✅ **一键式环境准备**: 仅需一个 `dotnet restore` 命令，即可从官方 NuGet 源和 GitHub Packages 自动拉取所有依赖。

## 🚀 快速上手

本章节将引导你完成从环境配置到创建第一个 Mod 的完整流程。

### 前置条件

- **.NET SDK**: 版本需满足 `global.json` 文件中的定义。
- **GitHub PAT**: 一个拥有 `read:packages` 权限的 [Personal Access Token](https://github.com/settings/tokens)。

### 1. ⚙️ 环境配置

本项目的依赖分为两部分：一部分来自官方 NuGet 源的标准库，另一部分是托管在 GitHub Packages 上的项目特有库。因此，你需要配置凭据以访问 GitHub Packages。

**VS Code / 命令行用户**:

1. 将你的 GitHub 用户名和 PAT 配置为环境变量 `GITHUB_USERNAME` 和 `GITHUB_TOKEN`。
2. 在项目根目录运行 `dotnet restore`。此命令将拉取所有必需的依赖。

> [!TIP]
> **Visual Studio 用户**: Visual Studio 会在打开解决方案时自动处理大部分依赖还原。你可能无需设置环境变量，只需在 `工具 > NuGet 包管理器 > 程序包管理器设置` 中添加 GitHub 包源并配置一次凭据即可。

> [!WARNING]
> 理想情况下，`dotnet restore` 能一键拉取所有依赖。但由于分发游戏文件存在法律风险，**本模板并未包含游戏核心库的在线包**。因此，在首次配置环境时，你可能会遇到 `NU1101` 等“包找不到”的错误。要解决此问题，你需要将这些缺失的依赖打包并推送至开发者私有的 NuGet 源。具体方法请参阅[管理非托管依赖](#-管理非托管依赖)章节。

### 2. 🎮 创建你的第一个 Mod

本框架遵循**约定优于配置**的理念，将创建新 Mod 的流程极大简化：

1. 在 `projects/mods/` 目录下为你的新 Mod 创建一个文件夹，例如 `MyNewMod`。
2. 在其中创建对应的项目文件夹，并遵循命名约定，例如 `MyNewMod.Backend`。
3. 在项目文件夹中，创建一个最简化的 C# 项目文件 `MyNewMod.Backend.csproj`：

    ```xml
    <Project Sdk="Microsoft.NET.Sdk">
    </Project>
    ```

完成！仅需遵循 `.Backend` 或 `.Frontend` 的命名约定，构建系统就会自动为你配置目标框架、引用所有游戏程序集、设置 `Publicizer` 与 `ILRepack` 等。你无需关心任何构建细节，可以立即开始编写 Mod 逻辑。

### 3. 💻 开始编码

现在，在你的项目文件夹中添加 C# 代码（如 `ModEntry.cs`），并遵循游戏官方的 Mod 开发文档编写逻辑即可。

## 🛠️ 进阶技巧

除了核心的自动化流程，本模板还提供了一些额外选项，以应对特殊场景。

### 📦 控制依赖内嵌

默认情况下，所有第三方依赖都会被内嵌到最终的 Mod 程序集中，以避免 DLL 冲突。但有时，你可能希望某个依赖**不被内嵌**，而是作为独立的 DLL 文件随 Mod 一同发布。在这种情况下，可以按以下方式操作：

- **对于 `<PackageReference>`**: 在 `.csproj` 文件中为对应的 `<PackageReference>` 添加元数据 `<LF2KeepItAsIs>true</LF2KeepItAsIs>` 和 `<GeneratePathProperty>true</GeneratePathProperty>`。

  **示例**:

  ```xml
  <!-- This package will NOT be merged into the main assembly. -->
  <PackageReference Include="LF2.UniTask" Version="*">
    <LF2KeepItAsIs>true</LF2KeepItAsIs>
    <GeneratePathProperty>true</GeneratePathProperty>
  </PackageReference>
  ```

- **对于 `<Reference>`**: 在 `.csproj` 文件中为对应的 `<Reference>` 添加元数据 `<LF2KeepItAsIs>true</LF2KeepItAsIs>`。

  **示例**:

  ```xml
  <!-- This reference will NOT be merged into the main assembly. -->
  <Reference Include="path/to/your/library.dll">
    <LF2KeepItAsIs>true</LF2KeepItAsIs>
  </Reference>
  ```

### 🔍 查阅构建变量

本项目的自动化构建依赖于一系列在各级 `Directory.Build.props` 文件中定义的 MSBuild 变量（如 `LF2IsBackend` 等）。

如果你需要进行深度定制（例如，在 `.csproj` 中添加自定义构建逻辑），可以直接查阅这些 `.props` 文件来了解所有可用的变量。

### 🎯 关于二进制依赖的范围

请注意，通过 NuGet 包（如 `LF2.Taiwu.Backend`）引用的游戏库是为适配当前仓库中的 Mod 而精心筛选的。如果你在开发中发现缺少某个游戏 API，则可能需要自行引用包含该 API 的其他游戏程序集。

### 📦 管理非托管依赖

正如“快速上手”章节所提到的，部分核心依赖未发布至任何公共 NuGet 源。为解决此问题，本模板提供了一套完整的非托管依赖解决方案，包括一个可复用的自动化工作流，用于将游戏库打包并发布到你自己的私有 NuGet 源。

关于此方案的**实施指南**、设计缘由以及相关风险，请参阅 [`unmanaged-vendor` 的专属文档](./projects/unmanaged-vendor/README.md)。

## 📚 参考资料

### 📁 目录结构

<details>
<summary>点击展开推荐的目录结构</summary>
<pre><code>.
├── Directory.Packages.props    # 全局 NuGet 包版本管理
├── projects/
│   ├── common/                 # 可供所有 Mod 复用的公共库
│   ├── mods/                   # 你的工作区：所有 Mod 项目都放在这里
│   │   └── MyNewMod/
│   │       ├── MyNewMod.Backend/
│   │       ├── MyNewMod.Frontend/
│   │       └── Config.Lua          # Mod 官方配置文件，建议纳入版本控制
│   └── unmanaged-vendor/       # 未托管资源的管理与打包配置
</code></pre>
</details>

本仓库的 `projects/mods/` 目录存放了数个 Mod 范例。详细列表与介绍请参阅该目录下的 [**README**](./projects/mods/README.md)。

### 🔩 核心工具

本模板的自动化功能主要由以下几个关键的开源工具驱动。感谢它们的开发者。

- **[ILRepack.Lib.MSBuild.Task](https://github.com/ravibpatel/ILRepack.Lib.MSBuild.Task)**: 负责将项目引用的所有第三方 DLL 合并到最终生成的 Mod 程序集中，解决“DLL地狱”问题。
- **[Publicizer](https://github.com/krafs/Publicizer)**: 此工具能够让 C# 编译器像访问 `public` 成员一样访问程序集中的 `private` 和 `internal` 成员，极大地提升了开发效率。

## 🤝 贡献与反馈

欢迎通过提交 Issue 或 Pull Request 来为本项目做出贡献。
