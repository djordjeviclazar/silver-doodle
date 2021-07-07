import com.univocity.parsers.csv.CsvParser;
import com.univocity.parsers.csv.CsvParserSettings;
import org.apache.log4j.Level;
import org.apache.log4j.Logger;
import org.apache.spark.api.java.JavaRDD;
import org.apache.spark.api.java.JavaSparkContext;
import org.apache.spark.ml.classification.*;
import org.apache.spark.ml.feature.LabeledPoint;
import org.apache.spark.ml.linalg.Vectors;
import org.apache.spark.mllib.evaluation.BinaryClassificationMetrics;
import org.apache.spark.sql.Dataset;
import org.apache.spark.sql.Row;
import org.apache.spark.sql.SparkSession;
import org.apache.spark.sql.functions;

import java.io.FileInputStream;
import java.io.InputStreamReader;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;

public class Classification
{
    public static void main(String[] args)
    {
        Logger logger = Logger.getRootLogger();
        logger.setLevel(Level.ERROR);

        Logger.getLogger("org.apache.spark").setLevel(Level.WARN);
        Logger.getLogger("org.spark-project").setLevel(Level.WARN);

        boolean pom = Files.exists(Path.of("E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Classification\\Classification\\Resources\\OnlineNewsPopularity_Normalized.csv"));
        System.out.println(pom);
        SparkSession session = SparkSession
                .builder()
                .appName("Spark test")
                .master("local")
                .getOrCreate();

        //JavaRDD<LabeledPoint> data = loadData(session, "E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Classification\\Classification\\Resources\\OnlineNewsPopularity_Normalized.csv");
        //JavaRDD<LabeledPoint> data = loadDataBinaryClassification(session, "E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Classification\\Classification\\Resources\\OnlineNewsPopularity_Normalized.csv");
        //JavaRDD<LabeledPoint> data = loadDataBinaryClassification(session, "E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Classification\\Classification\\Resources\\OnlineNewsPopularity_Normalized_WithoutWeekdays.csv");
        //JavaRDD<LabeledPoint> data = loadDataBinaryClassification(session, "E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Classification\\Classification\\Resources\\OnlineNewsPopularity_Normalized_EasySelection.csv");
        //JavaRDD<LabeledPoint> data = loadDataBinaryClassification(session, "E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Classification\\Classification\\Resources\\FirstSelection.csv");
        //JavaRDD<LabeledPoint> data = loadDataBinaryClassification(session, "E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Classification\\Classification\\Resources\\WithoutNUM.csv");
        //JavaRDD<LabeledPoint> data = loadDataBinaryClassification(session, "E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Classification\\Classification\\Resources\\HTRU2.csv");
        //JavaRDD<LabeledPoint> pulsarData = loadDataBinaryClassification(session, "E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Classification\\Classification\\Resources\\Pulsar\\Pulsar_1.csv");
        //JavaRDD<LabeledPoint> nonPulsarData = loadDataBinaryClassification(session, "E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Classification\\Classification\\Resources\\Pulsar\\NotPulsar_2.csv");
        //JavaRDD<LabeledPoint> data = loadDataBinaryClassification(session, "E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Classification\\Classification\\Resources\\Pulsar\\HTRU2_NonNegative.csv");
        JavaRDD<LabeledPoint> data = loadDataBinaryClassification(session, "E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Classification\\Classification\\Resources\\Pulsar\\HTRU2_NonNegative_pskew_dkurtosis.csv");

        LogisticRegression logisticRegression = new LogisticRegression().setMaxIter(20);
        OneVsRest oneR = new OneVsRest().setClassifier(logisticRegression);
        NaiveBayes naiveBayes = new NaiveBayes();

        DecisionTreeClassifier dTree = new DecisionTreeClassifier();
        RandomForestClassifier rForest = new RandomForestClassifier();


        //BinaryClassificationEvaluator evaluator = new BinaryClassificationEvaluator();
        //MulticlassMetrics metrics = new MulticlassMetrics(data.rdd());
        /*MulticlassClassificationEvaluator multiEvaluator = new MulticlassClassificationEvaluator()
                .setMetricName("accuracy");*/

        /*
        CrossValidator crossValidator = new CrossValidator()
                .setEstimator(oneR)
                .setEvaluator(metrics);
        */
        JavaRDD<LabeledPoint>[] javaRDDS = data.randomSplit(new double[]{0.7, 0.3});
        JavaRDD<LabeledPoint> trainingRDD = javaRDDS[0], testRDD = javaRDDS[1];
        Dataset<Row> trainingDataset = session.createDataFrame(trainingRDD, LabeledPoint.class);
        Dataset<Row> testDataset = session.createDataFrame(testRDD, LabeledPoint.class);


        // Create training and test datasets:
        /*JavaRDD<LabeledPoint>[] pulsarJavaRDDs = pulsarData.randomSplit(new double[]{0.7, 0.3});
        JavaRDD<LabeledPoint>[] nonPulsarJavaRDDs = nonPulsarData.randomSplit(new double[]{0.7, 0.3});
        JavaRDD<LabeledPoint> trainingRDD = pulsarJavaRDDs[0].union(nonPulsarJavaRDDs[1]);
        JavaRDD<LabeledPoint> testRDD = pulsarJavaRDDs[1].union(nonPulsarJavaRDDs[0]);
        Dataset<Row> trainingDataset = session.createDataFrame(trainingRDD, LabeledPoint.class);
        Dataset<Row> testDataset = session.createDataFrame(testRDD, LabeledPoint.class);*/


        // Creating models and evaluation
        OneVsRestModel oneRModel = oneR.fit(trainingDataset);
        Dataset<Row> oneRPredictions = oneRModel.transform(testDataset);
        Dataset<Row> oneRResults = oneRPredictions.select("prediction", "label");
        //double oneRAcc = multiEvaluator.evaluate(oneRResults);
        //double oneRAccE = evaluator.evaluate(oneRResults);

        Dataset<Row> oneRConfusionMatrix = oneRPredictions.groupBy("prediction", "label").agg(functions.count("*").as("C"));

        BinaryClassificationMetrics oneRMetrics = new BinaryClassificationMetrics(oneRResults);
        Object oneRAcc = oneRMetrics.precisionByThreshold().toJavaRDD().collect();
        Object oneRRec = oneRMetrics.recallByThreshold().toJavaRDD().collect();
        Object oneRPrecRecCurve = oneRMetrics.pr().toJavaRDD().collect();
        Object oneRFMeasure = oneRMetrics.fMeasureByThreshold().toJavaRDD().collect();
        double oneRAreaROC = oneRMetrics.areaUnderROC();
        double oneRAreaPR = oneRMetrics.areaUnderPR();
        System.out.println("OneR: \r\n");
        System.out.println("Accuracy: " + oneRAcc);
        //System.out.println("Accuracy_Num" + oneRAccE);
        System.out.println("Recall: " + oneRRec);
        System.out.println("Precision/Recall curve: " + oneRPrecRecCurve);
        System.out.println("F-Measure: " + oneRFMeasure);
        System.out.println("Area under ROC: " + oneRAreaROC);
        System.out.println("Area under PR: " + oneRAreaPR);
        System.out.println("Confusion matrix");
        oneRConfusionMatrix.show();
        System.out.println("--------------------------------");

        NaiveBayesModel naiveBayesModel = naiveBayes.fit(trainingDataset);
        Dataset<Row> naiveBayesPredictions = naiveBayesModel.transform(testDataset);
        Dataset<Row> naiveBayesResults = naiveBayesPredictions.select("prediction", "label");

        Dataset<Row> naiveBayesConfusionMatrix = naiveBayesPredictions.groupBy("prediction", "label").agg(functions.count("*").as("C"));

        BinaryClassificationMetrics naiveBayesMetrics = new BinaryClassificationMetrics(naiveBayesResults);
        Object naiveBayesAcc = naiveBayesMetrics.precisionByThreshold().toJavaRDD().collect();
        Object naiveBayesRec = naiveBayesMetrics.recallByThreshold().toJavaRDD().collect();
        Object naiveBayesPrecRecCurve = naiveBayesMetrics.pr().toJavaRDD().collect();
        Object naiveBayesFMeasure = naiveBayesMetrics.fMeasureByThreshold().toJavaRDD().collect();
        double naiveBayesAreaROC = naiveBayesMetrics.areaUnderROC();
        double naiveBayesAreaPR = naiveBayesMetrics.areaUnderPR();
        System.out.println("NaiveBayes: \r\n");
        System.out.println("Accuracy: " + naiveBayesAcc);
        System.out.println("Recall: " + naiveBayesRec);
        System.out.println("Precision/Recall curve: " + naiveBayesPrecRecCurve);
        System.out.println("F-Measure: " + naiveBayesFMeasure);
        System.out.println("Area under ROC: " + naiveBayesAreaROC);
        System.out.println("Area under PR: " + naiveBayesAreaPR);
        System.out.println("Confusion matrix: ");
        naiveBayesConfusionMatrix.show();
        System.out.println("--------------------------------");

        DecisionTreeClassificationModel dTreeModel = dTree.fit(trainingDataset);
        Dataset<Row> dTreePredictions = dTreeModel.transform(testDataset);
        Dataset<Row> dTreeResults = dTreePredictions.select("prediction", "label");

        //double dTreeAcc = multiEvaluator.evaluate(dTreeResults);
        //double dTreeAcc = evaluator.evaluate(dTreeResults);
        Dataset<Row> dTreeConfusionMatrix = dTreePredictions.groupBy("prediction", "label").agg(functions.count("*").as("C"));

        BinaryClassificationMetrics dTreeMetrics = new BinaryClassificationMetrics(dTreeResults);
        Object dTreeAcc = dTreeMetrics.precisionByThreshold().toJavaRDD().collect();
        Object dTreeRec = dTreeMetrics.recallByThreshold().toJavaRDD().collect();
        Object dTreePrecRecCurve = dTreeMetrics.pr().toJavaRDD().collect();
        Object dTreeFMeasure = dTreeMetrics.fMeasureByThreshold().toJavaRDD().collect();
        double dTreeAreaROC = dTreeMetrics.areaUnderROC();
        double dTreeAreaPR = dTreeMetrics.areaUnderPR();
        System.out.println("DTree: \r\n");
        System.out.println("Accuracy: " + dTreeAcc);
        System.out.println("Recall: " + dTreeRec);
        System.out.println("Precision/Recall curve: " + dTreePrecRecCurve);
        System.out.println("F-Measure: " + dTreeFMeasure);
        System.out.println("Area under ROC: " + dTreeAreaROC);
        System.out.println("Area under PR: " + dTreeAreaPR);
        System.out.println("Confusion matrix ");
        dTreeConfusionMatrix.show();
        System.out.println("--------------------------------");

        RandomForestClassificationModel rForestModel = rForest.fit(trainingDataset);
        Dataset<Row> rForestPredictions = rForestModel.transform(testDataset);
        Dataset<Row> rForestResults = rForestPredictions.select("prediction", "label");
        int FP = 0, TP = 0, FN = 0, TN = 0;

        Dataset<Row> rForestConfusionMatrix = rForestPredictions.groupBy("prediction", "label").agg(functions.count("*").as("C"));

        BinaryClassificationMetrics rForestMetrics = new BinaryClassificationMetrics(rForestResults);
        Object rForestAcc = rForestMetrics.precisionByThreshold().toJavaRDD().collect();
        Object rForestRec = rForestMetrics.recallByThreshold().toJavaRDD().collect();
        Object rForestPrecRecCurve = rForestMetrics.pr().toJavaRDD().collect();
        Object rForestFMeasure = rForestMetrics.fMeasureByThreshold().toJavaRDD().collect();
        double rForestAreaROC = rForestMetrics.areaUnderROC();
        double rForestAreaPR = rForestMetrics.areaUnderPR();
        System.out.println("Random Forest: \r\n");
        System.out.println("Accuracy: " + rForestAcc);
        System.out.println("Recall: " + rForestRec);
        System.out.println("Precision/Recall curve: " + rForestPrecRecCurve);
        System.out.println("F-Measure: " + rForestFMeasure);
        System.out.println("Area under ROC: " + rForestAreaROC);
        System.out.println("Area under PR: " + rForestAreaPR);
        System.out.println("Confusion matrix: ");
        rForestConfusionMatrix.show();
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

    public static JavaRDD<LabeledPoint> loadDataBinaryClassification(SparkSession session, String path)
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
            double labelValue = label;
            double[] features = new double[row.length - 1];
            for (int i = 0; i < row.length - 1; i++)
                features[i] = Double.parseDouble(row[i].trim());
            data.add(new LabeledPoint(labelValue, Vectors.dense(features)));
        }

        JavaSparkContext jc = JavaSparkContext.fromSparkContext(session.sparkContext());

        return jc.parallelize(data);
    }
}
