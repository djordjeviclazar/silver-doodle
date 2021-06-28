package empty;

import java.io.IOException;
import java.math.BigDecimal;
import java.math.RoundingMode;
import java.nio.file.Files;
import java.nio.file.Path;

public class NormalizationCSV
{
    public static void main(String[] args)
    {
        try
        {
            normalizeData();
        } catch (IOException ioException)
        {
            System.out.println(ioException.toString());
        } catch (Exception e)
        {
            System.out.println(e.toString());
        }
    }

    static void normalizeData() throws IOException
    {
        Path sourcePath = Path.of("E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Normalization\\NormalizationCSVJava\\Resource\\OnlineNewsPopularity_RemovedInvalidRow_FirstTwoColumns.csv");
        Path resultPath = Path.of("E:\\GithubRepo\\SP\\silver-doodle\\Spark\\Normalization\\NormalizationCSVJava\\Resource\\OnlineNewsPopularity_NormalizedBigDecimalString.csv");
        String content = Files.readString(sourcePath);
        String[] rows = content.split("\n");

        String minRow = rows[rows.length - 1].trim();
        String maxRow = rows[rows.length - 2].trim();
        BigDecimal[] minAttributes = getAttributes(minRow);
        BigDecimal[] maxAttributes = getAttributes(maxRow);

        String[] result = new String[((rows.length - 3) * maxAttributes.length) + 1];

        int i = 0;
        result[0] = rows[0];
        for (String row : rows)
        {
            if (i > 0 && i < rows.length - 3)
            {
                String trimedRow = row.trim();
                BigDecimal[] attributes = getAttributes(trimedRow);

                int k = 0;
                for (BigDecimal atribute : attributes)
                {
                    boolean isNormalized = (minAttributes[k].compareTo(new BigDecimal(0.1)) <= 0
                            && minAttributes[k].compareTo(new BigDecimal(0)) >= 0
                            && maxAttributes[k].compareTo(new BigDecimal(1)) <= 0
                            && maxAttributes[k].compareTo(new BigDecimal(0.9)) >= 0);
                    if (!isNormalized && k < attributes.length - 1)
                    {
                        // normalize attribute value
                        BigDecimal attribSubtract = atribute.subtract(minAttributes[k]);
                        BigDecimal newMaxAttrib = maxAttributes[k].subtract(minAttributes[k]);
                        //BigDecimal normalValue = (atribute.subtract(minAttributes[k]))
                        //       .divide(maxAttributes[k].subtract(minAttributes[k]));
                        BigDecimal normalValue = attribSubtract.divide(newMaxAttrib, 24, RoundingMode.HALF_UP);
                        String normalizedString = normalValue.toString() + (k < attributes.length - 1 ? "," : "\r\n");
                        result[((i - 1) * attributes.length + k) + 1] = normalizedString;
                    } else
                    {
                        result[((i - 1) * attributes.length + k) + 1] = atribute + (k < attributes.length - 1 ? "," : "\r\n");
                    }
                    k++;
                }
            } else
            {
                if (i == 0)
                {
                    result[0] = rows[0];
                }
            }
            i++;
        }

        String text = String.join("", result);
        Files.writeString(resultPath, text);
    }

    static BigDecimal[] getAttributes(String attributes)
    {

        String[] attributeArray = attributes.split(",");
        BigDecimal[] attributeResult = new BigDecimal[attributeArray.length];

        for (int i = 0; i < attributeArray.length; i++)
        {
            attributeResult[i] = new BigDecimal(attributeArray[i]);
        }

        return attributeResult;
    }
}
