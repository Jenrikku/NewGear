using ImGuiNET;
using NewGear.MainMachine.FileSystem;
using NewGear.MainMachine.GUI.WindowSystem.Windows;
using System.Collections;
using System.Collections.Specialized;

namespace NewGear.MainMachine.GUI.WindowSystem {
    internal static class WindowManager {
        public static bool[] WindowOpenedStates = new bool[] {
                true, true, true, true
        };

        private static WindowContainer? currentContainer;
        private static Dictionary<FileInstance, WindowContainer> instances = new();

        public static void Initialize() {
            FileManager.LoadedFiles.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) => {
                if(e.Action == NotifyCollectionChangedAction.Add) {
                    IList? newItems = e.NewItems;

                    if(newItems is not null)
                        foreach(FileInstance file in newItems) {
                            instances.Add(file, new(file));
                        }
                } else if(e.Action == NotifyCollectionChangedAction.Remove) {
                    IList? oldItems = e.OldItems;

                    if(oldItems is not null)
                        foreach(FileInstance file in oldItems) {
                            instances.Remove(file);
                        }
                }
            };

            FileManager.CurrentFileChanged += () => {
                if(FileManager.CurrentFile is null)
                    currentContainer = null;
                else
                    currentContainer = instances[FileManager.CurrentFile];
            };
        }

        public static void RenderActiveWindows() {
            if(currentContainer is null)
                return;

            for(int i = 0; i < currentContainer.WindowList.Length; i++)
                if(WindowOpenedStates[i]) {
                    currentContainer.WindowList[i].Render();
                    ImGui.End();
                }
        }

        public enum WindowType {
            FileTree,
            Viewport,
            HexView,
            Properties
        }

        private class WindowContainer {
            public ImGUIWindow[] WindowList;

            public WindowContainer(FileInstance file) {
                WindowList = new ImGUIWindow[] {
                    new FileTree(file),
                    new Viewport(file),
                    new HexView(file),
                    new Properties(file)
                };
            }
        }
    }
}
