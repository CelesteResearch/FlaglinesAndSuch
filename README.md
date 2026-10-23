[English](README.md) | [简体中文](README.zh-CN.md)

# FlaglinesAndSuch

Source port for the Shared runtime.

## Required files

- An original Celeste installation with `Celeste.dll` and `FNA.dll`.
- Original **FlaglinesAndSuch 1.6.80**: `FlaglinesAndSuch.zip`, available from the
  [supported archive](https://gamebanana.com/mmdl/1792428).
- The Workspace, Shared and Tools repositories. Host requirements are described
  in the [workspace README][workspace-readme].

Keep the archive intact; do not extract its DLL or edit its code. The archive and
embedded assembly hashes are recorded in [mod-rebuild.json](mod-rebuild.json).

Save the downloaded archive as `FlaglinesAndSuch.zip` in the original installation's `Mods` directory.

## Placement

```text
Workspace/
  Shared/
  Tools/
  Mods/
    FlaglinesAndSuch/                 # this repository

Celeste/                    # your original game installation
  Celeste.dll
  FNA.dll
  Mods/
    FlaglinesAndSuch.zip
```

Other required Mod archives stay together in the original installation's `Mods`
directory.

## Build

From the workspace directory, use the host options in the
[Tools README][tools-readme]. For example, for Godot:

```powershell
pwsh ./Tools/Prepare.ps1 `
  -CelesteDirectory '/path/to/Celeste' `
  -AudioBankDirectory '/path/to/Desktop' `
  -Targets Godot `
  -IncludeHelpers
```

Preparation builds the Shared and Helper projects and packages this source port
with the original content at `Content/Celeste/Mods/FlaglinesAndSuch.zip`.
Build and packaging tools use the workspace's ignored `artifacts/` directory.

After preparing Shared resources, the project can also be built directly:

```powershell
dotnet build ./Mods/FlaglinesAndSuch/Celeste.Mods.FlaglinesAndSuch.csproj -c Release
```

## License

The source and port changes use MIT.
See [LICENSE](LICENSE) and [NOTICE.md](NOTICE.md). Original game files, Mod
archives and locally generated packages are not included in this repository.

[workspace-readme]: https://github.com/CelesteResearch/Workspace/blob/main/README.md
[tools-readme]: https://github.com/CelesteResearch/Tools/blob/main/README.md
