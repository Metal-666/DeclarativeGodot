using Godot;

using System.Collections.Generic;

namespace DeclarativeGodot;

public abstract class DNodeBase : DObject {

	public virtual IReadOnlyList<DNodeBase> Children { get; init; } = [];

	protected virtual Node? Node { get; set; }

	protected internal override IEnumerable<DObject> GetChildren() =>
		Children;

}