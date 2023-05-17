namespace HyperDBDotnetClient {
    internal class Program {

        public static HyperDBDotNet.HyperDBDotNet DB;
        private static OpenAI.OpenAIClient Client;

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
            Client = new OpenAI.OpenAIClient(new OpenAI.OpenAIAuthentication(apikey));
            DB = new HyperDBDotNet.HyperDBDotNet(embedder);
            if (File.Exists("database.json")) {
                DB.Load("database.json");
            }

            var hasdocs = true;
            while (hasdocs) {
                Console.WriteLine("Enter the path to a document to add to the database, or enter 'done' to continue.");
                var docpath = Console.ReadLine();
                if (docpath == "done") {
                    hasdocs = false;
                } else {
                    LoadDocument(docpath);
                }
            }
          
            while (true) {
                Console.WriteLine();
                Console.WriteLine("Enter query:");
               var res = QueryDatabase(Console.ReadLine(), 10);
                Console.WriteLine(res);
            }
        }

        static void LoadDocument(string path) {
            var docdata = File.ReadAllText(path);
            var docs = docdata.Split("!@!");
            foreach (var doc in docs) {
                DB.AddDocument(doc.Trim());
            }
        }   
        static string QueryDatabase(string query, int topK, string append = "") {
            var res = DB.Query(query, topK);
            query = query + append;
            if (res.Count > 0) {
                var results = "";
                foreach (var doc in res) {
                    results += doc + "\n";
                }
                var messages = new List<OpenAI.Chat.Message>();
                messages.Add(new OpenAI.Chat.Message(OpenAI.Chat.Role.System,query));
                messages.Add(new OpenAI.Chat.Message(OpenAI.Chat.Role.User,results));
                var chatreq = new OpenAI.Chat.ChatRequest(messages, OpenAI.Models.Model.GPT3_5_Turbo);
                var response = Client.ChatEndpoint.GetCompletionAsync(chatreq).GetAwaiter().GetResult();
                var ret = response.FirstChoice.Message;
                return ret;
            } else {
                return "No results found.";
            }
        }
    }
}