# HoMM queue

Players queue for Heroes of Might and Magic.

![HoMM queue](/screenshot.png?raw=true "HoMM queue")

More details in the [following article](https://retifrav.github.io/blog/2019/09/21/homm-queue/).

## Requirements

.NET Core:

- SDK: 2.2.401
- runtime: 2.2.6

## Build and run

```
dotnet publish -o _deploy -c Release
dotnet _deploy/homm-queue.dll
```

## Players

Edit `wwwroot/queue.json` before your game. Put players there in the same order you have them in game.
