return {
    Title = "【测试】sl 使人疲劳",
    Source = 0,
    FileId = 3475660385,
    Version = "0.0.0.1",
    GameVersion = "0.0.78.24-test",
    Author = "我玩LF2",
    Description = "注意：mod 提供的功能全部默认关闭，大家按需开启。\n\n在尽量不影响难度或游戏公平的情况下，开一些小小的挂，减少制作者的 SL 强迫负担。覆盖范围有：\n1. 战斗捕捉或逃课；战斗AI难度固定成必死（智力上）。\n2. 开局创建人物的优化。\n3. 突破简化。\n\n注意：mod 提供的功能全部默认关闭，大家按需开启。",
    TagList = {
        [1] = "Modifications"
    },
    Visibility = 0,
    DefaultSettings = {
        [1] = {
            SettingType = "Toggle",
            Key = "niceCatch",
            DisplayName = "战斗只要能捉/敲就能捉/敲",
            Description = "战斗中且允许捕捉时，将捕捉和敲剑概率放大到百分之百。",
            DefaultValue = false
        },
        [2] = {
            SettingType = "Toggle",
            Key = "fullCombatAI",
            DisplayName = "AI战时智力为最高难度",
            Description = "只是提高智力，不是数值。是否生效未知，但是代码改动少，没副作用。",
            DefaultValue = false
        },
        [3] = {
            SettingType = "Toggle",
            Key = "missMe",
            DisplayName = "打不着我",
            Description = "隐蔽小村的还月入魔打起来太痛苦了，我开了便是。",
            DefaultValue = false
        },
        [4] = {
            SettingType = "Toggle",
            Key = "goodFeature",
            DisplayName = "太吾开局特性全为正面",
            Description = "从青梅借鉴个好东西；还包抓周。",
            DefaultValue = false
        },
        [5] = {
            SettingType = "Toggle",
            Key = "canMoveResource",
            DisplayName = "太吾村开局的有用资源至少2级（可移动的门槛）",
            Description = "开局就是要规划的啦。",
            DefaultValue = false
        },
        [6] = {
            SettingType = "Toggle",
            Key = "hobbyValue",
            DisplayName = "稳定的开局人物",
            Description = "作者喜好的开局调整。1. 双晚成。 2. 非门派倾向权重均值为C、D之间。3. 用16次roll中总值最高的数据。",
            DefaultValue = false
        },
        [7] = {
            SettingType = "Toggle",
            Key = "brightenUp",
            DisplayName = "突破开全图",
            Description = "开了就是开了。",
            DefaultValue = false
        },
        [8] = {
            SettingType = "Toggle",
            Key = "endlessStep",
            DisplayName = "突破步数花不完",
            Description = "只是花不完步数，造诣不够还是突破困难的。",
            DefaultValue = false
        }
    },
    UpdateLogList = {},
    ChangeConfig = false,
    HasArchive = false,
    NeedRestartWhenSettingChanged = false,
    BackendPlugins = {
        [1] = "TiredSL.Backend.dll"
    },
    Cover = "Cover.png",
    WorkshopCover = "Cover.png",
    DetailImageList = {
        [1] = "DetailImage1.png"
    }
}