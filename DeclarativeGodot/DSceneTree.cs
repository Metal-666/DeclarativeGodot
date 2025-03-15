using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

namespace DeclarativeGodot;

public abstract class DSceneTree : SceneTree {

	protected virtual List<DObject> CurrentDObjects { get; set; } =
		[];
	protected virtual List<(Node Node, string? Key)> CurrentNodes { get; set; } =
		[];

	public override void _Initialize() {

		base._Initialize();

		Rebuild();

	}

	public virtual void Rebuild() {

		foreach(DObject dObject in CurrentDObjects) {

			dObject.Dispose();

		}

		CurrentDObjects.Clear();

		DObject root = CreateRoot();

		List<Node> nodes = [];

		void BuildNodeTree(DObject dObject, Node parentNode) {

			CurrentDObjects.Add(dObject);

			Node? node = null;

			string? key = dObject._Key;
			Type nodeType = dObject.GetNodeType();

			if(!dObject._RecreateNode) {

				node =
					 CurrentNodes.FirstOrDefault(node =>
															!nodes.Contains(node.Node) &&
																node.Node.GetType() == nodeType &&
																node.Key == key)
									.Node;

			}

			if(node != null) {

				dObject.SetNode(node);

			}

			else {

				node = dObject.CreateNode();

				CurrentNodes.Add((node, key));

			}

			nodes.Add(node);

			Node currentParentNode = node.GetParent();

			if(currentParentNode != parentNode) {

				currentParentNode?.RemoveChild(node);

				parentNode.AddChild(node);

			}

			foreach(DObject childDObject in dObject.GetChildren()) {

				BuildNodeTree(childDObject, node);

			}

		}

		BuildNodeTree(root, Root);

		for(int i = CurrentNodes.Count - 1; i >= 0; i--) {

			(Node ExistingNode, string? Key) = CurrentNodes[i];

			if(nodes.Contains(ExistingNode)) {

				continue;

			}

			CurrentNodes.RemoveAt(i);

			ExistingNode.QueueFree();

		}

	}

	public abstract DObject CreateRoot();

}