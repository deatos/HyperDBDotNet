using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace HyperDBDotNet {
    public class HDEmbed : IEmbed {

        private OpenAI.OpenAIClient Client;

        public HDEmbed(string apikey) {
            var c = new OpenAI.OpenAIAuthentication(apikey);
            this.Client = new OpenAI.OpenAIClient(c);
        }

        public Vector<Double> GetVector(String Document) {
            var result = this.Client.EmbeddingsEndpoint.CreateEmbeddingAsync(Document, OpenAI.Models.Model.Embedding_Ada_002).GetAwaiter().GetResult();
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
