// The program begins here:

Console.WriteLine(
    " ======== NewGear v" +
    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version +
    " ======== ");

// Set working directory to the program's folder ---------------
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

// Make sure directories exist ---------------------------------
Directory.CreateDirectory(AppContext.BaseDirectory + "Gears");
Directory.CreateDirectory(AppContext.BaseDirectory + "Themes");

// Gears -------------------------------------------------------
Console.WriteLine("Loading gears:");
NewGear.GearSystem.GearLoading.GearManager.LoadGears();

// Open Files (Arguments) --------------------------------------
NewGear.MainMachine.FileSystem.FileManager.OpenFiles(args);

// Main Window -------------------------------------------------
Console.WriteLine("Rendering...");
NewGear.MainMachine.GUI.MainWindow.Start();