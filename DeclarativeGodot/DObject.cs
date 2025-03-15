using Godot;

using System;
using System.Collections.Generic;

namespace DeclarativeGodot;

public abstract class DObject : IDisposable {

#pragma warning disable IDE1006 // Naming Styles
	public virtual string? _Key { get; init; }
	public virtual bool _RecreateNode { get; init; }
#pragma warning restore IDE1006 // Naming Styles

	protected internal abstract void SetNode(Node node);
	protected internal abstract Node CreateNode();
	protected internal abstract Type GetNodeType();
	protected internal abstract IEnumerable<DObject> GetChildren();

	public virtual void Dispose() {

		GC.SuppressFinalize(this);

	}

}