## How to use

Create a new instance of the grid
```cs
private GridManager<BaseMapArea> grid = new(1000, 200, new DefaultMapAreaFactory());;
```
- **BaseMapArea** is information about your area (can be inherited)
- **DefaultMapAreaFactory** is a factory on how your map area are created (if you inherit BaseMapArea, you'll need to inherit that too)

----
> [!NOTE]
> This documentation is incomplete, using tips below on the meantime

Quite help: Inhedit `ITileData` to create your own tiles

See `GridManager.cs` to see what public methods you can use