# 游戏依赖打包与发布

本文提供一份自建 NuGet 包源或在本地生成游戏依赖包的操作指南，聚焦于为《太吾绘卷》Mod 项目补齐官方游戏程序集的流程。

## 准备目录结构

所有待打包的 DLL 需遵循 `projects/unmanaged-vendor/game/` 下既有项目的目录布局：

```text
projects/unmanaged-vendor/
`-- game/
    |-- Taiwu.Backend/
    |-- Taiwu.Frontend/
    |-- Taiwu.Modding/
    `-- Taiwu.Patching/
```

- 将游戏原始 DLL 放进 `game/<PackageId>/lib/`，打包后会作为 Mod 编译期依赖提供。

> [!NOTE]
> 若需处理第三方或 UPM 依赖，请参考 [依赖管理操作指南](./dependency-management.md) 的相关章节；本文仅关注 Taiwu 官方程序集。

> [!TIP]
> 不需要在 `lib/` 下继续按目标框架拆分目录；现有的 MSBuild 配置会替你完成。

---

## 发布到私有源（推荐）

当团队具备可访问的远程包源时，建议使用仓库自带的 GitHub Actions 工作流来生成并发布 NuGet 包，步骤如下：

1. **准备压缩包**：按照上文的目录约定整理 `game/` 并压缩为单个 `.zip` 文件。
2. **配置机密**：在你的仓库 Secrets 中设置 `LF2_GAME_LIBS_URL`，存放该压缩包的下载地址。
3. **运行工作流**：在 GitHub `Actions` 页面手动触发 `Pack and Publish Game Libraries`，填写目标版本号及压缩包地址。
4. **等待发布完成**：工作流会自动完成解压、打包与推送，仅针对 `projects/unmanaged-vendor/` 下标记为可打包的工程生成 NuGet 包。

完成后，在仓库根目录的 `nuget.config` 中新增（或启用）一个指向你私有包源的 `<add>` 条目，并为其配置凭据。请保留默认的 `iplaylf2` 源，它被用于访问公开的仓库依赖。随后即可直接通过 `dotnet restore` 获取依赖。

---

## 本地打包（离线 / 临时方案）

若处于离线环境，或只需在本机快速验证 Mod，可先将游戏 DLL 拷贝到对应的 `projects/unmanaged-vendor/game/<PackageId>/lib/` 目录，然后在仓库根目录执行打包命令：

```bash
dotnet pack --no-restore -c Release
```

- `--no-restore` 可避免在没有网络的情况下触发额外的包还原。
- 若需要自定义包版本，可追加 `-p:Version=<your-version>`。
- 输出的 `.nupkg` 默认写入仓库根目录的 `.lf2.nupkg/`。该目录已在 `nuget.config` 中以 `local` 源名预注册，默认处于禁用状态；需要时执行 `dotnet nuget enable source local` 启用，使用完后可用 `dotnet nuget disable source local` 关闭。

---

## 升级到新游戏版本

1. 更新仓库根目录 `Directory.Build.props` 中的 `LF2TaiwuVersion`。
2. 用新版本 DLL 覆盖 `game/` 下的 `lib/` 文件。
3. 通过 GitHub Actions 工作流或本地 `dotnet pack` 重新生成 NuGet 包。
4. 在 Mod 项目上执行 `dotnet restore` 与一次 `dotnet build -t:LF2PublishMod`，确保依赖链在新版下仍能顺利编译。
