using Godot;

using System.Collections.Generic;

namespace DeclarativeGodot;

public class DNode {

	public virtual IReadOnlyList<DNode> Children { get; init; } = [];

	protected internal virtual Node CreateNode() => new();

}