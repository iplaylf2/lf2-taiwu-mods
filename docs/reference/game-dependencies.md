# 游戏依赖包说明

本页面详细阐述了 `LF2.Taiwu.*` 系列包的设计思路、核心用途以及最佳引用实践。所有这些包都通过 [`game-libs.manifest.yaml`](../../projects/unmanaged-vendor/game/game-libs.manifest.yaml) 清单文件进行统一的自动化打包。

## 设计理念：语义化封装与依赖对齐

本模板将《太吾绘卷》官方的程序集重新整合为独立的 NuGet 包，其核心设计理念在于**将游戏自身的依赖策略清晰地暴露给开发者，同时进行语义化的封装组织**。

> **注意**：清单文件中使用 `Taiwu.*` 前缀进行目录组织，但实际生成的 NuGet 包为 `LF2.Taiwu.*` 前缀。

### 基本设计思路

需要明确的是，游戏本体已经包含并管理了大量第三方库（如 `Newtonsoft.Json`、`0Harmony` 等）。打包策略并非要创造新的依赖关系，而是将这些现有的、分散的 DLL 文件，按照其内在功能属性（如后端逻辑、前端界面、补丁框架等）重新组织成具有明确语义的 NuGet 包。

### 核心价值体现

这种设计理念为 Mod 开发带来了三个重要价值：

**1. 简化依赖引用**
Mod 开发者不再需要从上百个 DLL 文件中手动挑选所需的组件，只需引用如 `LF2.Taiwu.Backend` 这样高内聚的功能包即可，大大降低了学习成本和出错概率。

**2. 降低冲突风险**
通过引用这些预设的包，可以确保你的 Mod 使用的第三方库版本与游戏完全一致，从源头上避免了因引入不兼容版本库而导致的冲突问题。

**3. 促进关注点分离**
通过将依赖项细分为功能性包，一个模块可以仅引用其真正需要的子集。例如，一个纯粹的代码修改模块可以只依赖 `LF2.Taiwu.BepInEx`，而无需引用任何游戏逻辑或 UI 相关的程序集。

## 核心依赖包分类详解

[`game-libs.manifest.yaml`](../../projects/unmanaged-vendor/game/game-libs.manifest.yaml) 清单文件是整个依赖管理的核心配置，它将游戏的上百个 DLL 文件按 `Taiwu.*` 目录进行组织，然后通过构建系统转换为 `LF2.Taiwu.*` NuGet 包。深入理解这个映射关系，将帮助你更精准地选择合适的依赖包。

### 1. 游戏核心逻辑包

这是 Mod 开发中最常引用的部分，包含了游戏的核心业务逻辑，按照架构层次细分为三个包：

**`LF2.Taiwu.Backend`** - 后端核心逻辑
- **来源目录**：`Taiwu.Backend/`
- **包含内容**：游戏的核心后端逻辑程序集，主要包括 `Backend/GameData.dll`、`GameData.Config.dll`、`GameData.System.dll` 等关键文件
- **适用场景**：当你的 Mod 需要与游戏的核心数据、存档系统、角色逻辑等进行深度交互时，这是必需的依赖
- **典型用例**：修改游戏机制、处理存档数据、操作角色属性等

**`LF2.Taiwu.Frontend`** - 前端界面逻辑
- **来源目录**：`Taiwu.Frontend/`
- **包含内容**：游戏的前端相关程序集，包括 `Assembly-CSharp.dll`、`Encyclopedia.dll`、`Frontend/UI.dll` 等关键文件
- **适用场景**：当你的 Mod 需要修改游戏界面、创建新的 UI 元素、响应用户交互操作时
- **典型用例**：添加新的界面面板、修改现有 UI、处理用户输入事件等

