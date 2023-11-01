using NewGear.GearSystem.GearManagement;
using NewGear.GearSystem.Interfaces;

namespace NewGear.GUI;

public static class EditorHolder
{
    private static readonly Dictionary<IFile, IWindowGear[]> _perFileEditors = new();

    public static IWindowGear[]? Editors { get; private set; }

    public static void ChangeCurrentEditors(IFile file)
    {
        IWindowGear[]? editors;

        if (!_perFileEditors.TryGetValue(file, out editors))
        {
            int count = GearHolder.WindowGears.Count;
            editors = new IWindowGear[count];

            for (int i = 0; i < count; i++)
            {
                IWindowGear window = (IWindowGear)
                    Activator.CreateInstance(GearHolder.WindowGears[i])!;

                window.AttachedFile = file;

                editors[i] = window;

                _perFileEditors.Add(file, editors);
            }
        }

        Editors = editors;
    }

    public static void ClearCurrentEditors() => Editors = null;
}
