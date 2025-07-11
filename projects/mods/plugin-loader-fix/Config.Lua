return {
	Title = "Mod加载适配",
	Source = 0,
	FileId = 1,
	Version = "0.0.0.1",
	GameVersion = "0.0.78.49-test",
	Author = "我玩LF2",
	Description = "给Mod作为依赖的，也几乎没用",
	TagList = {
		[1] = "Frameworks",
	},
	Visibility = 2,
	DefaultSettings = { },
	UpdateLogList = { },
	ChangeConfig = false,
	HasArchive = false,
	NeedRestartWhenSettingChanged = false,
	BackendPlugins = {
		[1] = "PluginLoaderFix.Backend/PluginLoaderFix.Backend.dll"
	},
	FrontendPlugins = {
		[1] = "PluginLoaderFix.Frontend/PluginLoaderFix.Frontend.dll",
	},
	Cover = nil,
	WorkshopCover = nil,
}
