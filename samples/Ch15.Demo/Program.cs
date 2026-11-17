using System.Globalization;
using Ch15.Demo;

// The same output on every machine, whatever its own culture is.
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

DoubleDemo.Run();
SqliteDemo.Run();
RoundingDemo.Run();
CultureDemo.Run();
TimeDemo.Run();
