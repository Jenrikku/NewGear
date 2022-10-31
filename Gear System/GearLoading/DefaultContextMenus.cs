using NewGear.GearSystem.Interfaces;
using NewGear.Trees.TrueTree;

namespace NewGear.GearSystem.GearLoading {
    public static class DefaultContextMenus {
        public readonly static (string, ContextItemAction, ContextItemEnabledCheck)[] ContainerMenu
            = new (string, ContextItemAction, ContextItemEnabledCheck)[] {
                (
                    "Add file",
                    (IGear gear, ContextItemArguments arguments) => {},
                    (IGear gear, ContextItemArguments arguments) =>
                        gear is IContainerGear && arguments["SelectedNode"] is null
                ),
                (
                    "Export all files...",
                    (IGear gear, ContextItemArguments arguments) => {
                        if(gear is not IContainerGear container)
                            return;

                        foreach(INode node in container.RootNode) {

                        }
                    },
                    (IGear gear, ContextItemArguments arguments) => {
                        if(gear is not IContainerGear container)
                            return false;

                        return container.RootNode.HasChildren && arguments["SelectedNode"] is null;
                    }
                ),
                (
                    "Export file",
                    (IGear gear, ContextItemArguments arguments) => {

                    },
                    (IGear gear, ContextItemArguments arguments) =>
                        gear is IContainerGear && arguments["SelectedNode"] is LeafNode
                ),
            };
    }
}
