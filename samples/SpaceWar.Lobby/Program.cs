using SpaceWar;

var settings = AppSettings.LoadFromJson("appsettings.json");
settings.ParseArgs(args);
using Game1 game = new(settings);
game.Run();
