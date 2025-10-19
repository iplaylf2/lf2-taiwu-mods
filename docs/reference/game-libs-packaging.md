# 游戏依赖打包参考手册

本文提供打包《太吾绘卷》官方程序集所需的完整信息，涵盖目录规范、FileCourier 自动分拣、远程私有源发布以及离线/本地打包方案。How-to 文档会针对具体场景给出快速步骤，若需更细节的说明或想在两种方案之间切换，请以本手册为准。

## 目录结构与清单

所有待打包的 DLL 需遵循 `projects/unmanaged-vendor/game/` 下的目录布局。`projects/unmanaged-vendor/game/game-libs.manifest.yaml` 已按包 ID → 目标 `lib/` 目录列出完整清单，可直接照单整理。

```text
projects/unmanaged-vendor/
`-- game/
    |-- Taiwu.Backend/
    |-- Taiwu.Frontend/
    |-- Taiwu.Modding/
    `-- Taiwu.Patching/
```

- 将游戏原始 DLL 放进 `game/<PackageId>/lib/`，打包后会作为 Mod 编译期依赖提供。manifest 为这一布局的机器可读版本，适合用来校对或脚本化处理。
- 不需要在 `lib/` 下继续按目标框架拆分目录；现有的 MSBuild 配置会代你完成。

> [!NOTE]
> 若需处理第三方或 UPM 依赖，请参考 `../how-to/dependency-management.md` 的相关章节；本文聚焦 Taiwu 官方程序集。

### 使用 FileCourier 分拣文件（可选）

可从本仓库的 Release 页面下载与你平台匹配的 FileCourier 可执行文件，并与 `game-libs.manifest.yaml` 放在同一目录。随后直接运行：

```bash
./FileCourier "<游戏安装目录>" "<输出目录>/game" -m game-libs.manifest.yaml
```

命令会按照 manifest 自动复制所需文件，生成的 `game/` 目录即可用于压缩或放回仓库。

> [!NOTE]
> FileCourier 的定位与维护计划在 `../../projects/unmanaged-vendor/README.md` 中统一说明，试用前可先阅读 `#FileCourier-自动分拣工具` 章节获取最新信息。

---

## 通过 GitHub Actions 发布到私有源

当团队具备可访问的远程包源时，推荐使用仓库自带的 GitHub Actions 工作流来生成并发布 NuGet 包。

1. **准备压缩包**：依据 manifest 整理好 `game/` 目录后压缩为单个 `.zip` 文件；若已使用上一节的 FileCourier，直接压缩输出即可。
2. **配置机密**：在仓库 Secrets 中设置 `LF2_GAME_LIBS_URL`，存放该压缩包的下载地址。
3. **运行工作流**：在 GitHub `Actions` 页面手动触发 `Pack and Publish Game Libraries`，填写目标版本号（`Package Version`），若需覆盖默认推送目标可在 `Source` 字段填写自定义 NuGet 源；压缩包地址会从 `LF2_GAME_LIBS_URL` 机密中读取，无需手动输入。
4. **等待发布完成**：工作流会自动完成解压、打包与推送，仅针对 `projects/unmanaged-vendor/` 下标记为可打包的工程生成 NuGet 包。
5. **消费依赖**：在仓库根目录的 `nuget.config` 中新增（或启用）一个指向你私有包源的 `<add>` 条目，并为其配置凭据，然后执行 `dotnet restore`。

---

## 在离线或受限环境下本地打包

当无法使用远程私有源时，可在本地生成 NuGet 包并通过仓库已预置的 `local` 源供 `dotnet` 直接读取。

1. **整理 DLL**：按 `game-libs.manifest.yaml` 将文件放入 `projects/unmanaged-vendor/game/<PackageId>/lib/`。若不想手动整理，可复用 FileCourier。
2. **打包**：在仓库根目录运行：

   ```bash
   dotnet pack ./projects/unmanaged-vendor/game/game.slnx -c Release
   ```

   该命令会把所有 `unmanaged-vendor/game` 项目打包到 `.lf2.nupkg/` 目录。

3. **启用本地源**：执行 `dotnet nuget enable source local`，启用 `nuget.config` 中预置的本地源。
4. **恢复依赖**：运行 `dotnet restore`，NuGet 会从 `.lf2.nupkg/` 读取包。

> [!TIP]
> 使用完本地源后，可执行 `dotnet nuget disable source local` 关闭，切换回远程源无需额外清理本地包。

---

## 升级到新游戏版本

1. 更新仓库根目录 `Directory.Build.props` 中的 `LF2TaiwuVersion`。
2. 用新版本 DLL 覆盖 `game/` 下的 `lib/` 文件。
3. 通过 GitHub Actions 工作流或本地执行 `dotnet pack ./projects/unmanaged-vendor/game/game.slnx` 重新生成 NuGet 包。
4. 在 Mod 项目上执行 `dotnet restore` 与一次 `dotnet build -t:LF2PublishMod`，确认依赖链在新版下仍能顺利编译。
