using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HyperDBDotNet {
    public class HyperDBDotNet {
        public static Boolean DEBUGMODE = false;
        private readonly VectorOperationsCPU _vectorOperations;
        
        
        //private Func<IEnumerable<string>, Matrix<double>> EmbeddingFunction { get; set; }
        //private Func<Matrix<double>, Vector<double>, Vector<double>> SimilarityMetric { get; set; }
        private IEmbed Embedder { get; set; }

        public HyperDBDotNet(IEmbed Embedder) {
            var HDDocuments = new List<string>();
            this._vectorOperations = new VectorOperationsCPU(null, HDDocuments);
            this.Embedder = Embedder;
        }


        public void AddDocument(string document, Vector<double> vector = null) {
            if (document == "") return;
            //TODO: Need better handling here
            vector ??= this.Embedder.GetVector(document);
            _vectorOperations.AddDocument(document, vector);
        }


        public void Save(string storageFile) {
            _vectorOperations.Save(storageFile);
        }
        public int Load(string storageFile) {
            return _vectorOperations.Load(storageFile);
        }


        public List<string> Query(string queryText, int topK = 5) {
            var queryVector = this.Embedder.GetVector(queryText);
            //TODO: FIX THIS
            var rankedResults = _vectorOperations.HyperSvmRankingAlgorithmSort(queryVector, topK);
            return _vectorOperations.GetResults(rankedResults);
        }


    }
}
