using System;
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using OmniToolbox.Commands;
using OmniToolbox.Common.Module.Abstractions;
using OmniToolbox.Common.Module.Enums;
using OmniToolbox.Common.Module.Models;
using OmniToolbox.UI;

namespace OmniToolbox.TreeHouseOnline;

[ModuleApiVersion(1)]
public sealed class OnlineIconExample : OmniModuleBase
{
    public override ModuleInfo Info { get; } = new()
    {
        Category = ModuleCategory.Interface,
        Author = "YouShu",
        Commands = [new("OpenIcon", OmniCommandCatalog.FormatModule(nameof(OnlineIconExample), "icon"))]
    };

    public override ModuleLocalization Localization { get; } = new()
    {
        SimplifiedChinese = new Dictionary<string, string>
        {
            ["Title"] = "在线模块示范",
            ["Description"] = "选择图标并保存图标 ID。",
            ["OpenIcon"] = "打开图标选择器",
            ["IconID"] = "图标 ID：{0}",
            ["SelectIcon"] = "选择图标",
            ["ClearIcon"] = "清除图标"
        }
    };

    private Settings config = new();

    public override bool HasSettings => true;

    public override bool TryHandleCommand(string arguments)
    {
        if (!string.Equals(arguments, "icon", StringComparison.OrdinalIgnoreCase)) return false;
        OpenIconBrowser(iconID => config.IconID = iconID);
        return true;
    }

    public override bool DrawSettings()
    {
        ImGui.TextUnformatted(string.Format(OmniLoc.Get(this, "IconID"), config.IconID));
        if (ImGui.Button(OmniLoc.Get(this, "SelectIcon")))
            OpenIconBrowser(iconID => config.IconID = iconID);
        ImGui.SameLine();
        return ImGui.Button(OmniLoc.Get(this, "ClearIcon")) && ResetSettings();
    }

    public override bool ResetSettings()
    {
        if (config.IconID == 0) return false;
        config.IconID = 0;
        return true;
    }

    private sealed class Settings
    {
        public uint IconID { get; set; }
    }
}
