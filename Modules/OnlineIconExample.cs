using Dalamud.Bindings.ImGui;
using OmniToolbox.Common.Module.Abstractions;
using OmniToolbox.Common.Module.Enums;
using OmniToolbox.Common.Module.Models;

namespace OmniToolbox.TreeHouseOnline;

public sealed class OnlineIconExample : ModuleBase
{
    public override ModuleInfo Info { get; } = new()
    {
        Title = "在线模块示范",
        Description = "选择图标并保存图标 ID。",
        Category = ModuleCategory.Interface,
        Author = "YouShu"
    };

    private OnlineIconExampleConfig config = new();

    public override bool HasSettings => true;

    public override bool DrawSettings()
    {
        ImGui.TextUnformatted($"图标 ID：{config.IconID}");
        if (ImGui.Button("选择图标"))
        {
            OpenIconBrowser(iconID => config.IconID = iconID);
        }

        ImGui.SameLine();
        if (ImGui.Button("清除图标"))
        {
            return ResetSettings();
        }

        return false;
    }

    public override bool ResetSettings()
    {
        config.IconID = 0;
        return true;
    }
}

public sealed class OnlineIconExampleConfig
{
    public uint IconID { get; set; }
}
