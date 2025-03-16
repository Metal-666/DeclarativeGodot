# DeclarativeGodot

This is a highly experimental (and unfinished) package that adds Flutter-like Node tree building to Godot.

## Usage

1. Install the package via Nuget: https://www.nuget.org/packages/DeclarativeGodot.
2. Create a new Godot project (or open an existing one).
3. Set any scene as the main scene. The declarative tree will be created alongside its contents (a Godot limitation, since you can't run a game without a main scene).
4. Add a new script with `DeclarativeGodot.DSceneTree` as the base class.
   1. Add the `[GlobalClass]` attribute.
   2. Override the `_Initialize` method and call `base._Initialize()`.
   3. Override the `CreateRoot` method. Here you can start building your ~~`Widget`~~ `DObject` tree.
5. In Godot settings, set this class as the current Main Loop (Project > Project Settings... > Application > Run > Main Loop Type).
6. Profit! I guess?

## Concept and implementation

When designing this package I attempted to recreate Flutter's declarative approach while keeping everything as simple as possible (you build your tree using nested `Widgets` (which are immutable) and when you want to make changes to it you call a function which rebuilds it).

The class hierarchy in DeclarativeGodot looks like this:

|- `DSceneTree` - a class which you need to extend for the whole thing to work; extends `SceneTree`  
|- `DObject` - a base class for tree nodes, similar to Flutter's `Widget`  
  |- `DWidget` - a base class for creating your own nodes, similar to Flutter's `StatelessWidget`  
  |- `DNodeBase` - a base class for all the Godot Nodes (more on that later)  
    |- `DNode`  
      |- `DNode2D`  
        |- ...  
      |- `DNode3D`  
        |- ...  
      |- ...

The `DObject` class has the following public properties:

- `string? _Key` - similar to Flutter's `key` property. Tells the declarative tree to reuse the underlying `Node` after rebuilds.
- `bool _RecreateNode` - if set to `true`, will force the tree to create a new `Node` during the rebuild. If this is set to `false` (the `bool` default) and the `_Key` to `null` (also the default), the tree will attempt to reuse a random `Node` of a corresponding type.

Unlike Flutter, Godot allows each `Node` to have any number of child nodes. That's why the base `DObject` class has a `IEnumerable<DObject> GetChildren()` method. `DNodeBase` implements this via a `Children` property that you can assign. `DWidget` implements it via the `Build` method.

Now the fun part - the declarative Godot Node counterparts. For this, source generation was used. The source generator would go over every type in the `GodotSharp` package which extends `Node`, and generate a `D*` wrapper (`DNode`, `DLabel`, `DSprite2D` etc). These wrappers expose all the same properties that the underlying node has. When the tree is (re)built, those properties will then be (re)assigned to the underlying node (which could be a newly created one or a reused one). All these properties are nullable. If a property is `null` (meaning it wasn't explicitly set), it will not be assigned, keeping the default/reused value of the `Node`.

Additionally, for all the `events` (`Signals`) of the underlying `Node`, a property is created (with an `On*` prefix) and automatically subscribed/unsubscribed.

To rebuild the tree call `Rebuild` from a `DObject` subclass or directly on your `DSceneTree` subclass.

## Code example

> MyCustomTree.cs:

```csharp
using DeclarativeGodot;

using Godot;

[GlobalClass]
public partial class MyCustomTree : DSceneTree {

	public override void _Initialize() =>
      base._Initialize();

	public override DObject CreateRoot() =>
		new MyCustomWidget();

}
```

> MyCustomWidget.cs:

```csharp
using DeclarativeGodot;

using Godot;

public class MyCustomWidget : DWidget {

	protected override DObject Build() =>
		new DControl() {
         OnReady = () => GD.Print("The widget is ready!"),
         Children = [
            new DVBoxContainer() {
               Children = [
                  new DLabel() {
                     Text = "Hello world!",
                  },
                  new DButton() {
                     Text = "Exit",
                     OnPressed = () => (Engine.GetMainLoop() as SceneTree)?.Quit()
                  }
               ]
            }
         ]
      };

}
```
