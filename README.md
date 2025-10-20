# 太吾绘卷 Mod 开发模板

本仓库汇总了在 Mod 开发实践中沉淀的通用模板与工作区，旨在通过现代化的工具链和高度自动化的构建系统，为开发者提供一个高效、稳定且易于协作的开发环境。

## 核心特性

- ✅ **自动化项目配置**：只需按约定命名并创建项目，框架便会自动配置目标平台、依赖引用等所有繁琐细节。
- ✅ **依赖自动内嵌**：告别“DLL 冲突”，你发布的 Mod 将是单个独立的程序集文件。
- ✅ **完整智能提示**：游戏私有 API 也能获得完整的代码提示，如同调用原生函数般流畅自然。
- ✅ **一键式环境准备**：仅需一个 `dotnet restore` 命令，即可从官方 NuGet 源和 GitHub Packages 自动拉取所有依赖。

## 快速上手

本章节将引导你完成从环境配置到创建第一个 Mod 的完整流程。

### 前置条件

- **.NET SDK**：版本需满足 `global.json` 文件中的定义。
- **GitHub PAT**：一个拥有 `read:packages` 权限的 [Personal Access Token](https://github.com/settings/tokens)。

### 1. 环境配置

本项目的依赖分为两部分：一部分来自官方 NuGet 源的标准库，另一部分是托管在 GitHub Packages 上的项目特有库。因此，你需要配置凭据以访问 GitHub Packages。

**VS Code / 命令行用户**：

1. 将你的 GitHub 用户名和 PAT 配置为环境变量 `GITHUB_USERNAME` 和 `GITHUB_TOKEN`。
2. 在项目根目录运行 `dotnet restore`。此命令将拉取所有必需的依赖。

> [!TIP]
> **Visual Studio 用户**：如果遇到凭据配置或 Release 编译报错等问题，请查阅专门的[环境配置指南](./docs/how-to/visual-studio-setup.md)。

> [!WARNING]
> 理想情况下，`dotnet restore` 能一键拉取所有依赖。但由于分发游戏文件存在法律风险，**本模板并未包含游戏核心库的在线包**。因此，在首次配置环境时，你可能会遇到 `NU1101` 等“包找不到”的错误。要解决此问题，你需要将这些缺失的依赖打包并推送至开发者私有的 NuGet 源。具体方法请参阅 [**非托管依赖项设置指南**](./projects/unmanaged-vendor/README.md)。

### 2. 创建你的第一个 Mod

本框架遵循**约定优于配置**的理念，将创建新 Mod 的流程极大简化：

1. 在 `projects/mods/` 目录下为你的新 Mod 创建一个文件夹，例如 `MyNewMod`。
2. 在其中创建对应的项目文件夹，并遵循命名约定，例如 `MyNewMod.Backend`。
3. 在项目文件夹中，创建一个最简化的 C# 项目文件 `MyNewMod.Backend.csproj`：

    ```xml
    <Project Sdk="Microsoft.NET.Sdk">
    </Project>
    ```

完成！仅需遵循 `.Backend` 或 `.Frontend` 的命名约定，构建系统就会自动为你配置目标框架、引用所有游戏程序集、设置 `Publicizer` 与 `ILRepack` 等。你无需关心任何构建细节，可以立即开始编写 Mod 逻辑。

### 3. 开始编码

现在，在你的项目文件夹中添加 C# 代码（如 `ModEntry.cs`），并遵循游戏官方的 Mod 开发文档编写逻辑即可。

### 4. 构建与发布

准备交付时，执行一次 `dotnet build -c Release -t:LF2PublishMod -p:LF2Mod=<mod-name>` 即可产出游戏可识别的目录结构（默认输出在 `.lf2.publish/<mod-name>/`）。更多自动化与高级用法见 [构建系统参考](./docs/reference/build-system.md)。

## 进一步阅读

- **[文档索引](./docs/README.md)**：浏览完整的文档分类，按需跳转到操作指南与参考资料。
- **[仓库内 Mod 导览](./projects/mods/README.md)**：查看目前包含的 Mod 列表、状态与源码路径。

## 贡献与反馈

欢迎通过提交 Issue 或 Pull Request 来为本项目做出贡献。
