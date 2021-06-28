using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NormalizationCSV
{
    class Program
    {
        static void Main(string[] args)
        {
            string content = File.ReadAllText("E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Normalization\\NormalizationCSV\\NormalizationCSV\\OnlineNewsPopularity\\OnlineNewsPopularity_RemovedInvalidRow_FirstTwoColumns.csv");

            string[] rows = content.Split(("\n").ToCharArray());

            string minRow = rows[rows.Length - 2].Trim();
            string maxRow = rows[rows.Length - 3].Trim();
            double[] minAttributes = getAttributes(minRow);
            double[] maxAttributes = getAttributes(maxRow);

            string[] result = new string[(rows.Length - 3) * maxAttributes.Length];

            int i = 0;
            foreach (var row in rows)
            {
                if (i > 0 && i < rows.Length - 3)
                {
                    string trimedRow = row.Trim();
                    double[] attributes = getAttributes(trimedRow);

                    int k = 0;
                    foreach (var atribute in attributes)
                    {
                        if (i == 0 && k == 0)
                        {
                            result[0] = rows[0];
                        }
                        else
                        {
                            bool isNormalized = (minAttributes[k] < 0.1 && minAttributes[k] >= 0
                                        && maxAttributes[k] <= 1 && maxAttributes[k] > 0.9);
                            if (!isNormalized)
                            {
                                // normalize attribute value
                                double normalValue = (atribute - minAttributes[k]) / (maxAttributes[k] - minAttributes[k]);
                                result[(i * attributes.Length + k)] = normalValue + (k < attributes.Length - 1 ? "," : "\r\n");
                            }
                        }
                        k++;
                    }
                }
                i++;
            }

            string text =  String.Join("", result);

            File.WriteAllText("E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Normalization\\NormalizationCSV\\NormalizationCSV\\OnlineNewsPopularity\\OnlineNewsPopularity_Normalized.csv"
                                , text);
        }

        static double[] getAttributes(string attributes)
        {
            string[] attributeArray = attributes.Split((",").ToCharArray());
            double[] attributeResult = attributeArray.Select(x => Double.Parse(x)).ToArray();
            return attributeResult;
        }
    }
}
