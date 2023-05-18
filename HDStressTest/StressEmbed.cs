using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace HDStressTest {
    public class StressEmbed : HyperDBDotNet.IEmbed {

        private List<Vector<double>> vectors;
        private int _vecon;
        private OpenAI.OpenAIClient Client;

        public StressEmbed(string apikey) {
            this.vectors = new List<Vector<double>>();
            _vecon = 0;
            this.Client = new OpenAI.OpenAIClient(new OpenAI.OpenAIAuthentication(apikey));
        }

        public void AddVector(string query) {
            var result = this.Client.EmbeddingsEndpoint.CreateEmbeddingAsync(query, OpenAI.Models.Model.Embedding_Ada_002).GetAwaiter().GetResult();
            var res = result.Data[0];
            var ret = Vector<Double>.Build.Dense(res.Embedding.Count);
            for (int i = 0; i < res.Embedding.Count; i++) {
                ret[i] = res.Embedding[i];
            }
            vectors.Add(ret);
        }

        public Vector<double> GetVector(string Query) {
            //cycle through list of search vectors
            Vector<double> ret = vectors[_vecon];
            _vecon++;
            if(_vecon >= vectors.Count) {
                _vecon = 0;
            }
            return ret;
        }
        
    }
}
