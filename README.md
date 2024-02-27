# HLAE Launcher

An HLAE updater that checks for the version of the executable.
It can easily be modified to work for any program by adding the name of the executable and the Github repository link.
```cs
private const string toolName = "HLAE"; // Your tools name
private const string urlRepo = "https://github.com/advancedfx/advancedfx"; // Link to the repository
```

![image](https://github.com/Devostated/HLAELauncher/assets/30211694/87d314b6-0dd7-40f6-8d79-20a8f9693790)

## Build Requirements
- Newtonsoft.Json
- Costura.Fody
