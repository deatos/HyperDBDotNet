using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using System.Text;
using System.Threading.Tasks;

namespace HyperDBDotNet {
    public class HDEmbed : IEmbed {

        private OpenAI.OpenAIClient Client;
        public static int TotalTokens = 0;

        public HDEmbed(string apikey) {
            this.Client = new OpenAI.OpenAIClient(new OpenAI.OpenAIAuthentication(apikey));
        }

        public Vector<Double> GetVector(String Document) {
            var result = this.Client.EmbeddingsEndpoint.CreateEmbeddingAsync(Document, OpenAI.Models.Model.Embedding_Ada_002).GetAwaiter().GetResult();
            TotalTokens += result.Usage.TotalTokens;
            //TODO: Make this not hard coded
            Console.WriteLine($"Total Tokens: {TotalTokens}, Cost: {(TotalTokens/1000)*0.0004}");
            //TODO: Add error handling
            var res = result.Data[0];
            var ret = Vector<Double>.Build.Dense(res.Embedding.Count);
            for (int i = 0; i < res.Embedding.Count; i++) {
                ret[i] = res.Embedding[i];
            }
            return ret;
        }
    }
}
