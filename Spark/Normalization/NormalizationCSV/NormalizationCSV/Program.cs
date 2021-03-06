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
            string content = File.ReadAllText("E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Normalization\\NormalizationCSV\\NormalizationCSV\\OnlineNewsPopularity\\HTRU2_WithMinMax.csv");

            string[] rows = content.Split(("\n").ToCharArray());

            string minRow = rows[rows.Length - 2].Trim();
            string maxRow = rows[rows.Length - 3].Trim();
            double[] minAttributes = getAttributes(minRow);
            double[] maxAttributes = getAttributes(maxRow);

            string[] result = new string[((rows.Length - 3) * maxAttributes.Length) + 1];

            int i = 0;
            result[0] = rows[0];
            foreach (var row in rows)
            {
                if (i > 0 && i < rows.Length - 3)
                {
                    string trimedRow = row.Trim();
                    double[] attributes = getAttributes(trimedRow);

                    int k = 0;
                    foreach (var atribute in attributes)
                    {
                        bool isNormalized = (minAttributes[k] >= 0 /*&& minAttributes[k] >= 0
                                    && maxAttributes[k] <= 2 && maxAttributes[k] > 0.9*/);
                        if (!isNormalized && k < attributes.Length - 1)
                        {

                            // normalize attribute value
                            //double normalValue = (atribute - minAttributes[k]) / (maxAttributes[k] - minAttributes[k]);
                            double normalValue = atribute - minAttributes[k];
                            result[((i - 1) * attributes.Length + k) + 1] = normalValue + (k < attributes.Length - 1 ? "," : "\r\n");
                        }
                        else
                        {
                            /*if (!isNormalized && k == attributes.Length - 1)
                            {
                                String value = atribute <= 1400 ? "Low" : "High";
                                result[((i - 1) * attributes.Length + k) + 1] = value  + (k < attributes.Length - 1 ? "," : "\r\n");
                            }
                            else
                            {*/
                                result[((i - 1) * attributes.Length + k) + 1] = atribute + (k < attributes.Length - 1 ? "," : "\r\n");
                            //}
                            
                        }
                        k++;
                    }
                }
                i++;
            }

            string text =  String.Join("", result);

            File.WriteAllText("E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Normalization\\NormalizationCSV\\NormalizationCSV\\OnlineNewsPopularity\\HTRU2_NonNegative.csv"
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
