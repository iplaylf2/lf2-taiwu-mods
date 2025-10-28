# 构建系统参考

本页面汇总了仓库内与构建系统相关的核心技术信息，包括常用的 MSBuild 变量说明、关键工具介绍以及高级定制指南，帮助你在默认模板之外进行更深度的系统定制。

## MSBuild 构建变量详解

本项目的自动化构建系统依赖于一系列在各级 `Directory.Build.props` 文件中定义的 MSBuild 变量（如 `LF2IsBackend`、`LF2IsFrontend` 等）。这些变量是构建系统的核心配置元素，控制着项目的编译行为、目标平台设置以及依赖管理策略。

当你需要进行深度定制时，例如在 `.csproj` 文件中添加自定义构建逻辑，可以直接查阅这些 `.props` 文件来了解所有可用的变量及其具体作用。

## 核心构建目标文件

### 解决方案构建入口

**[`Directory.Solution.targets`](../../Directory.Solution.targets)** - 解决方案级别的构建入口，按顺序导入关键的构建脚本：

```xml
<Import Project="$(LF2ModsDir)/LF2Mod.targets" />
<Import Project="$(LF2UnmanagedVendorDir)/game/LF2GameLib.targets" />
```

### 游戏依赖包构建

**[`LF2GameLib.targets`](../../projects/unmanaged-vendor/game/LF2GameLib.targets)** - 游戏依赖包构建的核心脚本，定义了 `LF2PackGameLibs` 目标，实现了从 `Taiwu.*` 目录到 `LF2.Taiwu.*` NuGet 包的自动化转换。

该脚本具备**智能项目选择**能力：根据目录中包含的程序集类型（backend/frontend）自动选择合适的项目模板进行打包。

### Mod 发布机制

**[`LF2Mod.targets`](../../projects/mods/LF2Mod.targets)** - Mod 打包发布的核心脚本，定义了 `LF2PublishMod` 目标，负责：

- 编译 Mod 项目的所有组件（Backend/Frontend/Common）
- 整理资源文件和程序集到标准目录结构
- 生成可分发的 Mod 包到 `.lf2.publish/` 目录

### 程序集合并机制

**[`ILRepack.targets`](../../projects/mods/ILRepack.targets)** - ILRepack 工具的集成脚本，实现了复杂的依赖合并逻辑：

- **智能依赖分析**：自动识别需要保留的外部依赖（通过 `LF2KeepItAsIs` 标记）
- **并行合并**：使用 `Parallel="true"` 提升合并性能
- **内部化处理**：将依赖项的公共类型内部化，避免命名冲突
- **自动清理**：合并完成后自动清理中间文件

该脚本是确保 Mod 独立运行、避免"DLL 地狱"问题的关键组件。

## 处理缺失的游戏依赖

需要说明的是，虽然本模板通过 [`game-libs.manifest.yaml`](../../projects/unmanaged-vendor/game/game-libs.manifest.yaml) 提供的依赖包已经覆盖了绝大部分开发场景，但并未包含游戏目录下的全部程序集。这种设计是为了保持依赖管理的精简和高效。

当你在开发过程中发现缺少某个游戏 API 时，这通常意味着其所在的程序集尚未被纳入打包清单。面对这种情况，**正确的处理方式**是：

1. 更新 `game-libs.manifest.yaml` 文件
2. 将缺失的程序集添加到打包列表中
3. 重新生成 NuGet 包

**强烈不建议**在项目文件中直接通过文件路径引用 DLL，因为这种做法会绕过模板提供的统一依赖管理机制，可能导致版本不一致和其他维护问题。

关于如何更新清单文件并重新打包的操作步骤，请参阅相关操作指南。技术规格请参阅 [依赖基础设施](./dependency-infrastructure.md)。

## Mod 打包使用指南

**使用 `LF2Mod.targets` 提供的 `LF2PublishMod` 目标**：

### 基本用法

```bash
dotnet build -c Release -t:LF2PublishMod -p:LF2Mod=<mod-name>
```

其中 `<mod-name>` 是你的 Mod 项目文件夹名称（不包含 `.Backend` 或 `.Frontend` 后缀），例如对于 `MyMod.Backend` 项目，应使用 `MyMod`。

### Release 模式的重要性

使用 `-c Release` 参数是触发 `ILRepack` 工具执行的关键条件。在 `Release` 模式下，构建系统会自动执行以下重要操作：

- 自动将所有必要的依赖项 DLL 合并到你的 Mod 主程序集中
- 生成一个完全独立的程序集文件，不会与其他 Mod 产生依赖冲突
- 确保分发的 Mod 具有最佳的兼容性和稳定性

### 输出结果

执行命令后，系统会同时编译前端与后端插件，并将生成的完整 Mod 包写入仓库根目录的 `.lf2.publish/` 目录中。这种设计便于你：

- 直接将内容复制到游戏的 Mods 目录进行测试
- 作为后续发版操作的基础文件
- 确保打包结果的目录结构符合游戏标准

## Git 标签与自动化发版

当需要产出可分发的压缩包时，可以通过 Git 标签来触发 `.github/workflows/push-to-mod-tag.yaml` 中配置的自动化发版流程。这种机制能够实现标准化的版本管理和自动化的发布流程。

### 标签格式规范

标签格式必须遵循 `mods/<mod-name>/v<version>` 的固定模式，例如：

```bash
git tag mods/my-mod/v1.2.0
git push origin mods/my-mod/v1.2.0
```

### 自动化流程说明

推送标签后，GitHub Actions 工作流会自动执行以下操作：

1. **解析标签信息**：从标签中提取 Mod 名称和版本号
2. **调用构建目标**：使用 `LF2PublishMod` 目标生成 `.lf2.publish/<mod-name>/` 下的完整产物
3. **打包发布**：将产物打包为 `<mod-name>_v<version>.zip` 格式的压缩包
4. **创建 Release**：自动发布到 GitHub Release，方便用户直接下载

## 核心工具链介绍

本模板的自动化功能依赖于以下几个优秀的开源工具，它们是整个构建系统的技术基石：

### ILRepack.Lib.MSBuild.Task

- **功能定位**：程序集合并工具
- **核心价值**：负责将项目引用的所有第三方 DLL 合并到最终生成的 Mod 程序集中，从根本上解决"DLL 地狱"问题
- **项目地址**：[ILRepack.Lib.MSBuild.Task](https://github.com/ravibpatel/ILRepack.Lib.MSBuild.Task)

### Publicizer

- **功能定位**：访问权限扩展工具
- **核心价值**：让 C# 编译器能够像访问 `public` 成员一样访问程序集中的 `private` 和 `internal` 成员，极大地提升了 Mod 开发的便利性和效率
- **项目地址**：[Publicizer](https://github.com/krafs/Publicizer)

衷心感谢这些工具的开发者为开源社区做出的贡献。
