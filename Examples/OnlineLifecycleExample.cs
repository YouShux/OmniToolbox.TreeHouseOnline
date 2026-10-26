using System;
using System.Collections.Generic;
using System.Threading;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;
using TreeTools.Threading;
using OmniToolbox.Common.Module.Abstractions;
using OmniToolbox.Common.Module.Enums;
using OmniToolbox.Common.Module.Models;
using OmniToolbox.Host;
using TreeTools.Lifecycle;
using OmniToolbox.UI;

namespace OmniToolbox.TreeHouseOnline;

[ModuleApiVersion(1)]
public sealed class OnlineLifecycleExample : OmniModuleBase
{
    public override ModuleInfo Info { get; } = new()
    {
        Category = ModuleCategory.Debug,
        Author = "YouShu"
    };

    public override ModuleLocalization Localization { get; } = ModuleLocalization.Create(
        new Dictionary<string, string>
        {
            ["Title"] = "生命周期示例",
            ["Description"] = "观察每次启用期间的事件和托管任务计数。",
            ["FrameworkUpdates"] = "本次启用的框架更新次数：{0}",
            ["BackgroundTicks"] = "本次启用的后台计时次数：{0}",
            ["Stop"] = "临时停用"
        });

    private long frameworkUpdates;
    private long backgroundTicks;

    public override bool HasSettings => true;

    protected override void OnStart(FeatureLifetime activation)
    {
        frameworkUpdates = 0;
        Interlocked.Exchange(ref backgroundTicks, 0);

        var framework = DalamudServices.Framework;
        activation.OnStopping(() => framework.Update -= OnFrameworkUpdate);
        framework.Update += OnFrameworkUpdate;

        var tasks = new TaskHelper { TimeoutMS = 0 };
        activation.Add(tasks);
        tasks.EnqueueAsync(async token =>
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(token).ConfigureAwait(false))
                Interlocked.Increment(ref backgroundTicks);
        });
    }

    protected override void OnStop()
    {
        frameworkUpdates = 0;
        Interlocked.Exchange(ref backgroundTicks, 0);
    }

    public override bool DrawSettings()
    {
        ImGui.TextUnformatted(string.Format(OmniLoc.Get(this, "FrameworkUpdates"), frameworkUpdates));
        ImGui.TextUnformatted(string.Format(OmniLoc.Get(this, "BackgroundTicks"), Volatile.Read(ref backgroundTicks)));
        if (ImGui.Button(OmniLoc.Get(this, "Stop")))
            RequestEnabled(false);
        return false;
    }

    private void OnFrameworkUpdate(IFramework framework) => frameworkUpdates++;
}
