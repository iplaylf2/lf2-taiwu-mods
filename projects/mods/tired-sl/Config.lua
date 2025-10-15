return {
    Title = "【测试】sl 使人疲劳",
    Source = 0,
    FileId = 3475660385,
    Version = "0.0.0.1",
    GameVersion = "0.0.79.50-test",
    Author = "我玩LF2",
    Description = "[h1]sl 使人疲劳[/h1]\n在尽量不影响难度或游戏公平的情况下，开一些小小的挂，减少制作者的 SL 强迫负担。\n而且，本mod的各项功能都可以即时开关，无须重启（开局影响不可逆）。\n\n[h2]功能详情[/h2]\n\n[h3]战斗相关[/h3]\n[list]\n  [*][b]战斗只要能捉就能捉：[/b]太吾在捕捉时，只要概率大于零就必定成功。\n  [*][b]AI战时智力为最高难度：[/b]只是提高智力，不是数值。是否生效未知，但是代码改动少，没副作用。\n  [*][b]打不着我：[/b]隐蔽小村的还月入魔打起来太痛苦了，我开了便是。\n[/list]\n\n[h3]开局相关[/h3]\n[list]\n  [*][b]太吾开局特性全为正面：[/b]从青梅借鉴个好东西；还包抓周。\n  [*][b]太吾村开局的有用资源至少2级（可移动的门槛）：[/b]开局就是要规划的啦。\n  [*][b]稳定的开局人物：[/b]作者喜好的开局调整。具体是：1. 双晚成。2. 非门派倾向权重均值为C、D之间。3. 用16次roll中总值最高的数据。\n[/list]\n\n[h3]NPC 相关[/h3]\n[list]\n  [*][b]爱惜书籍的 NPC：[/b]大幅下调 NPC 将书籍放入玄机的可能性。\n[/list]\n\n[h3]突破相关[/h3]\n[list]\n  [*][b]突破开全图：[/b]开了就是开了。\n  [*][b]突破走的失败格子不消耗步数：[/b]本来损失加成就够惨了，救一下步数好了。\n[/list]\n\n[h2]开源与反馈[/h2]\n本 Mod 为开源项目。如果您在使用过程中遇到任何 Bug 或有功能上的建议，欢迎通过下方地址进行反馈。\n[url=https://github.com/iplaylf2/lf2-taiwu-mods/blob/master/projects/mods/README.md]点击这里访问 Github 反馈页面[/url]",
    TagList = {
        [1] = "Modifications"
    },
    Visibility = 0,
    DefaultSettings = {
        [1] = {
            SettingType = "Toggle",
            Key = "collapseCatchOdds",
            DisplayName = "战斗只要能捉就能捉",
            Description = "太吾在捕捉时，只要概率大于零就必定成功。",
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
            Key = "allGoodFeature",
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
            Key = "myHobbyValue",
            DisplayName = "稳定的开局人物",
            Description = "作者喜好的开局调整。1. 双晚成。 2. 非门派倾向权重均值为C、D之间。3. 用16次roll中总值最高的数据。",
            DefaultValue = false
        },
        [7] = {
            SettingType = "Toggle",
            Key = "cherishBooks",
            DisplayName = "爱惜书籍的 NPC",
            Description = "大幅下调 NPC 将书籍放入玄机的可能性。",
            DefaultValue = false
        },
        [8] = {
            SettingType = "Toggle",
            Key = "brightenUp",
            DisplayName = "突破开全图",
            Description = "开了就是开了。",
            DefaultValue = false
        },
        [9] = {
            SettingType = "Toggle",
            Key = "noCostOnFailMove",
            DisplayName = "突破走的失败格子不消耗步数",
            Description = "本来损失加成就够惨了，救一下步数好了。",
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
    Cover = "Cover.jpg",
    WorkshopCover = "Cover.jpg",
    DetailImageList = {
        [1] = "DetailImage1.png"
    }
}