# 离线环境下的游戏依赖准备

当无法访问团队私有源或仅需在本机快速验证 Mod 时，可直接在仓库内打包并消费游戏依赖。详细背景、目录规范及更多可选项请参阅《[游戏依赖打包参考手册](../reference/game-libs-packaging.md)》。

## 适用场景

- 新加入项目但尚无私有源凭据。
- 构建机或演示环境受限，无法访问 GitHub Packages／内部源。
- 临时迭代或排查问题，不准备立即发布到远程源。

> [!NOTE]
> 本地方案与远程私有源方案互斥。启用本地源时请关闭远程源，避免混用；若需切换回远程方案，请参见《[使用 GitHub Actions 发布游戏依赖](./game-libs-remote-publish.md)》。

## 快速步骤

1. **整理 DLL（可选 FileCourier）**  
   按 `game-libs.manifest.yaml` 的映射将游戏 DLL 放入 `projects/unmanaged-vendor/game/<PackageId>/lib/`。如需自动分拣，可运行：

   ```bash
   ./FileCourier "<游戏安装目录>" "<输出目录>/game" -m game-libs.manifest.yaml
   ```

2. **本地打包**  
   在仓库根目录执行：

   ```bash
   dotnet pack ./projects/unmanaged-vendor/game/game.slnx -c Release
   ```

   打包结果写入 `.lf2.nupkg/`，供 `dotnet` 本地读取。

3. **启用本地源并恢复依赖**

   ```bash
   dotnet nuget enable source local
   dotnet restore
   ```

> [!TIP]
> 处理完离线任务后可运行 `dotnet nuget disable source local` 恢复到默认配置，不需要清理 `.lf2.nupkg/`。

执行完以上步骤后，Mod 工程便能在离线或受限网络环境中完成 `dotnet build` 与 `dotnet restore`。
