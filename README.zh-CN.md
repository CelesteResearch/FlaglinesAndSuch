[English](README.md) | [简体中文](README.zh-CN.md)

# FlaglinesAndSuch

面向 Shared 运行库的源码移植。

## 所需文件

- 原始 Celeste 安装目录，其中包含 `Celeste.dll` 和 `FNA.dll`。
- 原始 **FlaglinesAndSuch 1.6.80** 压缩包 `FlaglinesAndSuch.zip`，可从
  [支持的版本压缩包](https://gamebanana.com/mmdl/1792428)获取。
- Workspace、Shared 和 Tools 仓库；宿主环境要求见
  [工作区 README][workspace-readme-zh-cn]。

保持压缩包完整，无需提取 DLL 或修改代码。支持的压缩包及内部程序集哈希记录在
[mod-rebuild.json](mod-rebuild.json)。

将下载的压缩包命名为 `FlaglinesAndSuch.zip`，放在原始游戏安装目录的 `Mods` 下。

## 文件摆放

```text
Workspace/
  Shared/
  Tools/
  Mods/
    FlaglinesAndSuch/                 # 本仓库

Celeste/                    # 原始游戏安装目录
  Celeste.dll
  FNA.dll
  Mods/
    FlaglinesAndSuch.zip
```

其他所需 Mod 压缩包也放在原始游戏安装目录的 `Mods` 下。

## 构建

在工作区目录使用 [Tools README][tools-readme-zh-cn] 中的宿主准备参数。
以下以 Godot 为例：

```powershell
pwsh ./Tools/Prepare.ps1 `
  -CelesteDirectory '/path/to/Celeste' `
  -AudioBankDirectory '/path/to/Desktop' `
  -Targets Godot `
  -IncludeHelpers
```

准备流程构建 Shared 及 Helper 项目，再将此源码移植与原始内容打包到
`Content/Celeste/Mods/FlaglinesAndSuch.zip`。构建与打包工具使用工作区已忽略的 `artifacts/` 目录。

也可在准备 Shared 资源后单独构建项目：

```powershell
dotnet build ./Mods/FlaglinesAndSuch/Celeste.Mods.FlaglinesAndSuch.csproj -c Release
```

## 许可

源码及移植修改采用 MIT，见 [LICENSE](LICENSE) 和 [NOTICE.md](NOTICE.md)。本仓库不包含原游戏文件、Mod 原始压缩包
或本地生成的运行包。

[workspace-readme-zh-cn]: https://github.com/CelesteResearch/Workspace/blob/main/README.zh-CN.md
[tools-readme-zh-cn]: https://github.com/CelesteResearch/Tools/blob/main/README.zh-CN.md
