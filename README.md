# OmniToolbox.TreeHouseOnline

Omni Toolbox 树树妙妙屋在线模块示范仓库，提供模块编写、SHA256 校验说明。

## 在 Omni 中添加

在“插件设置 → 在线模块”中填写仓库地址：

```text
https://github.com/YouShux/OmniToolbox.TreeHouseOnline
```

Omni 读取默认分支根目录的 `TreeHouseModules.json`，自动下载并安装清单中的模块。
首次安装后模块默认关闭，在“树树妙妙屋 → 在线”中手动启用。

也可以填写清单的 GitHub 文件页面地址或 raw HTTPS 地址。
当前支持公开仓库中的 `.cs`、`.dll` 文件，不支持私有仓库、Release 附件或 ZIP。

模块拥有与 Omni 相同的运行权限，没有独立沙箱。仅安装可信来源；
启用自动更新意味着持续信任该仓库后续发布的代码。

## 仓库结构

```text
OmniToolbox.TreeHouseOnline/
  Modules/
    OnlineIconExample.cs
  README.md
  TreeHouseModules.json
```

`Modules` 存放模块文件，根目录的清单描述可下载模块。
在线模块不是独立 Dalamud 插件，不能直接使用完整插件的 `Pluginmaster.json`。

## 编写在线模块

### 基本要求

- 模块继承 `ModuleBase`，并提供公共无参构造；没有声明构造函数时可以使用默认构造。
- 每个文件仅包含一个可实例化的 `ModuleBase` 实现，可以包含独立的配置类型等辅助类型。
- 模块类型名必须与清单的 `InternalName` 完全一致。
- `Info` 放在类型的首个成员位置，填写名称、说明、分类和作者。
- DLL 依赖必须由当前 Omni 环境提供；不要依赖下载目录中未被加载的额外程序集。
- 事件订阅和其他资源须有对应的停用或释放逻辑。

### 图标选择示例

```csharp
using System;
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
        Author = "YouShu",
        Commands = [new("打开图标选择器", "/omni OnlineIconExample icon")],
        SupportUrls = ["https://afdian.com/a/YouShu"],
        ReportURL = "https://discord.com/channels/1456729574330077206/1456740706109493339"
    };

    private OnlineIconExampleConfig config = new();

    public override bool HasSettings => true;

    public override bool TryHandleCommand(string arguments)
    {
        if (!string.Equals(arguments, "icon", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        OpenIconBrowser(iconID => config.IconID = iconID);
        return true;
    }

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
```

### 模块指令

启用示范模块后，输入 `/omni OnlineIconExample icon` 打开图标选择器。选择后沿用原有配置保存流程。`/omni OnlineIconExample` 不带参数时仍是模块开关。

本地和在线模块都可重写 `TryHandleCommand(string arguments)`。宿主按模块类名查找当前实例，将剩余参数传给它；模块只有启用且通过可用性检查后才会收到指令。返回 `true` 表示已处理，`false` 表示交回宿主原有路由。参数错误需要提示时，由模块提示并返回 `true`。

`Info.Commands` 用于展示帮助和复制指令，实际执行由 `TryHandleCommand` 实现。内置指令及 OmenTools 已注册的全局子指令优先，模块类名应避免与它们重名。这个入口无需手动注册和注销，模块卸载后自动停止分发。指令处理应快速返回；修改配置时使用模块自身的保存入口，宿主不会根据返回值自动保存。

此示范需要 Omni `1.1.3.0` 或更新版本提供的模块指令接口。

### 配置与图标浏览器

配置类型使用 `Config` 后缀，如 `OnlineIconExampleConfig`。
保留一个可识别的配置成员，并使用可写字段或属性，便于宿主绑定已保存的配置。
`DrawSettings()` 返回 `true` 时，宿主保存配置；输入框应在编辑结束或失焦后返回变更。
不要在构造时把配置对象另外缓存到不会同步更新的字段中。

`OpenIconBrowser` 是 `ModuleBase` 的受保护实例方法，在模块内部直接调用：

```csharp
OpenIconBrowser(iconID => config.IconID = iconID);
```

它不是 `Omni.OpenIconBrowser`。用户选中图标后执行回调，宿主随后触发配置保存。
模块停用或卸载时，基类会清理该模块的图标选择回调。
独立 UI 类需要由模块传入选择委托，不能直接访问此受保护方法。

### 作者支持与反馈

本地模块和在线模块都在 `ModuleInfo` 中填写：

```csharp
Author = "YouShu",
SupportUrls = ["https://afdian.com/a/YouShu"],
ReportURL = "https://discord.com/channels/1456729574330077206/1456740706109493339"
```

- `SupportUrls`：支持作者的链接列表，点击支持按钮后依次打开。
- `ReportURL`：该模块的反馈链接。
- 未填写时使用 Omni 默认地址；`SupportUrls = []` 表示不打开任何支持链接。

## 编写仓库清单

仓库根目录创建 `TreeHouseModules.json`：

