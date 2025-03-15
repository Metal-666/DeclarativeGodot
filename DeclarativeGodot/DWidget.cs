using Godot;

using System;
using System.Collections.Generic;

namespace DeclarativeGodot;

public abstract class DWidget : DObject {

	protected virtual Node? Node { get; set; }

	protected abstract DObject Build();

	protected internal override void SetNode(Node node) =>
		Node = node;
	protected internal override Node CreateNode() =>
		Node = new();
	protected internal override Type GetNodeType() =>
		typeof(Node);

	protected internal override IEnumerable<DObject> GetChildren() =>
		[Build()];

}