# 依赖管理操作指南

本指南汇总了围绕“依赖内嵌与分发”这一主题的常见任务，适用于需要精细控制打包行为或引入第三方库的场景。游戏官方程序集的打包细节已整理为《[游戏依赖打包参考手册](../reference/game-libs-packaging.md)》，如需准备 Taiwu 官方 DLL，请优先阅读该文档。

## 控制依赖内嵌

默认情况下，所有第三方依赖都会被内嵌到最终的 Mod 程序集中，以避免 DLL 冲突。但有时，你可能希望某个依赖**不被内嵌**，而是作为独立的 DLL 文件随 Mod 一同发布。在这种情况下，可以按以下方式操作：

- **对于 `<PackageReference>`**：在 `.csproj` 文件中为对应的 `<PackageReference>` 添加元数据 `<LF2KeepItAsIs>true</LF2KeepItAsIs>` 和 `<GeneratePathProperty>true</GeneratePathProperty>`。

  **示例**：

  ```xml
  <!-- This package will NOT be merged into the main assembly. -->
  <PackageReference Include="LF2.UniTask" Version="*">
    <LF2KeepItAsIs>true</LF2KeepItAsIs>
    <GeneratePathProperty>true</GeneratePathProperty>
  </PackageReference>
  ```

- **对于 `<Reference>`**：在 `.csproj` 文件中为对应的 `<Reference>` 添加元数据 `<LF2KeepItAsIs>true</LF2KeepItAsIs>`。

  **示例**：

  ```xml
  <!-- This reference will NOT be merged into the main assembly. -->
  <Reference Include="path/to/your/library.dll">
    <LF2KeepItAsIs>true</LF2KeepItAsIs>
  </Reference>
  ```

## 打包第三方库（以 UPM 为例）

部分库，尤其是为 Unity UPM 设计的库，没有官方 NuGet 包，但你的 Mod 在运行时又需要它们。对于这类库，你可以将它们打包为标准的 NuGet 包，使其 DLL 文件作为运行时依赖随 Mod 一同发布。

- **目录示例**：`unmanaged-vendor/upm/UniTask/lib/`
- **依赖示例**：`UniTask.dll`

> [!NOTE]
> 本仓库提供的 `pack-and-publish-game-libs` 自动化工作流**不处理**此类第三方库的打包与发布。这类库通常需要开发者自行获取其 DLL，并手动发布到私有源。
> 此外，请确保你使用的 UPM 包（或其 DLL）是针对与游戏兼容的 Unity 版本编译的，以避免运行时发生兼容性错误。

> [!TIP]
> 为避免不同 Mod 之间因携带不同版本的库而产生冲突，强烈建议使用 `ILRepack` 等工具将这类运行时依赖内嵌到你的主程序集中（这也是本模板的默认行为）。

## 通过物料覆盖进行深度定制

> 本节内容与 `projects/unmanaged-vendor/README.md` 中的自动化工作流相关。

自动化工作流在执行 `dotnet pack projects/unmanaged-vendor/game/game.slnx` 之前，会先将用户提供的游戏库压缩包解压并覆盖到 `projects/unmanaged-vendor/game` 目录。这个“覆盖”行为是可供高级用户利用的干预点。

除了基础的 DLL 文件外，你还可以在压缩包内遵循 `game` 目录的结构，附带任何你想覆盖的文件——例如，修改过的 `.csproj` 文件，甚至是 C# 源码（`.cs` 文件），从而在不修改 Git 仓库本身的情况下，实现对打包过程的深度定制。

> [!WARNING]
> 此功能非常强大，但也是一把双刃剑，推荐仅用于快速原型验证或个人实验。对于更通用的流程改进或 Bug 修复，通过代码（而非物料覆盖）来解决，是更可持续、更便于协作的工程实践。

善用以上三个工具箱，可以覆盖大部分“依赖如何被打包、发布和加载”的问题。遇到特殊情况时，先回顾你的目标是**调整打包行为**还是**引入额外依赖**，再选择对应的章节执行即可。
