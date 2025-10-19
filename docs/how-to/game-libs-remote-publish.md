# 使用 GitHub Actions 发布游戏依赖

当你已经准备好《太吾绘卷》的游戏程序集，需要把它们同步到团队可访问的私有 NuGet 源时，可按本流程操作。关于目录布局、FileCourier、离线备选方案等细节，请参阅参考文档《[游戏依赖打包参考手册](../reference/game-libs-packaging.md)》。

## 前置条件

- 已依据 manifest 整理好 `projects/unmanaged-vendor/game/` 目录，并压缩为单个 `.zip`（或确认有可下载的压缩包）。
- GitHub 仓库的 Secrets 中已配置目标包源的访问凭据。
- 了解你的私有源地址，稍后会写入 `nuget.config`。

## 操作步骤

1. **提供压缩包下载地址**  
   将整理好的压缩包上传到团队内部可访问的位置，并在仓库 Secrets 中设置或更新 `LF2_GAME_LIBS_URL`。

2. **触发工作流**  
   打开 GitHub `Actions`，选择 `Pack and Publish Game Libraries` 工作流，点击 `Run workflow`。  
   工作流会自动读取 `LF2_GAME_LIBS_URL` 指向的压缩包，无需在界面上填写下载链接。  
   - `Package Version`：填写希望发布的游戏依赖版本号。  
   - `Source`（可选）：如需推送到自定义 NuGet 源，填写目标源地址；留空则使用仓库所有者的 GitHub Packages。

3. **等待自动化执行**  
   工作流会下载压缩包、覆盖 `projects/unmanaged-vendor/game/` 目录、执行 `dotnet pack` 并推送包到目标私有源。依赖的打包策略与 reference 文档中描述的一致，无需手动操作。

4. **在本地消费包**  
   - 打开仓库根目录的 `nuget.config`，新增或启用一个指向私有源的 `<add>` 条目。  
   - 运行 `dotnet restore`，即可从私有源获取游戏依赖。

## 常见问题

- **需要回到离线方案？**  
  请按照《[离线环境下的游戏依赖准备](./game-libs-offline-setup.md)》执行，记得在完成后禁用私有源。

- **需要更新游戏版本？**  
  参考《[游戏依赖打包参考手册](../reference/game-libs-packaging.md#升级到新游戏版本)》中的版本升级流程。
