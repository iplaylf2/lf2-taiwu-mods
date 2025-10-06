# 📜 太吾绘卷 Mod 开发模板与示范

本仓库是一个为《太吾绘卷》Mod 开发打造的模板与示范项目，旨在通过现代化的工具链和高度自动化的构建系统，为开发者提供一个高效、稳定且易于协作的开发环境。

## ✨ 设计理念

这个项目不仅是一个 Mod 合集，更是一套精心设计的 **Mod 开发解决方案**，其核心在于：

- **高度自动化**: 通过预设的构建流程，你只需按约定命名项目，即可自动完成依赖引用、API 解锁等繁琐配置，专注于 Mod 逻辑本身。
- **告别冲突**: 所有必需的依赖库都会在构建时自动内嵌到你的 Mod 程序集中，从根本上避免了与其他 Mod 的“DLL 冲突”。
- **强类型与智能提示**: 游戏内部 API 已被“公开化”，你可以直接、安全地调用，并享受完整的 IDE 智能提示和编译时检查，告别手写字符串和反射。
- **依赖版本统一**: 所有项目的 NuGet 依赖版本由根目录的 `Directory.Packages.props` 文件集中管理，确保版本一致，易于维护。

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
│   ├── mods/                       # 所有独立 Mod 的项目目录
│   │   ├── roll-protagonist/       # 真实 Mod 示例：开局“Roll”点
│   │   └── ...
│   └── .editorconfig               # C# 代码风格配置
├── upm/                            # 存放 UPM (Unity Package Manager) 依赖 (需手动复制)
└── .vscode/                        # 推荐的 VSCode 开发配置
```

## 🚀 快速上手

本章节将引导你完成从环境配置到创建第一个 Mod 的完整流程。

### 前置条件

- **.NET SDK**: 版本需满足 `global.json` 文件中的定义。
- **GitHub PAT**: 一个拥有 `read:packages` 权限的 [Personal Access Token](https://github.com/settings/tokens)。

### 1. 环境配置

本项目的依赖（游戏核心库、NuGet 包）都托管在 GitHub 上，你需要先完成身份认证。

- **VS Code / 命令行用户**:
  1.  将你的 GitHub 用户名和 PAT 配置为环境变量 `GITHUB_USERNAME` 和 `GITHUB_TOKEN`。
  2.  在项目根目录运行 `dotnet restore`。
      > **✨ 自动化魔法**
      > 此命令不仅会还原标准的 NuGet 包，还会自动从 GitHub Releases 下载并解压**本项目所需**的游戏核心库 (`game-lib`) 和 Unity 库 (`upm`)。

- **Visual Studio 用户**:
  > **💡 IDE 提示**
  > Visual Studio 会在打开解决方案时自动处理大部分依赖还原。你可能无需设置环境变量，只需在 `工具 > NuGet 包管理器 > 程序包管理器设置` 中添加 GitHub 包源并配置一次凭据即可。

### 2. 创建你的第一个 Mod

本框架遵循**约定优于配置**的理念。创建一个新 Mod 的过程被大大简化：

1.  在 `projects/mods/` 目录下为你的新 Mod 创建一个文件夹，如 `MyNewMod`。
2.  在其中创建对应的项目文件夹，并遵循命名约定，如 `MyNewMod.Backend`。
3.  在项目文件夹中，创建一个最简化的 C# 项目文件 `MyNewMod.Backend.csproj`：
    ```xml
    <Project Sdk="Microsoft.NET.Sdk">
    </Project>
    ```

完成！仅需遵循 `.Backend` 或 `.Frontend` 的命名约定，构建系统就会自动为你设定目标框架、引用所有游戏程序集、配置 `Publicizer` 与 `ILRepack` 等。你无需关心任何构建细节，可以立即开始编写 Mod 逻辑。

### 3. 开始编码

现在，在你的项目文件夹中添加 C# 代码（如 `ModEntry.cs`），并遵循游戏官方的 Mod 开发文档编写逻辑即可。

## 🛠️ 进阶技巧

除了核心的自动化流程，本模板还提供了一些额外的命令与选项，以应对特殊场景。

### 修复或强制还原游戏库

游戏库的版本与本仓库源码绑定。通常情况下，你无需手动干预。但如果本地的 `game-lib` 或 `upm` 目录因故损坏或缺失，你可以运行以下命令来强制重新下载和解压，以恢复它们：

```bash
dotnet build -t:LF2ForceRestoreBinaryDependencies
```

### 为 Fork 仓库配置依赖源

默认情况下，上述命令会从主仓库 `iplaylf2/lf2-taiwu-mods` 下载游戏库。如果你 Fork 了本项目并希望从你自己的仓库 Release 中下载依赖，你需要设置 `LF2_DEPS_REPO` 环境变量。

- **格式**: `owner/repo`
- **示例**: `export LF2_DEPS_REPO="MyGitHubUser/my-forked-repo"`

### 控制依赖内嵌

默认情况下，所有第三方依赖都会被合并到最终的 Mod 程序集中，以避免 DLL 冲突。如果你希望某个特定的引用**不被合并**，保持为独立文件，可以在 `.csproj` 文件中为对应的 `<Reference>` 或 `<PackageReference>` 添加元数据 `<LF2KeepItAsIs>true</LF2KeepItAsIs>`。

**示例**:
```xml
<ItemGroup>
  <!-- This reference will NOT be merged into the main assembly. -->
  <PackageReference Include="Some.Package" Version="1.2.3">
    <LF2KeepItAsIs>true</LF2KeepItAsIs>
  </PackageReference>
</ItemGroup>
```

### 查阅构建变量

本项目的自动化构建依赖于一系列在各级 `Directory.Build.props` 文件中定义的 MSBuild 变量（如 `LF2GameLibDir`, `LF2IsBackend` 等）。

如果你需要进行深度定制（例如，在 `.csproj` 中添加自定义的构建逻辑），可以直接查阅这些 `.props` 文件来了解所有可用的变量。

### 关于二进制依赖的范围

请注意，`game-lib` 中包含的游戏库是为适配当前仓库中的 Mod 而精心筛选的。如果你在开发自己的新 Mod 时，发现缺少某些游戏程序集的引用，你可能需要手动从游戏目录复制它们到 `game-lib` 中，并自行调整项目的 `<Reference>`。

## 🔩 核心工具

本模板的自动化功能主要由以下几个关键的开源工具驱动。感谢它们的开发者。

- **[ILRepack.Lib.MSBuild.Task](https://github.com/ravibpatel/ILRepack.Lib.MSBuild.Task)**: 负责将项目引用的所有第三方 DLL 合并到最终生成的 Mod 程序集中，解决“DLL地狱”问题。
- **[Publicizer](https://github.com/krafs/Publicizer)**: 此工具能够让 C# 编译器像访问 `public` 成员一样访问程序集中的 `private` 和 `internal` 成员，极大地提升了开发效率。