# OmniToolbox.TreeHouseOnline

本仓库同时提供可运行的模块示例、生命周期示例和仓库清单格式。当前模块接口为 **API 1**，最低宿主为 Omni **1.1.8.2**。

## 系统架构

```text
OmniToolbox
├── OmniToolbox.Common
│   ├── Localizations
│   ├── TreeTools
│   └── TreeToolKit
├── OmniToolbox.Tree
├── OmniToolbox.TreePublic
└── TreeHouseOnline
    └── TreeHouseModules.json → 下载、校验、安装
```

| 项目 | 职责 |
| --- | --- |
| [OmniToolbox.Common](https://github.com/YouShux/OmniToolbox.Common) | 模块基类、配置、本地化、共享界面和公共服务 |
| `OmniToolbox.Tree` | Omni 私有内置模块，不通过在线仓库发布 |
| [OmniToolbox.TreePublic](https://github.com/YouShux/OmniToolbox.TreePublic) | Omni 公开内置模块，不通过在线仓库发布 |
| [TreeTools](https://github.com/YouShux/TreeTools) | 生命周期、任务队列、调度与通用工具 |
| [TreeToolKit](https://github.com/YouShux/TreeToolKit) | Kami 原生界面、窗口、节点和字体 |
| [Localizations](https://github.com/YouShux/OmniToolbox.Localizations) | 官方公共词条；作者词条随模块交付 |

## 接入流程

### 1. 让 Omni 找到仓库

在“插件设置 → 在线模块”中填写仓库地址：

```text
https://github.com/YouShux/OmniToolbox.TreeHouseOnline
```

支持公开仓库中的 `.cs`、`.dll`。首次安装后默认关闭，在“树树妙妙屋 → 在线”中手动启用。模块拥有与 Omni 相同的运行权限，请安装可信来源。

地址支持 GitHub 仓库、`blob` 文件和 `raw.githubusercontent.com` 清单。GitHub API 不可用时，仓库地址会自动回退到 raw `main`、`master`；直接填写 raw 清单时保持精确来源。

### 2. 写出可加载的模块

- 继承 `OmniModuleBase`，在具体模块类上声明 `[ModuleApiVersion(1)]`。外部模块提供公共无参构造，每个发布文件只包含一个可实例化模块。
- 类型名与清单 `InternalName` 一致。`Info` 填写分类、作者、权限和命令，标题与说明由 `Localization` 提供。
- `Localization` 必填，在 `SimplifiedChinese` 中提供非空 `Title`、`Description` 和模块使用的显示词条；繁体由 Omni 自动生成，不填写繁体字典。模块词条使用 `OmniLoc.Get(this, key)`，公共词条显式使用 `OmniLoc.Get(key)`。
- 配置使用内嵌 `Settings`，宿主绑定的成员命名为 `config`。保持已发布的 JSON 键不变，不在构造函数中保存配置引用或启动业务。
- `.dll` 使用当前 Omni 的 Common 及实际用到的 TreeTools、TreeToolKit 等依赖编译，不把公共库打包进模块。引用旧 Omni 工具或 Omen 原任务类型的 DLL 会被拒载。

### 3. 组织多模块仓库

一个仓库可以发布多个模块。`TreeHouseModules.json` 的 `Modules` 数组中，每个对象对应一个独立模块和一个文件：

```text
TreeHouseOnline/
├── TreeHouseModules.json
├── Modules/
│   ├── OnlineIconExample.cs
│   └── AnotherModule.cs
└── Examples/
    └── OnlineLifecycleExample.cs
```

- 文件名、类型名和清单 `InternalName` 保持一致。
- 一个在线 `.cs` 或 `.dll` 只能包含一个可实例化模块。
- `.cs` 模式按文件独立编译，helper 类型应放在同一文件中；需要跨文件组织时使用 `.dll`。
- 模块专用配置、面板、节点和 helper 放在对应模块文件或 DLL 内，不复制到 Common。
- `Examples/` 中的教学代码不加入清单。

### 4. 不要打包宿主依赖

| 交付方式 | 依赖从哪里来 |
| --- | --- |
| 在线 `.cs` | Omni 自动提供 Common、TreeTools、TreeToolKit、OmenTools、KamiToolKit 等编译引用 |
| 在线 `.dll` | 编译时引用当前 Omni 提供的 DLL；发布时只上传模块 DLL，不携带共享库 |
| 普通用户安装 | 只安装或更新 Omni，不需要额外安装 TreeTools、TreeToolKit、OmenTools 或 KamiToolKit |

在线模块不需要先手动安装 OmenTools 或 KamiToolKit。只有直接调用它们命名空间中的 API 时，作者代码才需要写对应 `using`；这些程序集仍由 Omni 宿主提供。

### 5. 参考完整模块

以下完整示例与 [OnlineIconExample.cs](Modules/OnlineIconExample.cs) 一致，发布版本为 **2.0.1**。

```csharp
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
```

`DrawSettings()` 返回 `true` 时，宿主保存配置；返回 `false` 表示没有变更。`ResetSettings()` 无实际变更时也必须返回 `false`。图标选择回调执行后会保存配置。

命令通过 `Info.Commands` 声明，`TryHandleCommand` 返回 `true` 仅表示命令已处理，不会自动保存配置。

## 生命周期与资源

构造函数只准备元数据和普通状态。事件、Hook、节点和后台工作在 `OnStart(FeatureLifetime activation)` 中接入，作用域类型导入 `using TreeTools.Lifecycle;`；没有对应工作时无需重写生命周期方法。

- 接入事件前用 `activation.OnStopping` 登记解绑；取得资源后立即用 `activation.Add` 或 `AddAsync` 登记释放。
- 必须完成的异步初始化使用 `activation.TrackStartup(task)`；持续任务使用 `Track` 或 `RunBackground`，并响应 `activation.CancellationToken`。
- 游戏对象和原生 UI 操作在主线程执行。模块由宿主管理，不自行销毁或卸载。
- 分步队列使用 `TreeTools.Threading.TaskHelper`，创建后立即 `activation.Add(helper)`。宿主会等待任务和取消回调实际结束。
- 普通启停复用实例，每次启用都会获得新的 `FeatureLifetime`，需要在 `OnStart` 重置该次启用状态。
- 停用时，宿主先关闭入口并等待任务退出，再调用 `OnStop()`，最后释放本次启用的资源。`OnDestroy()` 只处理实例最终释放。
- 自停用调用 `RequestEnabled(false)` 后立即返回。生命周期钩子和托管任务不能等待自身停用或同步阻塞异步操作。

完整用法见 [OnlineLifecycleExample.cs](Examples/OnlineLifecycleExample.cs)。语言切换只刷新文字、别名和搜索缓存，不启停业务模块。

## 发布清单

在仓库根目录保存 `TreeHouseModules.json`，完整内容如下：

```json
{
  "SchemaVersion": 1,
  "Modules": [
    {
      "InternalName": "OnlineIconExample",
      "Name": "在线模块示范",
      "Author": "YouShu",
      "Description": "选择图标并保存图标 ID。",
      "Version": "2.0.1",
      "ApiVersion": 1,
      "MinimumOmniVersion": "1.1.8.2",
      "File": "Modules/OnlineIconExample.cs",
      "Sha256": "B0094B16EC454A4CD08476027730BBFF8EF1647751716F726FF94AA30BFD1C71"
    }
  ]
}
```

`InternalName` 是模块类型名，`File` 是相对仓库根目录的文件路径，`Sha256` 对应最终文件字节。作者支持链接、反馈地址和预览图可选。

三个版本字段：

- `ApiVersion`：模块契约版本，当前必须为 `1`，并与类上的声明一致。
- `Version`：模块自身版本；本示例为 `2.0.1`，发布修改时递增。
- `MinimumOmniVersion`：最低宿主版本；新版接口填写 `1.1.8.2`，实际使用更高版本接口时相应提高。

`SchemaVersion` 是清单格式版本，保持 `1`。本地与在线模块使用同一接口校验。

## 发布更新

1. 修改模块，使用当前宿主依赖编译检查。
2. 提高模块 `Version`，核对 `ApiVersion` 和 `MinimumOmniVersion`。
3. 计算最终文件的 SHA256，并更新清单：

```powershell
(Get-FileHash .\Modules\OnlineIconExample.cs -Algorithm SHA256).Hash
```

4. 上传模块文件与清单，在 Omni 中刷新仓库并安装验证。

文件内容、换行、编码或 DLL 重新编译后，都必须重新计算摘要。

旧版模块会被拒绝加载。升级时需要替换基类、增加接口声明、迁移生命周期、补齐简体词条，再重新编译并更新清单。
