namespace HyperDBDotnetClient {
    internal class Program {

        public static HyperDBDotNet.HyperDBDotNet DB;


        static void Main(string[] args) {
            DB = new HyperDBDotNet.HyperDBDotNet();
        }
    }
}