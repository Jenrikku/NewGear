// The program begins here:

using System.Reflection;

using NewGear.Configuration;
using NewGear.GUI;

using NewGear.GearSystem.GearManagement;
using NewGear.Gears.BuiltIn;

Console.WriteLine(
    " ======== NewGear v" + Assembly.GetExecutingAssembly().GetName().Version + " ======== "
);

// Set working directory to the program's folder --------
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

// Load configuration if it exists ----------------------
ConfigManager.Load("NewGear.config.yaml");

// Load Built-In gear -----------------------------------
GearHolder.StoreGears(string.Empty, BuiltInGear.Generate(), Assembly.GetExecutingAssembly());

// Load all installed gears -----------------------------
GearLoader.LoadAllGears();

#if DEBUG

NewGear.FileManagement.FileOpenQueue.Add(Assembly.GetEntryAssembly()!.Location);
#endif

// Start window manager with the main window ------------
WindowManager.Add(new MainWindowContext());
WindowManager.Run();

// Save configuration once the program ends -------------
ConfigManager.Save("NewGear.config.yaml");
