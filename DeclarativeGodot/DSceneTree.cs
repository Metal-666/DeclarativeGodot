using Godot;

namespace DeclarativeGodot;

public abstract class DSceneTree : SceneTree {

	public override void _Initialize() {

		base._Initialize();

		DNode dNode = Create();

		static void ConstructTreeRecursive(DNode dNode, Node parentNode) {

			Node node = dNode.CreateNode();

			parentNode.AddChild(node);

			foreach(DNode childDNode in dNode.Children) {

				ConstructTreeRecursive(childDNode, node);

			}

		}

		ConstructTreeRecursive(dNode, Root);

	}

	public abstract DNode Create();

}