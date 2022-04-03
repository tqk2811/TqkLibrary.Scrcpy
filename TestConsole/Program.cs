// See https://aka.ms/new-console-template for more information
using TqkLibrary.Scrcpy;

Console.WriteLine("Hello, World!");
using (Scrcpy scrcpy = new Scrcpy("abcdef"))
{
    scrcpy.Connect();
}
Console.WriteLine("Success");