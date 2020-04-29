using System;
using System.Collections.Generic;
using System.Text;
using TransformerOptimizer.Components.Base;
using TransformerOptimizer.Parameters;
using static TransformerOptimizer.Data.Constants;
using static TransformerOptimizer.Data.Constants.GradeThicknesses;
using static TransformerOptimizer.Exceptions.Exceptions;
using static TransformerOptimizer.Data.LoadedData.Prices;
using static TransformerOptimizer.Data.LoadedData.StandardLaminations;

namespace TransformerOptimizer.Components.Factories
{
    /// <summary>
    /// Creates and stores a list of laminations based on given parameters.
    /// </summary>
    public class LaminationFactory
    {
        /// <summary>
        /// Constructor.
        /// 
        /// Generates a list of laminations based on the given parameters.
        /// As of 03/15/2019 <paramref name="yokeRange"/> does nothing as the standard laminations have a supplied dimension and cut to length laminations will determine the yoke size using <see cref="Data.Constants.DetermineYoke(CoreShape, Phase, double)"/>
        /// </summary>
        /// <param name="rangeSkips">RangeCombinationSkips representing which combinations of grades and thickness should be skipped.</param>
        /// <param name="stdLaminationsRange">IterableRange representing if both standard and cut to length laminations should be created.</param>
        /// <param name="shapeRange">IterableRange representing the range of core shapes to create with.</param>
        /// <param name="phase">Which phase to create for.</param>
        /// <param name="gradeRange">IterableRange representing the range of grades to create with.</param>
        /// <param name="thicknessRange">IterableRange representing the range of thickness to create with.</param>
        /// <param name="tongueRange">IterableRange representing the range of tongues to create with.</param>
        /// <param name="yokeRange">IterableRange representing the range of yokes to create with.</param>
        /// <param name="windowWidthRange">IterableRange representing the range of window widths to create with.</param>
        /// <param name="windowHeightRange">IterableRange representing the range of window heights to create with.</param>
        /// <param name="func">Optional parameter, a function delegate that returns a boolean, and takes two integer inputs. As of 03/15/2019 only used with <see cref="Optimizer.IncrementCurrentProcessProgress(int, int)"/>.</param>
        protected internal LaminationFactory(RangeCombinationSkips<int> rangeSkips, RangeInteger stdLaminationsRange, RangeInteger shapeRange, Phase phase, RangeInteger gradeRange, RangeInteger thicknessRange, RangeDouble tongueRange, RangeDouble yokeRange, RangeDouble windowWidthRange, RangeDouble windowHeightRange, Func<int, int, bool> func = null)
        {
            if ((tongueRange.MinValue != tongueRange.MaxValue) && tongueRange.StepSize == 0)
                throw new NoCoresFound("Tongue has different minimum and maximum values with a step size of zero.");
            if ((windowWidthRange.MinValue != windowWidthRange.MaxValue) && windowWidthRange.StepSize == 0)
                throw new NoCoresFound("Tongue has different minimum and maximum values with a step size of zero.");
            if ((windowHeightRange.MinValue != windowHeightRange.MaxValue) && windowHeightRange.StepSize == 0)
                throw new NoCoresFound("Tongue has different minimum and maximum values with a step size of zero.");
            int i = 0;
            int maxIterations = stdLaminationsRange.Iterations * shapeRange.Iterations * gradeRange.Iterations * thicknessRange.Iterations * tongueRange.Iterations * yokeRange.Iterations * windowWidthRange.Iterations * windowHeightRange.Iterations;
            Laminations = new List<Lamination>();
            windowHeightRange.NextRange = windowWidthRange;
            windowWidthRange.NextRange = yokeRange;
            yokeRange.NextRange = tongueRange;
            tongueRange.NextRange = thicknessRange;
            thicknessRange.NextRange = gradeRange;
            gradeRange.NextRange = shapeRange;
            shapeRange.NextRange = stdLaminationsRange;

            while (true)
            {
                try
                {
                    if (GetLaminationType(stdLaminationsRange.CurrentValue) == LaminationType.STANDARD)
                    {
                        if (GetGradeThickness(GetGrade(gradeRange.CurrentValue), thicknessRange.CurrentValue, out double thickness))
                        {
                            if (!rangeSkips.SkipValue(gradeRange.CurrentValue, thicknessRange.CurrentValue))
                                Laminations.AddRange(GetLaminations(GetCoreShape(shapeRange.CurrentValue), phase, GetGrade(gradeRange.CurrentValue), thickness, tongueRange.MinValue, tongueRange.MaxValue));
                        }
                        windowHeightRange.CurrentValue = windowHeightRange.MaxValue;
                        windowWidthRange.CurrentValue = windowWidthRange.MaxValue;
                        yokeRange.CurrentValue = yokeRange.MaxValue;
                        tongueRange.CurrentValue = tongueRange.MaxValue;
                    }
                    else
                    {
                        if (GetGradeThickness(GetGrade(gradeRange.CurrentValue), thicknessRange.CurrentValue, out double thickness))
                        {
                            if (!rangeSkips.SkipValue(gradeRange.CurrentValue, thicknessRange.CurrentValue))
                            {
                                Laminations.Add(new Lamination(false, "Cut-to-length", GetCoreShape(shapeRange.CurrentValue), phase, GetGrade(gradeRange.CurrentValue), thickness, tongueRange.CurrentValue,
                                    DetermineYoke(GetCoreShape(shapeRange.CurrentValue), phase, tongueRange.CurrentValue), windowWidthRange.CurrentValue, windowHeightRange.CurrentValue, 1, 1, 1, (double)GetMaterialDollarsPerPound(GetGrade(gradeRange.CurrentValue)), 0));
                            }
                        }
                    }
                    func?.Invoke(++i, maxIterations);
                    windowHeightRange.IncrementValue();
                }
                catch (IterationFinishedException) { break; }
            }
        }

