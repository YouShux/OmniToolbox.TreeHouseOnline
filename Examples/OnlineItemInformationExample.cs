using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Dalamud.Plugin.Ipc.Exceptions;
using OmniToolbox.Commands;
using OmniToolbox.Common.Module.Abstractions;
using OmniToolbox.Common.Module.Enums;
using OmniToolbox.Common.Module.Models;
using OmniToolbox.Host;
using OmniToolbox.UI;
using TreeTools.Lifecycle;
using TreeTools.Threading;

namespace OmniToolbox.TreeHouseOnline;

[ModuleApiVersion(1)]
public sealed class OnlineItemInformationExample : OmniModuleBase
{
    public override ModuleInfo Info { get; } = new()
    {
        Category = ModuleCategory.Item,
        Author = "YouShu",
        Commands = [new("OpenItem", OmniCommandCatalog.FormatModule(nameof(OnlineItemInformationExample), "40105"))]
    };

    public override ModuleLocalization Localization { get; } = ModuleLocalization.Create(
        new Dictionary<string, string>
        {
            ["Title"] = "物品信息调用示例",
            ["Description"] = "通过物品 ID 打开 Omni 物品信息窗口。",
            ["OpenItem"] = "打开指定物品的信息窗口"
        });

    private TaskHelper tasks = null!;

    protected override void OnStart(FeatureLifetime activation)
    {
        tasks = new() { TimeoutMS = 0 };
        activation.Add(tasks);
    }

    public override bool TryHandleCommand(string arguments)
    {
        if (!uint.TryParse(arguments, NumberStyles.None, CultureInfo.InvariantCulture, out var itemID))
            return false;

        tasks.EnqueueAsync(async token =>
        {
            token.ThrowIfCancellationRequested();
            var open = DalamudServices.PluginInterface.GetIpcSubscriber<uint, Task<string>>(
                "OmniToolbox.UI.OpenItemInformation");
            if (!open.HasFunction)
            {
                DalamudServices.PluginLog.Warning("Item information IPC is unavailable.");
                return;
            }

            try
            {
                using var response = JsonDocument.Parse(await open.InvokeFunc(itemID));
                var root = response.RootElement;
                var accepted = root.TryGetProperty("accepted", out var flag) && flag.GetBoolean();
                if (!accepted)
                    DalamudServices.PluginLog.Warning("Item information request rejected: {Error}", root.GetProperty("error"));
            }
            catch (IpcError exception)
            {
                DalamudServices.PluginLog.Warning(exception, "Item information IPC is unavailable.");
            }
        });
        return true;
    }
}
