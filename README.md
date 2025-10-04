# 太吾绘卷 Mods

## 简介

## 目录介绍

```
$ tree --gitignore -I ".git" -L 4 -a
.
├── Directory.Build.props
├── Directory.Packages.props        # 使用 CPM[^CPM] 管理包依赖
├── game-lib                        # 游戏的程序集目录
│   └── version.workflow.info
├── .github
│   └── workflows
│       └── dotnet.yml
├── .gitignore
├── global.json                     # 定义项目 sdk 版本
├── lf2-taiwu-mods.slnx
├── LICENSE
├── nuget.config                    # 将指定 github 账号作为第三方依赖的 nuget 源[^github nuget]
├── projects
│   ├── common                      # 公共代码依赖
│   │   ├── Directory.Build.props   # 通用的编译目标
│   │   ├── LF2.Backend.Helper      # 游戏后端库依赖
│   │   ├── LF2.Cecil.Helper        # 代码编辑库依赖
│   │   ├── LF2.Frontend.Helper     # 游戏前端库依赖
│   │   ├── LF2.Game.Helper         # 必选的文件依赖
│   │   └── LF2.Kit                 # 通用工具库依赖
│   ├── Directory.Build.props       # 代码风格检查与指定 C# 配置
│   ├── Directory.Build.targets
│   ├── Directory.Packages.props
│   ├── .editorconfig               # 项目代码风格配置
│   └── mods
│       ├── Directory.Build.props   # 根据 *.Backend 或 *.Frontend 区分配置
│       ├── Directory.Build.targets # 私有化所依赖的第三方程序集
│       ├── any-mod
│       │   ├── Config.lua
│       │   ├── mod.workflow.json
│       │   ├── AnyMod.Backend      # 后端 mod 项目
│       │   └── AnyMod.Frontend     # 前端 mod 项目
│       ├── ...
├── README.md
├── upm                             # 发布在 UPM[^UPM] 的第三方依赖
│   └── version.workflow.info
└── .vscode                         # vscode 开发风味更佳
    ├── extensions.json
    └── settings.json
```

[^CPM]: CPM

[^github nuget]: github nuget repo

[^UPM]: UPM

## 影响 Mod 行为的两个依赖包

### ILRepack.Lib.MSBuild.Task

将产物中的程序集，包括第三方包，合并到一个程序集并私有化，避免 Mod 之间的程序集冲突。

在 csproj 使用 `<LF2KeepItAsIs>true</LF2KeepItAsIs>` 可以避免指定内容被集成。参考例子：projects/mods/uni-task-support/UniTaskSupport.Frontend/UniTaskSupport.Frontend.csproj

### Krafs.Publicizer

将指定程序集中的内容全部公开化，在对游戏代码进行引用时能够直接访问其的私有内容，这在编译 Mod 时能增强健壮性。
在游戏官方进行更新后，也能通过引用最新的文件并编译，来进行有效性检查。

## 项目初始化

```bash
GITHUB_USERNAME="xxx" GITHUB_TOKEN="xxx" dotnet restore
```
*简要的 github nuget 源的说明*

## MSBuild 配置说明

### 变量

LF2GameLib    游戏的程序集目录
LF2Upm        UMP包目录
LF2Common     通用代码目录
LF2Mods       Mods目录
LF2IsBackend  在Mods目录下，以".Backend"结尾的项目，自动置 true
LF2IsFrontend 在Mods目录下，以".Frontend"结尾的项目，自动置 true
LF2IsModEntry 满足 LF2IsBackend 或 LF2IsFrontend 时自动置 true
LF2KeepItAsIs 当希望引用的程序集不被合并时，主动置 true