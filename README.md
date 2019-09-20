# HoMM queue

Players queue for Heroes of Might and Magic.

![HoMM queue](/screenshot.png?raw=true "HoMM queue")

## Build and run

```
dotnet publish -o _deploy -c Release
dotnet _deploy/homm-queue.dll
```

## Players

Edit `wwwroot/queue.json` before your game. Put players there in the same order you have them in game.
