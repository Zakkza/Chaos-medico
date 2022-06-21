
# Dynamic-Link Libraries (DLLs)

(For now) All DLLs are taken from the [Exiled GitHub repo](https://github.com/Exiled-Team/EXILED)
and SCP Secret Laboratory's Server Install.

## NuGet

We're now moving to EXILED from NuGet so this means that only these files from the server directory will be needed:
 - Assembly-CSharp.dll
 - Assembly-CSharp-firstpass.dll
 - UnityEngine.CoreModule.dll

**NOTE:** As of writing this there seems to be some issues with the NuGet EXILED Package which makes the Assembly-CSharp
that's provided with the package not work so hopefully in the future only DLLs from Unity will be needed.

## Dependencies for Non-NuGet Plugins

These are all found in the Server Directory:

 - Assembly-CSharp.dll
 - Assembly-CSharp-firstpass.dll
 - UnityEngine.CoreModule.dll
 - CommandSystem.Core.dll

The others are from [Exiled Releases](https://github.com/Exiled-Team/EXILED/releases).
