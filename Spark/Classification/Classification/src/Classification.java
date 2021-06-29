import com.univocity.parsers.csv.CsvParser;
import com.univocity.parsers.csv.CsvParserSettings;
import org.apache.spark.api.java.JavaRDD;
import org.apache.spark.api.java.JavaSparkContext;
import org.apache.spark.ml.classification.LogisticRegression;
import org.apache.spark.ml.classification.NaiveBayes;
import org.apache.spark.ml.classification.OneVsRest;
import org.apache.spark.ml.classification.OneVsRestModel;
import org.apache.spark.ml.evaluation.BinaryClassificationEvaluator;
import org.apache.spark.ml.evaluation.MulticlassClassificationEvaluator;
import org.apache.spark.ml.feature.LabeledPoint;
import org.apache.spark.ml.linalg.Vectors;
import org.apache.spark.mllib.evaluation.MulticlassMetrics;
import org.apache.spark.sql.Dataset;
import org.apache.spark.sql.Row;
import org.apache.spark.sql.SparkSession;

import java.io.FileInputStream;
import java.io.InputStreamReader;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;

public class Classification
{
    public static void main(String[] args)
    {
        boolean pom = Files.exists(Path.of("E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Classification\\Classification\\Resources\\OnlineNewsPopularity_Normalized.csv"));
        System.out.println(pom);
        SparkSession session = SparkSession
                .builder()
                .appName("Spark test")
                .master("local")
                .getOrCreate();

        JavaRDD<LabeledPoint> data = loadData(session, "E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Classification\\Classification\\Resources\\OnlineNewsPopularity_Normalized.csv");

        LogisticRegression logisticRegression = new LogisticRegression().setMaxIter(20);
        OneVsRest oneR = new OneVsRest().setClassifier(logisticRegression);
        NaiveBayes naiveBayes = new NaiveBayes();
        //DecisionTreeClassifier dTree = new DecisionTreeClassifier();
        //RandomForestClassifier rForest = new RandomForestClassifier();


        BinaryClassificationEvaluator evaluator = new BinaryClassificationEvaluator();
        MulticlassMetrics metrics = new MulticlassMetrics(data.rdd());
        MulticlassClassificationEvaluator multiEvaluator = new MulticlassClassificationEvaluator()
                .setMetricName("accuracy");

        /*
        CrossValidator crossValidator = new CrossValidator()
                .setEstimator(oneR)
                .setEvaluator(metrics);
        */
        JavaRDD<LabeledPoint>[] javaRDDS = data.randomSplit(new double[]{0.7, 0.3});
        JavaRDD<LabeledPoint> trainingRDD = javaRDDS[0], testRDD = javaRDDS[1];
        Dataset<Row> trainingDataset = session.createDataFrame(trainingRDD, LabeledPoint.class);
        Dataset<Row> testDataset = session.createDataFrame(testRDD, LabeledPoint.class);

        OneVsRestModel oneRModel = oneR.fit(trainingDataset);
        Dataset<Row> oneRPredictions = oneRModel.transform(testDataset);
        Dataset<Row> oneRResults = oneRPredictions.select("prediction", "label");

        double oneRAcc = evaluator.evaluate(oneRResults);
        System.out.println("OneR: \r\n");
        System.out.println("Accuracy: " + oneRAcc);
        System.out.println("--------------------------------");

        session.close();
    }

    public static InputStreamReader getReader(String path)
    {
        try
        {
            return new InputStreamReader(new FileInputStream(path), "UTF-8");
        } catch (Exception e)
        {
            e.printStackTrace();
        }
        return null;
    }

    public static JavaRDD<LabeledPoint> loadData(SparkSession session, String path)
    {

        ArrayList<LabeledPoint> data = new ArrayList<>();
        CsvParserSettings settings = new CsvParserSettings();
        settings.getFormat().setLineSeparator("\r\n");

        CsvParser parser = new CsvParser(settings);

        parser.beginParsing(getReader(path));

        String[] row = parser.parseNext();
        while ((row = parser.parseNext()) != null)
        {
            double label = Double.parseDouble(row[row.length - 1]);
            double labelValue = label <= 1000 ? 0.0 : (label <= 2400 ? 1.0 : 2.0);
            double[] features = new double[row.length - 1];
            for (int i = 0; i < row.length - 1; i++)
                features[i] = Double.parseDouble(row[i].trim());
            data.add(new LabeledPoint(labelValue, Vectors.dense(features)));
        }

        JavaSparkContext jc = JavaSparkContext.fromSparkContext(session.sparkContext());

        return jc.parallelize(data);
    }
}
