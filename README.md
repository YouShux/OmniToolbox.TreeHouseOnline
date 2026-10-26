# OmniToolbox.TreeHouseOnline

本仓库同时提供可运行的模块示例、生命周期示例和仓库清单格式。当前模块接口为 **API 1**，最低宿主为 Omni **1.1.8.2**。

如果不打算公开源码，欢迎先联系我讨论接入方式。确认后整合至 Omni 本体，统一混淆发布。

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

清单回退不豁免公开仓库校验。安装、更新及已安装在线模块加载前，Omni 会匿名确认 GitHub 仓库存在且公开；网络故障或 API 限流等导致无法确认时，阻止本次加载并提示重试，保留原文件、配置和启用意愿。

### 2. 写出可加载的模块

- 继承 `OmniModuleBase`，在具体模块类上声明 `[ModuleApiVersion(1)]`。外部模块提供公共无参构造，每个发布文件只包含一个可实例化模块。
- 类型名与清单 `InternalName` 一致。`Info` 填写分类、作者、权限和命令，标题与说明由 `Localization` 提供。
- 配置使用内嵌 `Settings`，宿主绑定的成员命名为 `config`。保持已发布的 JSON 键不变，不在构造函数中保存配置引用或启动业务。
- `.dll` 使用当前 Omni 的 Common 及实际用到的 TreeTools、TreeToolKit 等依赖编译，不把公共库打包进模块。引用旧 Omni 工具或 Omen 原任务类型的 DLL 会被拒载。

### 3. Localization 与语言完整性

- 窗口、按钮、提示、通知和命令说明等用户可见文字不得硬编码。
- `Localization` 必填，通过 `ModuleLocalization.Create(source, translations)` 创建；`Title`、`Description` 及模块实际使用的词条全部写入简体 `source`。
- 模块内使用 `OmniLoc.Get(this, key)`；公共控件文字使用 `OmniLoc.Get(key)`。
- `Info.Commands` 中引用的说明键必须存在于模块词条中。
- 作者只需维护一份简体词条，宿主在登记前生成并校验完整繁体；英文、日文、德文、法文可暂不填写。
- 可选 `translations` 类型为 `IReadOnlyDictionary<UILanguage, IReadOnlyDictionary<string, string>>`；未完成的词库允许缺项或空值，只有全部键非空且占位符正确时才开放该语言的模块入口。
- 查询严格使用所选语言，缺项或空译文会明确报错。简体和生成的繁体均不可为空，其他语言不自动填充简体。

### 4. 组织多模块仓库

一个仓库可以发布多个模块。`TreeHouseModules.json` 的 `Modules` 数组中，每个对象对应一个独立模块和一个文件：

```text
TreeHouseOnline/
├── TreeHouseModules.json
├── Modules/
│   ├── OnlineIconExample.cs  # 模块与 Localization 在同一文件
│   └── AnotherModule.cs
└── Examples/
    └── OnlineLifecycleExample.cs
```

- 文件名、类型名和清单 `InternalName` 保持一致。
- 一个在线 `.cs` 或 `.dll` 只能包含一个可实例化模块。
- `.cs` 模式按文件独立编译，helper 类型应放在同一文件中；需要跨文件组织时使用 `.dll`。
- 在线作者词条通过模块的 `ModuleLocalization.Create` 交付，不放入官方 `Localizations` 仓库。
- 模块专用配置、面板、节点、词条和 helper 放在对应模块文件或 DLL 内。
- `Examples/` 中的教学代码不加入发布清单。

`Localization` 是模块类的一部分，不是单独下载的 Loc 文件。增加“选择图标”文字时，在简体 `source` 中加入 `["SelectIcon"] = "选择图标"`，界面通过 `OmniLoc.Get(this, "SelectIcon")` 读取。后续新增文字时继续增加键和值即可。`.cs` 模式不能引用仓库中的另一份 Loc `.cs` 文件；`.dll` 模式可以把词条拆到自己的源码文件或嵌入资源中，但最终必须由模块的 `Localization` 属性返回。

### 5. 不要打包宿主依赖

| 交付方式 | 依赖从哪里来 |
| --- | --- |
| 在线 `.cs` | Omni 自动提供 Common、TreeTools、TreeToolKit、OmenTools、KamiToolKit 等编译引用 |
| 在线 `.dll` | 编译时引用当前 Omni 提供的 DLL；发布时只上传模块 DLL，不携带共享库 |
| 普通用户安装 | 只安装或更新 Omni，不需要额外安装 TreeTools、TreeToolKit、OmenTools 或 KamiToolKit |

在线模块不需要先手动安装 OmenTools 或 KamiToolKit。只有直接调用它们命名空间中的 API 时，作者代码才需要写对应 `using`；这些程序集仍由 Omni 宿主提供。

### 6. 参考完整模块

以下完整示例与 [OnlineIconExample.cs](Modules/OnlineIconExample.cs) 一致，发布版本为 **2.0.1**。示例只填写简体 `source`，繁体由 Omni 自动生成。

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

    public override ModuleLocalization Localization { get; } = ModuleLocalization.Create(
        new Dictionary<string, string>
        {
            ["Title"] = "在线模块示范",
            ["Description"] = "选择图标并保存图标 ID。",
            ["OpenIcon"] = "打开图标选择器",
            ["IconID"] = "图标 ID：{0}",
            ["SelectIcon"] = "选择图标",
            ["ClearIcon"] = "清除图标"
        });

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

