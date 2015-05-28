using System;

namespace Cechy
{
    public static class KMeans
    {
        private static int[] InitClustering(int numTuples, int numClusters, int seed)
        {
            var random = new Random(seed);
            var clustering = new int[numTuples];
            for (var i = 0; i < numClusters; ++i)
                clustering[i] = i;
            for (var i = numClusters; i < clustering.Length; ++i)
                clustering[i] = random.Next(0, numClusters);
            return clustering;
        }
        #region Float

        public static int[] Cluster(float[][] rawData, int numClusters)
        {
            var data = Normalized(rawData);
            var changed = true; var success = true;
            var clustering = InitClustering(data.Length, numClusters, 0);
            var means = AllocateFloat(numClusters, data[0].Length);
            var maxCount = data.Length * 10;
            var ct = 0;
            while (changed && success && ct < maxCount)
            {
                ++ct;
                success = UpdateMeans(data, clustering, means);
                changed = UpdateClustering(data, clustering, means);
            }
            return clustering;
        }
        private static float[][] Normalized(float[][] rawData)
        {
            var result = new float[rawData.Length][];
            for (var i = 0; i < rawData.Length; ++i)
            {
                result[i] = new float[rawData[i].Length];
                Array.Copy(rawData[i], result[i], rawData[i].Length);
            }

            for (var j = 0; j < result[0].Length; ++j)
            {
                var colSum = 0.0f;
                for (var i = 0; i < result.Length; ++i)
                    colSum += result[i][j];
                var mean = colSum / result.Length;
                var sum = 0.0f;
                for (var i = 0; i < result.Length; ++i)
                    sum += (result[i][j] - mean) * (result[i][j] - mean);
                var sd = sum / result.Length;
                for (var i = 0; i < result.Length; ++i)
                    result[i][j] = (result[i][j] - mean) / sd;
            }
            return result;
        }
        private static bool UpdateMeans(float[][] data, int[] clustering, float[][] means)
        {
            var numClusters = means.Length;
            var clusterCounts = new int[numClusters];
            for (var i = 0; i < data.Length; ++i)
            {
                var cluster = clustering[i];
                ++clusterCounts[cluster];
            }

            for (var k = 0; k < numClusters; ++k)
                if (clusterCounts[k] == 0)
                    return false;

            for (var k = 0; k < means.Length; ++k)
                for (var j = 0; j < means[k].Length; ++j)
                    means[k][j] = 0.0f;

            for (var i = 0; i < data.Length; ++i)
            {
                var cluster = clustering[i];
                for (var j = 0; j < data[i].Length; ++j)
                    means[cluster][j] += data[i][j]; // accumulate sum
            }

            for (var k = 0; k < means.Length; ++k)
                for (var j = 0; j < means[k].Length; ++j)
                    means[k][j] /= clusterCounts[k]; // danger of div by 0
            return true;
        }
        private static float[][] AllocateFloat(int numClusters, int numColumns)
        {
            var result = new float[numClusters][];
            for (var k = 0; k < numClusters; ++k)
                result[k] = new float[numColumns];
            return result;
        }
        private static bool UpdateClustering(float[][] data, int[] clustering, float[][] means)
        {
            var numClusters = means.Length;
            var changed = false;

            var newClustering = new int[clustering.Length];
            Array.Copy(clustering, newClustering, clustering.Length);

            var distances = new float[numClusters];

            for (var i = 0; i < data.Length; ++i)
            {
                for (var k = 0; k < numClusters; ++k)
                    distances[k] = Distance(data[i], means[k]);

                var newClusterID = MinIndex(distances);
                if (newClusterID != newClustering[i])
                {
                    changed = true;
                    newClustering[i] = newClusterID;
                }
            }

            if (changed == false)
                return false;

            var clusterCounts = new int[numClusters];
            for (var i = 0; i < data.Length; ++i)
            {
                var cluster = newClustering[i];
                ++clusterCounts[cluster];
            }

            for (var k = 0; k < numClusters; ++k)
                if (clusterCounts[k] == 0)
                    return false;

            Array.Copy(newClustering, clustering, newClustering.Length);
            return true; // no zero-counts and at least one change
        }
        private static float Distance(float[] tuple, float[] mean)
        {
            var sumSquaredDiffs = 0.0f;
            for (var j = 0; j < tuple.Length; ++j)
                sumSquaredDiffs += (tuple[j] - mean[j]) * (tuple[j] - mean[j]);
            return (float)Math.Sqrt(sumSquaredDiffs);
        }
        private static int MinIndex(float[] distances)
        {
            var indexOfMin = 0;
            var smallDist = distances[0];
            for (var k = 0; k < distances.Length; ++k)
            {
                if (distances[k] < smallDist)
                {
                    smallDist = distances[k];
                    indexOfMin = k;
                }
            }
            return indexOfMin;
        } 

