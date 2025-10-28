# 依赖管理操作指南

本指南汇总了围绕"依赖内嵌与分发"这一核心主题的常见操作任务，特别适用于需要精细控制打包行为或引入第三方库的开发场景。

关于游戏官方程序集的打包细节，请参阅 [依赖基础设施](../reference/dependency-infrastructure.md)。如果需要准备《太吾绘卷》官方 DLL，请参考相关操作指南。

## 控制依赖内嵌行为

在 `Release` 构建模式下，构建系统默认会将所有第三方依赖内嵌到最终的 Mod 程序集中，这种设计能够有效避免 DLL 冲突问题。然而，在某些特定场景下，你可能希望某个依赖保持独立，不进行内嵌处理，而是作为单独的 DLL 文件随 Mod 一起发布。

针对这种情况，提供以下两种控制方式：

### 方式一：控制 PackageReference 依赖

对于通过 NuGet 引用的包，在 `.csproj` 文件中为对应的 `<PackageReference>` 添加两项关键元数据：

```xml
<!-- This package will NOT be merged into the main assembly. -->
<PackageReference Include="LF2.UniTask">
  <LF2KeepItAsIs>true</LF2KeepItAsIs>
  <GeneratePathProperty>true</GeneratePathProperty>
</PackageReference>
```

### 方式二：控制 Reference 依赖

对于直接引用的 DLL 文件，在 `.csproj` 文件中为对应的 `<Reference>` 添加元数据：

```xml
<!-- This reference will NOT be merged into the main assembly. -->
<Reference Include="path/to/your/library.dll">
  <LF2KeepItAsIs>true</LF2KeepItAsIs>
</Reference>
```

## 打包第三方库（以 UPM 为例）

在实际开发中，你可能会遇到一些特殊的库，尤其是为 Unity UPM（Unity Package Manager）设计的库，它们没有提供官方的 NuGet 包，但你的 Mod 在运行时又需要依赖它们。针对这类情况，你可以将它们手动打包为标准的 NuGet 包，使其 DLL 文件能够作为运行时依赖随 Mod 一起发布。

**典型的目录结构示例**：

- 目标目录：`unmanaged-vendor/upm/UniTask/lib/`
- 核心文件：`UniTask.dll`

> [!NOTE]
> 请特别注意，本仓库提供的 `pack-and-publish-game-libs` 自动化工作流**不会处理**此类第三方库的打包与发布。这类库通常需要开发者自行获取其 DLL 文件，并手动发布到远程源。
>
> 此外，请务必确保你使用的 UPM 包（或其 DLL）是针对与游戏兼容的 Unity 版本编译的，以避免运行时发生兼容性错误。

> [!TIP]
> 为了避免不同 Mod 之间因携带不同版本的库而产生冲突，强烈建议使用 `ILRepack` 将这类运行时依赖内嵌到你的主程序集中[^1]。需要说明的是，这种内嵌处理在 `Release` 构建模式下是默认行为。
>

## 通过物料覆盖进行深度定制

> **注意**：本节内容与 `projects/unmanaged-vendor/README.md` 中描述的自动化工作流密切相关，建议先了解该工作流的基本原理。

在自动化工作流执行前，系统会将用户提供的压缩包解压并覆盖到对应目录。这个"覆盖"行为是一个强大的干预点，可供高级用户灵活利用。

### 覆盖机制的工作原理

除了基础的 DLL 文件外，你还可以在压缩包内遵循 `game` 目录的结构，附带任何你希望覆盖的文件。这包括但不限于：

- 修改过的 `.csproj` 项目文件
- 自定义的 C# 源码（`.cs` 文件）
- 其他配置或资源文件

通过这种方式，你可以在不修改 Git 仓库本身的情况下，实现对打包过程的深度定制。

### 使用建议

> [!WARNING]
> 此功能虽然强大，但也是一把双刃剑。推荐仅将其用于快速原型验证或个人实验场景。对于更通用的流程改进或 Bug 修复，通过代码修改（而非物料覆盖）来解决问题，是更可持续、更便于团队协作的工程实践。

## 总结

通过以上三个核心工具（控制依赖内嵌、打包第三方库、物料覆盖定制），你可以应对绝大多数"依赖如何被打包、发布和加载"的问题。当遇到特殊情况时，建议先明确你的目标——是**调整打包行为**还是**引入额外依赖**——然后选择对应的章节来执行具体的操作。

## 相关资源

- **[构建系统参考 - 核心工具链介绍](../reference/build-system.md#核心工具链介绍)** - 了解 ILRepack 等关键自动化工具的职责
- **[构建系统参考 - Mod 打包使用指南](../reference/build-system.md#mod-打包使用指南)** - 掌握打包发布目标与依赖合并流程
- **[依赖基础设施](../reference/dependency-infrastructure.md)** - 深入理解游戏依赖包体系与目录规范

## 参考资料

[^1]: `ILRepack` 是一个程序集合并工具，能够将多个 DLL 文件合并为单个程序集，从而解决依赖冲突问题。
