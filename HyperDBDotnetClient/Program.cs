namespace HyperDBDotnetClient {
    internal class Program {

        public static HyperDBDotNet.HyperDBDotNet DB;


        static void Main(string[] args) {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var apikey = "";
            if (File.Exists(Path.Combine(path, "oak.txt"))) {
                apikey = File.ReadAllText(Path.Combine(path, "oak.txt")).Trim();
            } else {
                Console.WriteLine("Cant load api key from file. Please enter it manually.");
                apikey = Console.ReadLine();
            }

            var embedder = new HyperDBDotNet.HDEmbed(apikey);
            DB = new HyperDBDotNet.HyperDBDotNet(embedder);
            DB.AddDocument("This is instructions for a program");
            DB.AddDocument("This is a test document");

            var res = DB.Query("find me a test document", 5);
            var res2 = DB.Query("find me instructions on a program", 5);
        }
    }
}