**`LF2.Taiwu.Shared`** - 共享组件
- **来源目录**：`Taiwu.Shared/`
- **包含内容**：游戏前后端共享的核心程序集和通用工具库
- **适用场景**：当你的 Mod 需要使用那些在前后端都存在的通用数据结构和工具函数时
- **设计目的**：统一管理共享组件，避免重复引用

### 2. Modding 框架包

这部分提供了让 Mod 能够正常运行并修改游戏代码的基础设施：

**`LF2.Taiwu.Modding`** - 官方 Mod 入口
- **来源目录**：`Taiwu.Modding/`
- **核心地位**：这是让你的 Mod 被游戏识别和加载的"钥匙"
- **重要性**：**所有 Mod 都必须引用此库**，否则无法被游戏正确识别
- **主要功能**：提供 Mod 生命周期管理、事件系统等基础服务

**`LF2.Taiwu.BepInEx`** - 代码补丁框架
- **来源目录**：`Taiwu.BepInEx/`
- **包含内容**：`0Harmony.dll`、`Mono.Cecil.dll`、`MonoMod.RuntimeDetour.dll` 等一系列用于运行时代码修改的核心库
- **适用场景**：当你需要使用 HarmonyX 等技术在运行时动态修改游戏的原有代码时
- **核心价值**：引用此包可以让你获得所有必要的"打补丁"工具，无需单独配置

### 3. 游戏内置第三方库包

游戏本体依赖了大量的第三方库。为了避免 Mod 引入不兼容的版本导致冲突，这些库也被重新打包为 `LF2.Taiwu.*` 前缀的版本。当你的 Mod 需要使用这些库时，**强烈建议优先引用这些 `LF2.Taiwu.*` 版本**：

**常用库示例**：
- **`LF2.Taiwu.Newtonsoft.Json`**：JSON 序列化和反序列化处理
- **`LF2.Taiwu.MoonSharp`**：Lua 脚本执行引擎
- **`LF2.Taiwu.Google.Protobuf`**：Protocol Buffers 数据序列化
- **`LF2.Taiwu.NLog`**：日志记录框架
- **`LF2.Taiwu.Spine`**：2D 动画渲染系统

### 4. Unity 引擎库包

Unity 引擎相关的程序集数量庞大，按照功能用途进行了科学的归类：

**`LF2.Taiwu.Unity.Core`** - 核心引擎模块
- **来源目录**：`Taiwu.Unity.Core/`
- **包含内容**：最常用和核心的 Unity 引擎模块
- **适用场景**：大部分 Mod 开发的基础需求

**`LF2.Taiwu.Unity.Addons`** - 附加组件模块
- **来源目录**：`Taiwu.Unity.Addons/`
- **包含内容**：`TextMeshPro`、`Timeline` 等 Unity 的官方附加组件
- **适用场景**：需要高级文本渲染或时间轴功能的 Mod

**`LF2.Taiwu.Unity.Services`** - 在线服务模块
- **来源目录**：`Taiwu.Unity.Services/`
- **包含内容**：Unity Analytics、云服务等在线服务相关的库
- **适用场景**：需要数据统计或云服务功能的 Mod

通过这种精细化的分类方式，你的 Mod 可以根据实际需求仅引用必需的 Unity 库，避免引入不必要的依赖，保持 Mod 的轻量化。

> **查看完整清单**：如需了解所有包的详细文件组成，请参阅 [`game-libs.manifest.yaml`](../../projects/unmanaged-vendor/game/game-libs.manifest.yaml) 获取完整的映射关系。

## 构建系统概览

本项目的构建系统具备**智能包识别**能力：通过检测 `Taiwu.*` 目录下 `lib/backend` 和 `lib/frontend` 的存在情况，自动选择合适的项目模板（Common、Backend 或 Frontend）进行打包。

详细的技术实现和构建命令请参阅：
- **[构建系统参考](./build-system.md)** - 深入了解 MSBuild 目标和自动化机制
- **[游戏依赖打包手册](./game-libs-packaging.md)** - 完整的打包流程和最佳实践
