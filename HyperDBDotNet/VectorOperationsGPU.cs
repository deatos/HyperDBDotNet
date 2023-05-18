using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using MathNet.Numerics.LinearAlgebra.Double;

namespace HyperDBDotNet {
    public class VectorOperationsGPU {
        private Matrix<double> _vectors;
        private List<string> _documents;
        private Matrix<double> _normvectors;
        private bool _isDirty = true;
        private Context _context;
        private Accelerator _accelerator;
        private Action<Index1D, ArrayView<double>, ArrayView<double>> _kernel;


        public VectorOperationsGPU(Matrix<double> HDVectors, List<string> HDDocuments) {
            this._vectors = HDVectors;
            this._documents = HDDocuments;
            var builder = Context.Create();
            builder.AllAccelerators();
            builder.Caching(CachingMode.Default);
            _context = builder.ToContext();
            _accelerator = _context.CreateCudaAccelerator(0);
            _kernel = _accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<double>, ArrayView<double>>(NormKernel);
        }
        private Vector<double> GetNormVector(Vector<double> vector) {
            return vector / vector.L2Norm();
        }

        private Matrix<double> GetNormVector(Matrix<double> matrix) {
            var normMatrix = Matrix<double>.Build.Dense(matrix.RowCount, matrix.ColumnCount);
            Parallel.For(0, matrix.RowCount, i => {
                normMatrix.SetRow(i, GetNormVector(matrix.Row(i)));
            });
            return normMatrix;
        }
        private double[] GetNormVector(double[] vector) {
            var deviceVector = _accelerator.Allocate1D<double>(vector.Length);
            deviceVector.CopyFromCPU(vector);
            var output = _accelerator.Allocate1D<double>(vector.Length);
            _kernel((int)output.Length, deviceVector.View, output.View);
            _accelerator.Synchronize();
            double[] result = new double[output.Length];
            output.CopyToCPU(result);
            deviceVector.Dispose();
            output.Dispose();
            return result;
        }

        private double[][] GetNormMatrix(double[][] matrix) {
            int rows = matrix.Length;
            int cols = matrix[0].Length;

            var normMatrix = new double[rows][];

            for (int i = 0; i < rows; i++) {
                normMatrix[i] = GetNormVector(matrix[i]);
            }

            return normMatrix;
        }
        private static void NormKernel(Index1D i, ArrayView<double> data, ArrayView<double> output) {
            double norm = 0.0;
            for (int j = 0; j < data.Length; j++)
                norm += data[j] * data[j];
            norm = Math.Sqrt(norm);

            output[i] = data[i] / norm;
        }



        public Vector<double> CosineSimilarity(Vector<double> queryVector) {
            if (_isDirty) {
                _normvectors = GetNormVector(_vectors);
                _isDirty = false;
            }
            var normQueryVector = GetNormVector(queryVector);
            return _normvectors * normQueryVector;
        }

        public int[] HyperSvmRankingAlgorithmSort(Vector<double> queryVector, int topK = 5, Func<Matrix<double>, Vector<double>, Vector<double>> metric = null) {
            //  if (metric == null) {
            //    metric = CosineSimilarity;
            // }

            var similarities = CosineSimilarity(queryVector);
            var sortedIndices = Enumerable.Range(0, similarities.Count).ToArray();
            Array.Sort(similarities.ToArray(), sortedIndices, Comparer<double>.Create((x, y) => y.CompareTo(x)));
            return sortedIndices.Take(topK).ToArray();
        }

        public void Save(string storageFile) {
            var data = new Dictionary<string, object> {
                ["vectors"] = _vectors.ToRowArrays().ToList(),
                ["documents"] = _documents
            };
            var json = JsonConvert.SerializeObject(data);
            using (var writer = new StreamWriter(storageFile)) {
                writer.Write(json);
            }
        }
        public int Load(string storageFile) {
            using (var reader = new StreamReader(storageFile)) {
                var json = reader.ReadToEnd();
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                _documents = ((JArray)data["documents"]).ToObject<List<string>>();

                var vectorsList = ((JArray)data["vectors"]).ToObject<List<List<double>>>();
                var rows = vectorsList.Count;
                var cols = vectorsList[0].Count;
                _vectors = Matrix<double>.Build.DenseOfRowArrays(vectorsList.Select(v => v.ToArray()).ToArray());
                _isDirty = true;
                return _vectors.RowCount;
            }
        }
        public void AddDocument(string document, Vector<double> vector) {


            if (_vectors == null) {
                _vectors = Matrix<double>.Build.Dense(0, vector.Count);
            } else if (vector.Count != _vectors.ColumnCount) {
                throw new ArgumentException("All vectors must have the same length.");
            }
            _vectors = _vectors.InsertRow(_vectors.RowCount, vector);
            _documents.Add(document);
            _isDirty = true;
        }
        public List<string> GetResults(int[] RankedResults) {
            return RankedResults.Select(index => _documents[index]).ToList();
        }
    }
}