        #endregion

        #region Double
        public static int[] Cluster(double[][] rawData, int numClusters)
        {
            var data = Normalized(rawData);
            var changed = true; var success = true;
            var clustering = InitClustering(data.Length, numClusters, 0);
            var means = AllocateDouble(numClusters, data[0].Length);
            var maxCount = data.Length * 10;
            var ct = 0;
            while (changed == true && success == true && ct < maxCount)
            {
                ++ct;
                success = UpdateMeans(data, clustering, means);
                changed = UpdateClustering(data, clustering, means);
            }
            return clustering;
        }
        private static double[][] Normalized(double[][] rawData)
        {
            var result = new double[rawData.Length][];
            for (var i = 0; i < rawData.Length; ++i)
            {
                result[i] = new double[rawData[i].Length];
                Array.Copy(rawData[i], result[i], rawData[i].Length);
            }

            for (var j = 0; j < result[0].Length; ++j)
            {
                var colSum = 0.0;
                for (var i = 0; i < result.Length; ++i)
                    colSum += result[i][j];
                var mean = colSum / result.Length;
                var sum = 0.0;
                for (var i = 0; i < result.Length; ++i)
                    sum += (result[i][j] - mean) * (result[i][j] - mean);
                var sd = sum / result.Length;
                for (var i = 0; i < result.Length; ++i)
                    result[i][j] = (result[i][j] - mean) / sd;
            }
            return result;
        }
        private static bool UpdateMeans(double[][] data, int[] clustering, double[][] means)
        {
            var numClusters = means.Length;
            var clusterCounts = new int[numClusters];
            for (var i = 0; i < data.Length; ++i)
            {
                var cluster = clustering[i];
                ++clusterCounts[cluster];
            }

            for (var k = 0; k < numClusters; ++k)
                if (clusterCounts[k] == 0)
                    return false;

            for (var k = 0; k < means.Length; ++k)
                for (var j = 0; j < means[k].Length; ++j)
                    means[k][j] = 0.0;

            for (var i = 0; i < data.Length; ++i)
            {
                var cluster = clustering[i];
                for (var j = 0; j < data[i].Length; ++j)
                    means[cluster][j] += data[i][j]; // accumulate sum
            }

            for (var k = 0; k < means.Length; ++k)
                for (var j = 0; j < means[k].Length; ++j)
                    means[k][j] /= clusterCounts[k]; // danger of div by 0
            return true;
        }
        private static double[][] AllocateDouble(int numClusters, int numColumns)
        {
            var result = new double[numClusters][];
            for (var k = 0; k < numClusters; ++k)
                result[k] = new double[numColumns];
            return result;
        }
        private static bool UpdateClustering(double[][] data, int[] clustering, double[][] means)
        {
            var numClusters = means.Length;
            var changed = false;

            var newClustering = new int[clustering.Length];
            Array.Copy(clustering, newClustering, clustering.Length);

            var distances = new double[numClusters];

            for (var i = 0; i < data.Length; ++i)
            {
                for (var k = 0; k < numClusters; ++k)
                    distances[k] = Distance(data[i], means[k]);

                var newClusterID = MinIndex(distances);
                if (newClusterID != newClustering[i])
                {
                    changed = true;
                    newClustering[i] = newClusterID;
                }
            }

            if (changed == false)
                return false;

            var clusterCounts = new int[numClusters];
            for (var i = 0; i < data.Length; ++i)
            {
                var cluster = newClustering[i];
                ++clusterCounts[cluster];
            }

            for (var k = 0; k < numClusters; ++k)
                if (clusterCounts[k] == 0)
                    return false;

            Array.Copy(newClustering, clustering, newClustering.Length);
            return true; // no zero-counts and at least one change
        }
        private static double Distance(double[] tuple, double[] mean)
        {
            var sumSquaredDiffs = 0.0;
            for (var j = 0; j < tuple.Length; ++j)
                sumSquaredDiffs += (tuple[j] - mean[j]) * (tuple[j] - mean[j]);
            return Math.Sqrt(sumSquaredDiffs);
        }
        private static int MinIndex(double[] distances)
        {
            var indexOfMin = 0;
            var smallDist = distances[0];
            for (var k = 0; k < distances.Length; ++k)
            {
                if (distances[k] < smallDist)
                {
                    smallDist = distances[k];
                    indexOfMin = k;
                }
            }
            return indexOfMin;
        } 
        #endregion
    }
}