        /// <summary>
        /// Constructor.
        /// 
        /// Generates a list of laminations based on the given parameters.
        /// As of 03/15/2019 <paramref name="yokeRange"/> does nothing as the standard laminations have a supplied dimension and cut to length laminations will determine the yoke size using <see cref="Data.Constants.DetermineYoke(CoreShape, Phase, double)"/>
        /// </summary>
        /// <param name="rangeLams">RangeLaminationDetails representing the range of laminations to iterate with.</param>
        /// <param name="phase">Which phase to create for.</param>
        /// <param name="tongueRange">IterableRange representing the range of tongues to create with.</param>
        /// <param name="yokeRange">IterableRange representing the range of yokes to create with.</param>
        /// <param name="windowWidthRange">IterableRange representing the range of window widths to create with.</param>
        /// <param name="windowHeightRange">IterableRange representing the range of window heights to create with.</param>
        /// <param name="func">Optional parameter, a function delegate that returns a boolean, and takes two integer inputs. As of 03/15/2019 only used with <see cref="Optimizer.IncrementCurrentProcessProgress(int, int)"/>.</param>
        protected internal LaminationFactory(RangeLaminationDetails rangeLams, Phase phase, RangeDouble tongueRange, RangeDouble yokeRange, RangeDouble windowWidthRange, RangeDouble windowHeightRange, Func<int, int, bool> func = null)
        {
            int i = 0;
            int maxIterations = rangeLams.Iterations * tongueRange.Iterations * yokeRange.Iterations * windowWidthRange.Iterations * windowHeightRange.Iterations;
            Laminations = new List<Lamination>();
            windowHeightRange.NextRange = windowWidthRange;
            windowWidthRange.NextRange = yokeRange;
            yokeRange.NextRange = tongueRange;
            tongueRange.NextRange = rangeLams;

            while (true)
            {
                try
                {
                    if (rangeLams.CurrentValue.IsStandard)
                    {
                        Laminations.AddRange(GetLaminations(rangeLams.CurrentValue.Shape, phase, rangeLams.CurrentValue.Grade, rangeLams.CurrentValue.Thickness, tongueRange.MinValue, tongueRange.MaxValue));
                        windowHeightRange.CurrentValue = windowHeightRange.MaxValue;
                        windowWidthRange.CurrentValue = windowWidthRange.MaxValue;
                        yokeRange.CurrentValue = yokeRange.MaxValue;
                        tongueRange.CurrentValue = tongueRange.MaxValue;
                    }
                    else
                    {
                        Laminations.Add(new Lamination(false, "Cut-to-length", rangeLams.CurrentValue.Shape, phase, rangeLams.CurrentValue.Grade, rangeLams.CurrentValue.Thickness, tongueRange.CurrentValue,
                            DetermineYoke(rangeLams.CurrentValue.Shape, phase, tongueRange.CurrentValue), windowWidthRange.CurrentValue, windowHeightRange.CurrentValue, 1, 1, 1, (double)GetMaterialDollarsPerPound(rangeLams.CurrentValue.Grade), 0));
                    }
                    func?.Invoke(++i, maxIterations);
                    windowHeightRange.IncrementValue();
                }
                catch (IterationFinishedException) { break; }
            }
        }

        /// <summary>
        /// List of laminations.
        /// </summary>
        protected internal List<Lamination> Laminations { get; }
    }
}