`DrawSettings()` 仅在实际变更时返回 `true`，宿主保存当前模块配置；输入框和拖动控件在编辑结束后报告变更。`ResetSettings()` 无实际变更时也必须返回 `false`。图标选择回调执行后会保存当前模块配置。

命令通过 `Info.Commands` 声明，`TryHandleCommand` 返回 `true` 仅表示命令已处理，不会自动保存配置。

## 生命周期与资源

构造函数只准备元数据和普通状态。事件、Hook、节点和后台工作在 `OnStart(FeatureLifetime activation)` 中接入，作用域类型导入 `using TreeTools.Lifecycle;`；没有对应工作时无需重写生命周期方法。

- 接入事件前用 `activation.OnStopping` 登记解绑；`IDisposable` 资源用 `activation.Add(resource)`，`IAsyncDisposable` 资源用 `activation.AddAsync(resource.DisposeAsync)` 登记释放。异步释放先取消并等待工作退出，再释放依赖。
- 必须完成的异步初始化使用 `activation.TrackStartup(task)`；持续任务使用 `Track` 或 `RunBackground`，并响应 `activation.CancellationToken`。
- 游戏对象和原生 UI 操作在主线程执行。模块由宿主管理，不自行销毁或卸载。
- 大量初始化使用 `TreeTools.Threading.FrameworkWorkScheduler.RunAsync(items, operation, activation.CancellationToken)` 分帧执行，单项工作保持轻量；游戏文本在框架线程提取，纯文件解析和计算可放后台。
- 输入与节点更新优先使用对应事件；事件覆盖不足时保留必要检查，状态未变直接返回。
- 分步队列使用 `TreeTools.Threading.TaskHelper`，创建后立即 `activation.Add(helper)`。宿主会等待任务和取消回调实际结束。
- 普通启停复用实例，每次启用都会获得新的 `FeatureLifetime`，需要在 `OnStart` 重置该次启用状态。
- 停用时，宿主先关闭入口并等待任务退出，再调用 `OnStop()`，最后释放本次启用的资源。`OnDestroy()` 只处理实例最终释放。
- 自停用调用 `RequestEnabled(false)` 后立即返回。生命周期钩子和托管任务不能等待自身停用或同步阻塞异步操作。

完整用法见 [OnlineLifecycleExample.cs](Examples/OnlineLifecycleExample.cs)。语言切换刷新文字、别名和搜索缓存。未提供当前语言完整词库的模块显示“尚未翻译”，设置与命令入口暂停，运行中的模块暂时停用；切回支持的语言后按原启用意愿恢复。

## 发布清单

在仓库根目录保存 `TreeHouseModules.json`，完整内容如下：

```json
{
  "SchemaVersion": 1,
  "Modules": [
    {
      "InternalName": "OnlineIconExample",
      "Author": "YouShu",
      "RepositoryURL": "https://github.com/YouShux/OmniToolbox.TreeHouseOnline",
      "Version": "2.0.1",
      "ApiVersion": 1,
      "MinimumOmniVersion": "1.1.8.2",
      "File": "Modules/OnlineIconExample.cs",
      "Sha256": "8B8A5A6E7CAB95C7CA970DDF2CDA3AC41EE80FA538F783E28D2B9A119538D7DE",
      "Localization": {
        "Source": {
          "Title": "在线模块示范",
          "Description": "选择图标并保存图标 ID。"
        }
      }
    }
  ]
}
```

`InternalName` 是模块类型名，`File` 是相对仓库根目录的文件路径，`Sha256` 对应最终文件字节。`Localization.Source` 必须包含非空的 `Title`、`Description`；`Localization.Translations` 可省略，或以 `English`、`Japanese`、`German`、`French` 为语言键填写相同词条。

每个模块必须填写非空 `Author`、完整 `RepositoryURL` 及示例中的其余必填字段。`RepositoryURL` 使用 `https://github.com/owner/repository` ，须与清单及模块文件实际所属仓库一致；缺少信息、仓库不存在或未公开时不允许加载。公开仓库仍需由用户判断代码是否可信。

三个版本字段：

- `ApiVersion`：模块契约版本，当前必须为 `1`，并与类上的声明一致。
- `Version`：模块自身版本；本示例为 `2.0.1`，发布修改时递增。
- `MinimumOmniVersion`：最低宿主版本；新版接口填写 `1.1.8.2`，实际使用更高版本接口时相应提高。

`SchemaVersion` 是清单格式版本，必须为 `1`。本地与在线模块使用同一接口校验。

## 发布更新

1. 修改模块，使用当前宿主依赖编译检查。
2. 提高模块 `Version`，核对 `ApiVersion` 和 `MinimumOmniVersion`。
3. 计算最终文件的 SHA256，并更新清单：

```powershell
(Get-FileHash .\Modules\OnlineIconExample.cs -Algorithm SHA256).Hash
```

4. 上传模块文件与清单，在 Omni 中刷新仓库并安装验证。

文件内容、换行、编码或 DLL 重新编译后，都必须重新计算摘要。

模块必须使用 API 1，并通过 `ModuleLocalization.Create` 提供简体源文，重新编译后更新清单和摘要。Schema 1 清单只接受 `Localization` 结构。旧安装描述无法读取时保留原文件与用户配置，提示按当前格式重新建立仓库记录。
