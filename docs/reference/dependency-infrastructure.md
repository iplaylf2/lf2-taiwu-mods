# 依赖基础设施

本文档说明游戏依赖管理的底层基础设施，包括包体系架构、构建系统和相关工具链。

## 包体系架构

### 设计原理

本模板将游戏自带的程序集按功能重新组织为语义化的 NuGet 包体系，实现从分散 DLL 文件到高内聚功能包的转换。

> **包用途说明**：详细的包分类和使用场景请参阅 [游戏依赖包说明](./game-dependencies.md)

### 命名约定

基于上述设计原理，包体系采用以下命名约定：

- **目录命名**：使用 `Taiwu.*` 前缀进行组织
- **包命名**：生成 `LF2.Taiwu.*` 前缀的 NuGet 包
- **映射关系**：`Taiwu.*` 目录名对应 `LF2.Taiwu.*` 包 ID

## 目录结构规范

### 标准布局

所有待打包的 DLL 需遵循 `projects/unmanaged-vendor/game/` 下的标准目录布局：

```text
projects/unmanaged-vendor/
`-- game/
    |-- Taiwu.Backend/lib/backend/
    |-- Taiwu.Frontend/lib/frontend/
    |-- Taiwu.Shared/lib/backend/
    |-- Taiwu.Shared/lib/frontend/
    |-- Taiwu.Modding/lib/backend/
    |-- Taiwu.Modding/lib/frontend/
    `-- Taiwu.BepInEx/lib/backend/
    `-- Taiwu.BepInEx/lib/frontend/
    `-- ... (其他包)
```

### 目录命名约定

- **`lib/backend`**：后端编译时依赖，包含 `Backend/` 目录下的游戏程序集
- **`lib/frontend`**：前端编译时依赖，包含 `The Scroll of Taiwu_Data/Managed/` 目录下的程序集

### 清单文件格式

[`game-libs.manifest.yaml`](../../projects/unmanaged-vendor/game/game-libs.manifest.yaml) 定义完整的文件映射关系：

```yaml
# 包说明注释
- target-dir: Taiwu.Backend/lib/backend  # 目标目录
  source-files:                          # 源文件列表
    - Backend/GameData.dll
    - Backend/GameData.pdb
```

该清单文件支持：

- 校对文件组织完整性
- 自动化脚本处理
- 构建系统集成

## 构建系统集成

### 自动包识别机制

构建系统通过检测 `Taiwu.*` 目录下的程序集类型自动选择项目模板：

- **仅包含 `lib/backend`**：使用 Backend 模板
- **仅包含 `lib/frontend`**：使用 Frontend 模板
- **同时包含两者**：使用 Common 模板

### 版本管理机制

游戏依赖包版本通过 `LF2TaiwuVersion` 属性统一管理，位于仓库根目录的 [`Directory.Build.props`](../../Directory.Build.props) 文件中。

升级游戏版本流程：

1. 更新 `LF2TaiwuVersion` 属性值
2. 替换对应目录下的 DLL 文件
3. 重新生成 NuGet 包

### 打包目标

`LF2PackGameLibs` 目标负责将所有标记为可打包的项目生成 NuGet 包：

- **输出目录**：`.lf2.nupkg/`
- **目标平台**：自动检测并适配
- **依赖管理**：仅编译时引用，不随 Mod 分发

## 工具链支持

### FileCourier 工具

FileCourier 是本仓库提供的跨平台文件分拣工具，支持基于 manifest 的自动化文件整理。

**技术规格**：

- 下载地址：[GitHub Releases](https://github.com/iplaylf2/lf2-taiwu-mods/releases)
- 配置文件：`game-libs.manifest.yaml`
- 命令格式：`./FileCourier <源目录> <目标目录> -m <manifest文件>`

> **详细说明**：参阅 [FileCourier 文档](../../projects/unmanaged-vendor/tools/FileCourier/README.md)

### 包源配置

系统支持两种包源模式：

- **远程源**：通过 GitHub Actions 自动发布到远程 NuGet 源
- **本地源**：使用 [`nuget.config`](../../nuget.config) 中预置的 `local` 源进行本地开发

## 法律合规性注意

本依赖管理方案涉及重新打包和分发游戏文件，请务必注意：

- **本仓库不托管任何游戏核心程序集**
- **重新打包的游戏依赖必须保持私有，仅限团队内部使用**
- **重新分发游戏文件可能涉及 EULA 合规性问题，开发者需自行评估并承担相应风险**

## 相关参考

- **[游戏依赖包说明](./game-dependencies.md)** - 了解可用包的分类和用途
- **[构建系统参考](./build-system.md)** - 深入了解 MSBuild 目标和自动化机制
- **[操作指南](../how-to/)** - 具体的设置和维护步骤
