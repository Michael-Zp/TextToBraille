using CommandLine;
using Shared;
using System.Numerics;
using TextGenerator.Alphabet;

namespace StlGenerator
{
    public static class StlGenerator
    {
        public static void GenerateStl(string modelName, string outputPath, List<List<Letter>> braillePoints, TypesettingOptions typesettingOptions, OutputFormat outputFormat, PrintOptions printOptions)
        {
            IStlExporter exporter = outputFormat == OutputFormat.Text ? new TextBasedStlExporter() : new BinaryStlExporter();
            ModelBuilder builder = new ModelBuilder(modelName, exporter);

            var numColumns = braillePoints.MaxBy((line) => line.Count)!.Count;
            var numRows = braillePoints.Count;

            var plateSize = new Vector3(
                typesettingOptions.LetterWidth * numColumns + (numColumns + 1) * typesettingOptions.SpaceBetweenLetters,
                typesettingOptions.LetterHeight * numRows + (numRows + 1) * typesettingOptions.SpaceBetweenLines,
                0.5f);

            var knobRadius = 0.75f;
            var knobHeight = 0.5f;

            // For now just have the plate at 0.0, should be fine
            var plateCenter = Vector3.Zero;

            if (printOptions.PrintWithBasePlate)
            {
                builder.AddAABox(plateSize, plateCenter);
                knobHeight *= 2;
            }

            // Take whole width of letter, reduce it by two knobs (4 * knobRadius), divide by 3, because we have 3 spaces inbetween, divide by 2, because only one half of the middle space matters, add knob radius again to find middle.
            var rightColumnOffset = ((typesettingOptions.LetterWidth - (4 * knobRadius)) / 3.0f / 2.0f) + knobRadius;
            var leftColumn = -rightColumnOffset;

            // Get one half of the letter height, remove the space, that is used up by the middle knob, divide by 2 to find center in the rest of the space, add knob Radius of the middle knob again to get offset
            var topRowOffset = (((typesettingOptions.LetterHeight / 2.0f) - knobRadius) / 2.0f) + knobRadius;
            var middleRowOffset = 0.0f;
            var bottomRowOffset = -topRowOffset;

            // Start at the left which is going to one side half of the plate size and then going back to the center the outer space and half the letter width/height
            var startPoint = new Vector3(
                (plateSize.X / 2.0f * -1) + typesettingOptions.SpaceBetweenLetters + typesettingOptions.LetterWidth / 2.0f,
                (plateSize.Y / 2.0f) - typesettingOptions.SpaceBetweenLines - typesettingOptions.LetterHeight / 2.0f,
                plateSize.Z - 0.05f); // Have a small offset into the plate, to prevent z-fighting like issues


            for (int row = 0; row < braillePoints.Count; ++row)
            {
                for (int column = 0; column < braillePoints[row].Count; ++column)
                {
                    var centerOffset = startPoint + new Vector3(
                        (typesettingOptions.LetterWidth + typesettingOptions.SpaceBetweenLetters) * column, // Go right with columns
                        (typesettingOptions.LetterHeight + typesettingOptions.SpaceBetweenLines) * row * -1, // Go down with rows
                        0.0f);

                    // Braille is numbered like this:
                    // 1 4
                    // 2 5
                    // 3 6
                    for (int pointNumber = 0; pointNumber < 6; pointNumber++)
                    {
                        if (!braillePoints[row][column].IsPointPresent(pointNumber))
                        {
                            continue;
                        }

                        var columnOffset = pointNumber <= 2 ? leftColumn : rightColumnOffset;

                        float rowOffset;
                        switch (pointNumber % 3)
                        {
                            case 0:
                                rowOffset = topRowOffset;
                                break;

                            case 1:
                                rowOffset = middleRowOffset;
                                break;

                            case 2:
                                rowOffset = bottomRowOffset;
                                break;

                            default:
                                throw new Exception();
                        }

                        builder.AddZFacingRoundedZylinder(
                            knobRadius,
                            knobHeight,
                            64,
                            knobRadius / 5.0f,
                            knobHeight / 2.0f,
                            16,
                            new Vector3(columnOffset, rowOffset, 0.0f) + centerOffset);
                    }
                }
            }

            builder.Build(outputPath);
        }

        public static int Main(string[] args)
        {
            var clOptions = Parser.Default.ParseArguments<CLOptions>(args).Value;

            if (clOptions == null)
            {
                return 1;
            }

            var modelName = "MyGeneratedStlCSharp";
            var outputPath = Path.Join(clOptions.OutputPath, $"{modelName}.stl");

            GenerateStl(
                modelName,
                outputPath,
                new List<List<Letter>>() { new List<Letter>() { new SixPointLetter(clOptions.BraillePoints, false) } },
                new TypesettingOptions(6, 9, 2, 4),
                OutputFormat.Text,
                new PrintOptions(true));

            return 0;
        }
    }
}