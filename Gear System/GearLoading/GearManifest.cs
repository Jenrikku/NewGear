using NewGear.GearSystem.Enums;
using NewGear.GearSystem.Interfaces;

namespace NewGear.GearSystem.GearLoading {
    public abstract class GearManifest {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string[] Authors { get; }
        public string[]? OriginalSources { get; }

        /// <summary>
        /// The gears contained within this project. Every class not listed here will be ignored.
        /// </summary>
        public abstract GearEntry[] Entries { get; }
    }

    public class GearEntry {
        /// <summary>
        /// The <see cref="Type"/> of the gear's class.
        /// </summary>
        public Type Type;
        /// <summary>
        /// Contains all the actions available when the user calls the context menu for this gear.
        /// </summary>
        public (string name,
                ContextItemAction action,
                ContextItemEnabledCheck isEnabled)[]? ContextMenu;
        /// <summary>
        /// An editor that will be opened by default when a file is opened with this gear.
        /// </summary>
        public EditorEntries DefaultEditor = EditorEntries.None;
        public FileIdentification? Identify;

        public GearEntry(Type type) {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }
    }

    public delegate bool FileIdentification(string filename, byte[] contents);

    public delegate void ContextItemAction(IGear gear, ContextItemArguments arguments);
    public delegate bool ContextItemEnabledCheck(IGear gear, ContextItemArguments arguments);

    public class ContextItemArguments {
        /// <summary>
        /// Gets or sets an argument.
        /// </summary>
        public object? this[string name] {
            get {
                object? value = null;

                Arguments.TryGetValue(name, out value);
                return value;
            }
            set {
                if(Arguments.ContainsKey(name))
                    Arguments[name] = value;
                else
                    Arguments.Add(name, value);
            }
        }

        public Dictionary<string, object?> Arguments { get; } = new();
    }
}
