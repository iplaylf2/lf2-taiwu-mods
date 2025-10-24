# 本地源方案操作指南

当无法访问团队远程源或仅需在本机快速验证 Mod 时，可直接在仓库内打包并消费游戏依赖。本指南提供完整的本地打包操作步骤。

## 适用场景

- 新加入项目但尚无远程源凭据。
- 构建机或演示环境受限，无法访问 GitHub Packages／内部源。
- 临时迭代或排查问题，不准备立即发布到远程源。

> [!NOTE]
> 本地方案与远程源方案互斥。启用本地源时请关闭远程源，避免混用；若需切换回远程方案，请参见 [非托管供应商依赖管理 - 远程源方案](../../projects/unmanaged-vendor/README.md#方案一远程源方案推荐)。

## 操作指南

### 第一步：整理游戏 DLL 文件

根据 [`game-libs.manifest.yaml`](../../projects/unmanaged-vendor/game/game-libs.manifest.yaml) 中的映射关系[^1]，将游戏程序集放置到对应目录。

#### 选项一：手动整理

1. 根据清单文件中的映射关系整理文件
2. 将游戏 DLL 放置到 `<临时目录>/game/<PackageId>/lib/` 目录[^2]
3. 确保目录结构符合标准布局

完成后，将生成的 `game/` 目录用于后续操作。

#### 选项二：使用 FileCourier 自动整理（推荐）

1. 从 [GitHub Releases](https://github.com/iplaylf2/lf2-taiwu-mods/releases) 下载对应平台的 FileCourier 可执行文件[^3]
2. 将可执行文件与 `game-libs.manifest.yaml` 放在同一目录
3. 运行命令：

   ```bash
   ./FileCourier "<游戏安装目录>" "<临时输出目录>/game" -m game-libs.manifest.yaml
   ```

FileCourier 会自动按照 manifest 复制所需文件并生成正确的目录结构。完成后，将生成的 `game/` 目录用于后续操作。

### 第二步：本地打包

将生成的 `game/` 目录复制到 `projects/unmanaged-vendor/` 下。

在仓库根目录执行打包命令：

```bash
dotnet build ./projects/unmanaged-vendor/game/game.slnx -c Release -t:LF2PackGameLibs
```

该命令会扫描所有项目并生成 NuGet 包[^4]。

### 第三步：启用本地 NuGet 源

启用预配置的本地 NuGet 源：

```bash
dotnet nuget enable source local
```

然后恢复项目依赖：

```bash
dotnet restore
```

此时 NuGet 会从 `.lf2.nupkg/` 目录读取本地生成的包。

## 验证成功

执行完所有步骤后，可以通过以下方式验证配置成功：

1. **检查包恢复**：`dotnet restore` 应该无错误完成
2. **验证编译**：`dotnet build` 应该能正常编译 Mod 项目
3. **检查依赖**：在 Visual Studio 或 VS Code 中查看项目依赖是否正确引用

执行完以上步骤后，Mod 工程便能完全使用本地生成的依赖包完成 `dotnet build` 与 `dotnet restore`。

> [!TIP]
> **进阶指南**
> 关于在不同 NuGet 源之间切换、版本管理、故障排除等更多内容，请参阅 [《NuGet 源管理指南》](./nuget-source-management.md)。

## 相关资源

- **[远程源方案](../../projects/unmanaged-vendor/README.md#方案一远程源方案推荐)** - 团队协作的远程发布方案
- **[游戏依赖包说明](../reference/game-dependencies.md)** - 各依赖包的详细用途与引用场景

## 参考资料

[^1]: 清单文件定义了游戏 DLL 文件到包目录的映射规则，是自动化文件整理的核心配置。详细格式说明请参阅：[游戏依赖包技术规格 - 清单文件格式](../reference/game-libs-packaging.md#清单文件格式)

[^2]: `<PackageId>/lib/` 目录结构遵循本仓库的包体系设计，区分 backend 和 frontend 两种目标框架。详细规范请参阅：[游戏依赖包技术规格 - 目录命名约定](../reference/game-libs-packaging.md#目录命名约定)

[^3]: FileCourier 是本仓库提供的跨平台文件分拣工具，支持基于 manifest 的自动化文件整理。详细了解其功能请参阅：[FileCourier 工具文档](../../projects/unmanaged-vendor/tools/FileCourier/README.md)

[^4]: `LF2PackGameLibs` 是构建系统提供的打包目标，能够自动识别项目类型并生成对应的 NuGet 包。详细机制请参阅：[游戏依赖包技术规格 - 打包目标](../reference/game-libs-packaging.md#打包目标)