```json
{
  "SchemaVersion": 1,
  "Modules": [
    {
      "InternalName": "OnlineIconExample",
      "Name": "在线模块示范",
      "Author": "YouShu",
      "Description": "选择图标并保存图标 ID。",
      "Version": "1.1.0",
      "MinimumOmniVersion": "1.1.3.0",
      "File": "Modules/OnlineIconExample.cs",
      "Sha256": "<替换为实际模块文件的64位SHA256>"
    }
  ]
}
```

占位校验值不能直接用于安装，必须按下一节生成后替换。

| 字段 | 填写要求 |
| --- | --- |
| `SchemaVersion` | 当前填写 `1` |
| `InternalName` | 与模块类型名一致，使用 ASCII 字母、数字和下划线，首字符不能为数字 |
| `Name`、`Author`、`Description` | 清单中的展示信息，与模块实际信息保持一致 |
| `Version` | 模块发布版本，使用两至四段非负整数，如 `1.0.0` |
| `MinimumOmniVersion` | 运行该模块所需的最低 Omni 版本 |
| `File` | 相对清单的文件路径，或完整 GitHub raw HTTPS 地址 |
| `Sha256` | 对 `File` 指向的实际文件计算 SHA256 |

`File` 扩展名使用小写 `.cs` 或 `.dll`。单个文件最大 32 MiB；
清单最大 2 MiB、最多 500 个模块。版本号不使用 `v` 前缀或预发布后缀。
相同类型名的模块不能同时安装，包括与内置、本地或其他在线仓库的模块重名。

## 生成 SHA256

SHA256 根据文件的实际字节内容计算，不是 Git 提交号，也不是手动编写的编号。
Omni 下载后会重新计算并与清单比较，不一致则拒绝安装。
校验通过只说明文件符合清单，不代表代码安全或作者身份可信。

### PowerShell 命令

以本仓库示范文件为例，按实际存放位置修改路径：

```powershell
Get-FileHash -LiteralPath 'E:\XLPlugins\FF XIV\OmniToolbox.TreeHouseOnline\Modules\OnlineIconExample.cs' -Algorithm SHA256
```

输出中的 `Hash` 就是要填写到 `Sha256` 的值。只显示校验值：

```powershell
(Get-FileHash -LiteralPath 'E:\XLPlugins\FF XIV\OmniToolbox.TreeHouseOnline\Modules\OnlineIconExample.cs' -Algorithm SHA256).Hash
```

发布 DLL 时，将路径换成最终上传的 DLL。清单下载什么文件，就计算那个文件，
不要对整个目录、其他构建产物或 GitHub 的 HTML 展示页面计算摘要。

### 检查文件与清单

以下命令检查示范模块，输出 `True` 表示本地文件与清单一致：

```powershell
$root = 'E:\XLPlugins\FF XIV\OmniToolbox.TreeHouseOnline'
$manifest = Get-Content -Raw -LiteralPath (Join-Path $root 'TreeHouseModules.json') | ConvertFrom-Json
$module = $manifest.Modules | Where-Object InternalName -EQ 'OnlineIconExample'
$file = Join-Path $root $module.File
$actual = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash
$actual -eq $module.Sha256
```

此命令适用于清单中的仓库相对路径，不用于完整 HTTPS 下载地址。

### 校验不一致的常见原因

- 计算后又修改了代码、空格或注释。
- 文件编码、UTF-8 BOM 或 CRLF / LF 换行符发生变化。
- 计算的是旧 DLL，上传的是重新编译的 DLL。
- 清单的 `File` 指向另一个文件，或文件更新后没有同步更新清单。

Git 可能转换换行符，因此本地检查一致不等于远端一致。
发生下载校验失败时，应下载 `File` 指向的原始文件，再用 `Get-FileHash` 检查。

## 发布与更新

1. 完成模块修改和必要的编译检查，确定最终上传文件。
2. 计算该文件的 SHA256。
3. 提高清单中的 `Version`，更新 `Sha256`；地址变化时同步更新 `File`。
4. 将模块文件和清单一起提交、上传。
5. 在 Omni 中刷新仓库，验证下载、加载和配置保存。

只修改 `Sha256` 而不提高 `Version`，不会触发已安装模块的版本更新。
重新编译或修改文件后，应重新计算摘要。
推荐将下载地址固定到提交或独立版本目录，减少清单与文件发布时序不一致的问题。

Omni 启动时及每六小时刷新清单，也可在设置中手动刷新。
开启自动更新后，只下载已安装模块的更高版本，下次启动 Omni 时应用。
更新编译或构造加载失败时恢复旧文件；这不代表能检测模块所有运行时错误。
卸载的模块不会因普通刷新而自动重装。

## 本地存储

在线下载文件和仓库记录位于卫月提供的 Omni 配置目录下的 `TreeHouseOnline`。
文件按仓库和模块的哈希目录隔离；`repositories.json` 保存来源、缓存清单和版本记录。

用户模块配置沿用 `TreeHouse` 存储。更新只替换程序文件，卸载默认保留用户配置。
