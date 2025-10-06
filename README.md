# 📜 太吾绘卷 Mod 开发模板与示范

本仓库是一个为《太吾绘卷》Mod 开发打造的模板与示范项目，旨在通过现代化的工具链和高度自动化的构建系统，为开发者提供一个高效、稳定且易于协作的开发环境。

## ✨ 核心特性

- ✅ **自动化项目配置**: 创建项目并按约定命名，框架会自动**完成**目标平台、依赖引用等所有繁琐配置。
- ✅ **依赖自动内嵌**: 告别“DLL冲突”，你的 Mod 发布时会是一个独立的纯净文件。
- ✅ **完整智能提示**: 游戏私有 API 也拥有完整的代码提示，像调用官方函数一样顺滑。
- ✅ **一键式环境准备**: 只需一个 `dotnet restore` 命令，即可自动备齐所有游戏库和开发库。

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

## 📚 参考资料

### 📁 目录结构

<details>
<summary>点击展开推荐的目录结构</summary>
<pre><code>.
├── Directory.Build.props       # 自动化核心：定义全局构建属性
├── Directory.Packages.props    # 统一管理所有项目的NuGet包版本
├── game-lib/                   # (自动下载) 游戏核心程序集
├── upm/                        # (自动下载) Unity核心程序集
├── projects/
│   ├── common/                 # 公共库项目，可供所有Mod复用
│   └── mods/                   # 你的工作区：所有Mod项目都放在这里
│       └── MyNewMod/
│           ├── MyNewMod.Backend/   # Mod后端项目 (遵循.Backend命名约定)
│           │   └── MyNewMod.Backend.csproj
│           ├── MyNewMod.Frontend/  # Mod前端项目 (遵循.Frontend命名约定)
│           │   └── MyNewMod.Frontend.csproj
│           └── Config.Lua
</code></pre>
</details>

### 🔩 核心工具

本模板的自动化功能主要由以下几个关键的开源工具驱动。感谢它们的开发者。

- **[ILRepack.Lib.MSBuild.Task](https://github.com/ravibpatel/ILRepack.Lib.MSBuild.Task)**: 负责将项目引用的所有第三方 DLL 合并到最终生成的 Mod 程序集中，解决“DLL地狱”问题。
- **[Publicizer](https://github.com/krafs/Publicizer)**: 此工具能够让 C# 编译器像访问 `public` 成员一样访问程序集中的 `private` 和 `internal` 成员，极大地提升了开发效率。

## 🤝 贡献与反馈

欢迎通过提交 Issue 或 Pull Request 来为本项目做出贡献。