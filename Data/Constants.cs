using System;
using System.Collections.Generic;
using System.Text;

namespace TransformerOptimizer.Data
{
    /// <summary>
    /// Static class that holds constants used thoughtout the program.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Represents a lamination to iterate.
        /// </summary>
        public class LaminationDetails
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="isStd">If the lamination would be a standard lamination or not.</param>
            /// <param name="shape">Shape of the lamination.</param>
            /// <param name="grade">Grade of the lamination.</param>
            /// <param name="thickness">Lamination thickness.</param>
            public LaminationDetails(bool isStd, CoreShape shape, Grade grade, double thickness)
            {
                this.IsStandard = isStd;
                this.Shape = shape;
                this.Grade = grade;
                this.Thickness = thickness;
            }

            /// <summary>
            /// If the lamination would be a standard lamination or not.
            /// </summary>
            public bool IsStandard { get; private set; }
            /// <summary>
            /// Shape of the lamination.
            /// </summary>
            public CoreShape Shape { get; private set; }
            /// <summary>
            /// Grade of the lamination.
            /// </summary>
            public Grade Grade { get; private set; }
            /// <summary>
            /// Lamination thickness.
            /// </summary>
            public double Thickness { get; private set; }
        }

        /// <summary>
        /// How the ducts will be placed around the winding/layer.
        /// </summary>
        public enum DuctLocation
        {
            /// <summary>
            /// No ducts will be placed; this shouldn't be needed.
            /// </summary>
            NONE = 0,
            /// <summary>
            /// Ducts will be placed at the front and back of the coil.
            /// 
            /// This will not affect the build.
            /// </summary>
            FRONT_AND_BACK,
            /// <summary>
            /// Ducts will be placed on all sides of the coil.
            /// 
            /// This will affect the build.
            /// </summary>
            ALL_AROUND,
            /// <summary>
            /// Ducts will be placed on three sides of the coil; use this only with UI cores.
            /// 
            /// This will not affect the build.
            /// </summary>
            THREE_SIDED
        }
        /// <summary>
        /// Returns <see cref="DuctLocation"/> from user input.
        ///
        /// As of 04/09/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Integer value in enum list.</param>
        /// <returns>Value of <see cref="DuctLocation"/> corresponding to user input.</returns>
        public static DuctLocation GetDuctLocation(int value)
        {
            return (DuctLocation)value;
        }
        /// <summary>
        /// Returns <see cref="DuctLocation"/> from user input.
        ///
        /// As of 04/09/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Double value converted to integer in enum list.</param>
        /// <returns>Value of <see cref="DuctLocation"/> corresponding to user input.</returns>
        public static DuctLocation GetDuctLocation(double value)
        {
            return (DuctLocation)(int)value;
        }
        /// <summary>
        /// Returns <see cref="DuctLocation"/> from user input.
        /// 
        /// As of 04/09/2019 no input checking is provided, this method defaults to <see cref="DuctLocation.NONE"/>.
        /// </summary>
        /// <param name="value">Enum value to find with name of.</param>
        /// <returns>Value of <see cref="DuctLocation"/> corresponding to user input.</returns>
        public static DuctLocation GetDuctLocation(string value)
        {
            DuctLocation ductLocation = DuctLocation.NONE;
            if (Enum.TryParse(value, out ductLocation))
                return ductLocation;
            return DuctLocation.NONE;
        }
        /// <summary>
        /// Type of lamination, either standard or cut-to-length.
        /// </summary>
        public enum LaminationType
        {
            /// <summary>
            /// Lamination will be loaded lamination from <see cref="LoadedData.StandardLaminations"/> class.
            /// </summary>
            STANDARD,
            /// <summary>
            /// Lamination will be created by user entered data.
            /// </summary>
            CUT_TO_LENGTH
        }
        /// <summary>
        /// Returns <see cref="LaminationType"/> from user input.
        ///
        /// As of 03/18/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Integer value in enum list.</param>
        /// <returns>Value of <see cref="LaminationType"/> corresponding to user input.</returns>
        public static LaminationType GetLaminationType(int value)
        {
            return (LaminationType)value;
        }
        /// <summary>
        /// Returns <see cref="LaminationType"/> from user input.
        ///
        /// As of 03/18/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Double value converted to integer in enum list.</param>
        /// <returns>Value of <see cref="LaminationType"/> corresponding to user input.</returns>
        public static LaminationType GetLaminationType(double value)
        {
            return (LaminationType)(int)value;
        }
        /// <summary>
        /// Returns <see cref="LaminationType"/> from user input.
        /// 
        /// As of 03/18/2019 no input checking is provided, this method defaults to <see cref="LaminationType.CUT_TO_LENGTH"/>.
        /// </summary>
        /// <param name="value">Enum value to find with name of.</param>
        /// <returns>Value of <see cref="LaminationType"/> corresponding to user input.</returns>
        public static LaminationType GetLaminationType(string value)
        {
            LaminationType laminationType = LaminationType.CUT_TO_LENGTH;
            if (Enum.TryParse(value, out laminationType))
                return laminationType;
            return LaminationType.CUT_TO_LENGTH;
        }
        /// <summary>
        /// Determines what the output of calling ToString() on an optimizer object is. As of 04/01/2019 this enum is not is use.
        /// </summary>
        public enum ToStringType
        {
            /// <summary>
            /// Print a basic summary of the object.
            /// </summary>
            BASIC,
            /// <summary>
            /// Print an enhanced summary of the object.
            /// </summary>            
            VERBOSE
        }
        /// <summary>
        /// What types of core shapes are available.
        /// </summary>
        public enum CoreShape
        {
            /// <summary>
            /// Lamination consisting of an E-piece and an I-piece.
            /// </summary>
            EI,
            /// <summary>
            /// Lamination consisting of a U-piece and an I-piece.
            /// </summary>
            UI,
            /// <summary>
            /// Five legged lamination, should probably only be used with three phase cut-to-length laminations.
            /// </summary>
            FiveLegged
        }
        /// <summary>
        /// Returns <see cref="CoreShape"/> from user input.
        ///
        /// As of 04/01/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Integer value in enum list.</param>
        /// <returns>Type of core shape.</returns>
        public static CoreShape GetCoreShape(int value)
        {
            return (CoreShape)value;
        }
        /// <summary>
        /// Returns <see cref="CoreShape"/> from user input.
        ///
        /// As of 04/01/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Double value converted to integer in enum list.</param>
        /// <returns>Type of core shape.</returns>
        public static CoreShape GetCoreShape(double value)
        {
            return (CoreShape)(int)value;
        }
        /// <summary>
        /// Returns <see cref="CoreShape"/> from user input.
        ///
        /// As of 04/01/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Enum value to find with name of.</param>
        /// <returns>Type of core shape.</returns>
        public static CoreShape GetCoreShape(string value)
        {
            CoreShape coreShape = CoreShape.EI;
            if (Enum.TryParse(value, out coreShape))
                return coreShape;
            return CoreShape.EI;
        }
        /// <summary>
        /// Different styles of how a UI lamination design can have its' windings connected.
        /// </summary>
        public enum UIStyle
        {
            /// <summary>
            /// Default style passed to a design.
            /// </summary>
            NA = -1,
            /// <summary>
            /// Windings to be connected in series.
            /// 
            /// Voltage in each winding is one half the total.
            /// Current in each winding is equal to the total.
            /// Turns in each winding are one half the total.
            /// </summary>
            SERIES = 0,
            /// <summary>
            /// Windings to be connected in parallel.
            /// 
            /// Voltage in each winding is equal to the total.
            /// Current in each winding is one half the total.
            /// Turns in each winding are equal to the total.
            /// </summary>
            PARALLEL
        }
        /// <summary>
        /// Returns <see cref="UIStyle"/> from user input.
        ///
        /// As of 04/01/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Integer value in enum list.</param>
        /// <returns>Type of the UI style.</returns>
        public static UIStyle GetUIStyle(int value)
        {
            return (UIStyle)value;
        }
        /// <summary>
        /// Returns <see cref="UIStyle"/> from user input.
        ///
        /// As of 04/01/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Double value converted to integer in enum list.</param>
        /// <returns>Type of the UI style.</returns>
        public static UIStyle GetUIStyle(double value)
        {
            return (UIStyle)(int)value;
        }
        /// <summary>
        /// Returns <see cref="UIStyle"/> from user input.
        ///
        /// As of 04/01/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Enum value to find with name of.</param>
        /// <returns>Type of the UI style.</returns>
        public static UIStyle GetUIStyle(string value)
        {
            UIStyle uiStyle = UIStyle.SERIES;
            if (Enum.TryParse(value, out uiStyle))
                return uiStyle;
            return UIStyle.SERIES;
        }
        /// <summary>
        /// Different phases that the transformer is rated for.
        /// </summary>
        public enum Phase
        {
            /// <summary>
            /// Program assumes one coil on middle leg of core for EI and two coils for UI.
            /// </summary>
            SINGLE,
            /// <summary>
            /// Program assumes three coils, one on each leg of core.
            /// </summary>
            THREE
        }
        /// <summary>
        /// Returns <see cref="Phase"/> from user input.
        ///
        /// As of 04/01/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Integer value in enum list.</param>
        /// <returns>Corresponding phase value.</returns>
        public static Phase GetPhase(int value)
        {
            return (Phase)value;
        }
        /// <summary>
        /// Returns <see cref="Phase"/> from user input.
        ///
        /// As of 04/01/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Double value converted to integer in enum list.</param>
        /// <returns>Corresponding phase value.</returns>
        public static Phase GetPhase(double value)
        {
            return (Phase)(int)value;
        }
        /// <summary>
        /// Returns <see cref="Phase"/> from user input.
        ///
        /// As of 04/01/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Enum value to find with name of.</param>
        /// <returns>Corresponding phase value.</returns>
        public static Phase GetPhase(string value)
        {
            if (value == "1" || value == "SINGLE")
                return Phase.SINGLE;
            else if (value == "3" || value == "THREE")
                return Phase.THREE;
            else
            {
                Phase phase = Phase.SINGLE;
                if (Enum.TryParse(value, out phase))
                    return phase;
                return Phase.SINGLE;
            }
        }
        /// <summary>
        /// Different connection types a winding can have.
        /// </summary>
        public enum Connection
        {
            /// <summary>
            /// Winding is not connected in any special way.
            /// 
            /// All single phase designs should use this connection type.
            /// </summary>
            OPEN,
            /// <summary>
            /// Windings are connected in a delta fashion.
            /// </summary>
            DELTA,
            /// <summary>
            /// Windings are connected in a wye fashion.
            /// </summary>
            WYE
        }
        /// <summary>
        /// Returns <see cref="Connection"/> from user input.
        ///
        /// As of 04/03/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Integer value in enum list.</param>
        /// <returns>Corresponding connection value.</returns>
        public static Connection GetConnection(int value)
        {
            return (Connection)value;
        }
        /// <summary>
        /// Returns <see cref="Connection"/> from user input.
        ///
        /// As of 04/03/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Double value converted to integer in enum list.</param>
        /// <returns>Corresponding connection value.</returns>
        public static Connection GetConnection(double value)
        {
            return (Connection)(int)value;
        }
        /// <summary>
        /// Returns <see cref="Connection"/> from user input.
        ///
        /// As of 04/03/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Enum value to find with name of.</param>
        /// <returns>Corresponding connection value.</returns>
        public static Connection GetConnection(string value)
        {
            Connection connection = Connection.OPEN;
            if (Enum.TryParse(value, out connection))
                return connection;
            return Connection.OPEN;
        }
        /// <summary>
        /// Grade of the lamination to be used.
        /// </summary>
        public enum Grade
        {
            /// <summary>
            /// Possible sizes: 29M6.
            /// </summary>
            M6,
            /// <summary>
            /// Possible sizes: 26M19, 24M19.
            /// </summary>
            M19,
            /// <summary>
            /// Possible sizes: 26M50, 24M50.
            /// </summary>
            M50,
            /// <summary>
            /// Possible sizes: 29M12.
            /// </summary>
            M12,
            /// <summary>
            /// Possible sizes: 0.009
            /// </summary>
            M085_ButtLap,
            /// <summary>
            /// Possible sizes: 0.009
            /// </summary>
            M085_StepLap
        }
        /// <summary>
        /// Returns <see cref="Grade"/> from user input.
        ///
        /// As of 04/04/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Integer value in enum list.</param>
        /// <returns>Corresponding grade value.</returns>
        public static Grade GetGrade(int value)
        {
            return (Grade)value;
        }
        /// <summary>
        /// Returns <see cref="Grade"/> from user input.
        ///
        /// As of 04/04/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Double value converted to integer in enum list.</param>
        /// <returns>Corresponding grade value.</returns>
        public static Grade GetGrade(double value)
        {
            return (Grade)(int)value;
        }
        /// <summary>
        /// Returns <see cref="Grade"/> from user input.
        ///
        /// As of 04/04/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Enum value to find with name of.</param>
        /// <returns>Corresponding grade value.</returns>
        public static Grade GetGrade(string value)
        {
            Grade grade = Grade.M6;
            if (Enum.TryParse(value, out grade))
                return grade;
            return Grade.M6;
        }
        /// <summary>
        /// Thickness of the lamination to be used.
        /// </summary>
        public enum LaminationThickness
        {
            /// <summary>
            /// 0.014
            /// </summary>
            _29,
            /// <summary>
            /// 0.0185
            /// </summary>
            _26,
            /// <summary>
            /// 0.025
            /// </summary>
            _24
        }
        /// <summary>
        /// Returns <see cref="LaminationThickness"/> from user input.
        ///
        /// As of 04/04/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Integer value in enum list.</param>
        /// <returns>Corresponding lamination thickness value.</returns>
        public static LaminationThickness GetLaminationThickness(int value)
        {
            return (LaminationThickness)value;
        }
        /// <summary>
        /// Returns <see cref="LaminationThickness"/> from user input.
        ///
        /// As of 04/04/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Double value converted to integer in enum list.</param>
        /// <returns>Corresponding lamination thickness value.</returns>
        public static LaminationThickness GetLaminationThickness(double value)
        {
            return (LaminationThickness)(int)value;
        }
        /// <summary>
        /// Returns <see cref="LaminationThickness"/> from user input.
        ///
        /// As of 04/04/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Enum value to find with name of.</param>
        /// <returns>Corresponding lamination thickness value.</returns>
        public static LaminationThickness GetLaminationThickness(string value)
        {
            LaminationThickness laminationThickness = LaminationThickness._24;
            if (Enum.TryParse(value, out laminationThickness))
                return laminationThickness;
            return LaminationThickness._24;
        }
        /// <summary>
        /// Different materials that a wire can be made of.
        /// </summary>
        public enum WireMaterial
        {
            /// <summary>
            /// Wire made up of aluminum. Currently only rectangular aluminum wire is supported.
            /// </summary>
            ALUMINUM,
            /// <summary>
            /// Wire made up of copper. Currently both round and rectangular wires are supported.
            /// </summary>
            COPPER,
            /// <summary>
            /// Wire made up of copper or aluminum. Round and rectangular wires are determined during iteration.
            /// </summary>
            ANY
        }
        /// <summary>
        /// Returns <see cref="WireMaterial"/> from user input.
        ///
        /// As of 04/04/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Integer value in enum list.</param>
        /// <returns>Corresponding wire material value.</returns>
        public static WireMaterial GetWireMaterial(int value)
        {
            return (WireMaterial)value;
        }
        /// <summary>
        /// Returns <see cref="WireMaterial"/> from user input.
        ///
        /// As of 04/04/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Double value converted to integer in enum list.</param>
        /// <returns>Corresponding wire material value.</returns>
        public static WireMaterial GetWireMaterial(double value)
        {
            return (WireMaterial)(int)value;
        }
        /// <summary>
        /// Returns <see cref="WireMaterial"/> from user input.
        ///
        /// As of 04/04/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Enum value to find with name of.</param>
        /// <returns>Corresponding wire material value.</returns>
        public static WireMaterial GetWireMaterial(string value)
        {
            WireMaterial wireMaterial = WireMaterial.COPPER;
            if (Enum.TryParse(value, out wireMaterial))
                return wireMaterial;
            return WireMaterial.COPPER;
        }
        /// <summary>
        /// Different shapes that a wire can be made of.
        /// </summary>
        public enum WireShape
        {
            /// <summary>
            /// Round wire shape, currently only supported by copper wires.
            /// </summary>
            ROUND,
            /// <summary>
            /// Rectangular wire shape, currently supported by both aluminum and copper wires.
            /// </summary>
            RECTANGULAR,
            /// <summary>
            /// Foil wrapping instead of wire, currently supported by both aluminum and copper.
            /// </summary>
            FOIL,
            /// <summary>
            /// Either round or rectangular shape, shape is determined in iteration.
            /// </summary>
            ANY
        }
        /// <summary>
        /// Returns <see cref="WireShape"/> from user input.
        ///
        /// As of 04/04/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Integer value in enum list.</param>
        /// <returns>Corresponding wire shape value.</returns>
        public static WireShape GetWireShape(int value)
        {
            return (WireShape)value;
        }
        /// <summary>
        /// Returns <see cref="WireShape"/> from user input.
        ///
        /// As of 04/04/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Double value converted to integer in enum list.</param>
        /// <returns>Corresponding wire shape value.</returns>
        public static WireShape GetWireShape(double value)
        {
            return (WireShape)(int)value;
        }
        /// <summary>
        /// Returns <see cref="WireShape"/> from user input.
        ///
        /// As of 04/04/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Enum value to find with name of.</param>
        /// <returns>Corresponding wire shape value.</returns>
        public static WireShape GetWireShape(string value)
        {
            WireShape wireShape = WireShape.RECTANGULAR;
            if (Enum.TryParse(value, out wireShape))
                return wireShape;
            return WireShape.RECTANGULAR;
        }
        /// <summary>
        /// Arrangement of combining mutliple wires into one.
        /// </summary>
        public enum Bifilar
        {
            /// <summary>
            /// Default wire choice, a single wire.
            /// </summary>
            _1H1W,
            /// <summary>
            /// Two wires side by side.
            /// </summary>
            _1H2W,
            /// <summary>
            /// Three wires side by side.
            /// </summary>
            _1H3W,
            /// <summary>
            /// Four wires side by side.
            /// </summary>
            _1H4W,
            /// <summary>
            /// X wires stacked on top of each other, should be used in Foil windings only.
            /// </summary>
            _XH1W
        }
        /// <summary>
        /// Returns <see cref="Bifilar"/> from user input.
        ///
        /// As of 04/04/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Integer value in enum list.</param>
        /// <returns>Corresponding bifilar value.</returns>
        public static Bifilar GetBifilar(int value)
        {
            return (Bifilar)value;
        }
        /// <summary>
        /// Returns <see cref="Bifilar"/> from user input.
        ///
        /// As of 04/04/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Double value converted to integer in enum list.</param>
        /// <returns>Corresponding bifilar value.</returns>
        public static Bifilar GetBifilar(double value)
        {
            return (Bifilar)(int)value;
        }
        /// <summary>
        /// Returns <see cref="Bifilar"/> from user input.
        ///
        /// As of 04/04/2019 no input checking is provided.
        /// </summary>
        /// <param name="value">Enum value to find with name of.</param>
        /// <returns>Corresponding bifilar value.</returns>
        public static Bifilar GetBifilar(string value)
        {
            Bifilar bifilar = Bifilar._1H1W;
            if (Enum.TryParse(value, out bifilar))
                return bifilar;
            return Bifilar._1H1W;
        }

        /// <summary>
        /// Contains the different thickness that each grade is able to use.
        /// </summary>
        public static class GradeThicknesses
        {
            static List<LaminationThickness> _m6Thicknesses { get; }
            static List<LaminationThickness> _m19Thicknesses { get; }
            static List<LaminationThickness> _m50Thicknesses { get; }

            static GradeThicknesses()
            {
                _m6Thicknesses = new List<LaminationThickness>
                {
                    LaminationThickness._29
                };
                _m19Thicknesses = new List<LaminationThickness>
                {
                    LaminationThickness._26,
                    LaminationThickness._24
                };
                _m50Thicknesses = new List<LaminationThickness>
                {
                    LaminationThickness._26,
                    LaminationThickness._24
                };
            }

            /// <summary>
            /// Return the corresponding thickness for a certain grade and lamination thickness.
            /// 
            /// As of 04/04/2019 this should probably be an extension method and enum attributes.
            /// </summary>
            /// <param name="grade">The grade of the lamination.</param>
            /// <param name="thicknessIndex">The index of the lamination thickness.</param>
            /// <param name="thickness">Out parameter for the corresponding thickness.</param>
            /// <returns>Whether given <paramref name="grade"/> and <paramref name="thicknessIndex"/> are valid combinations.</returns>
            public static bool GetGradeThickness(Grade grade, int thicknessIndex, out double thickness)
            {
                switch (grade)
                {
                    case Grade.M6:
                        if (thicknessIndex == 0)
                        {
                            thickness = 0.014;
                            return true;
                        }
                        else
                        {
                            thickness = 0;
                            return false;
                        }
                    case Grade.M12:
                        if (thicknessIndex == 0)
                        {
                            thickness = 0.014;
                            return true;
                        }
                        else
                        {
                            thickness = 0;
                            return false;
                        }
                    case Grade.M19:
                        if (thicknessIndex == 1)
                        {
                            thickness = 0.0185;
                            return true;
                        }
                        else if (thicknessIndex == 2)
                        {
                            thickness = 0.024;
                            return true;
                        }
                        else
                        {
                            thickness = 0;
                            return false;
                        }
                    case Grade.M50:
                        if (thicknessIndex == 1)
                        {
                            thickness = 0.0185;
                            return true;
                        }
                        else if (thicknessIndex == 2)
                        {
                            thickness = 0.024;
                            return true;
                        }
                        else
                        {
                            thickness = 0;
                            return false;
                        }
                    default:
                        thickness = 0;
                        return false;
                }
            }
        }

        /// <summary>
        /// Contains different factors that lamination grades have on calculated values.
        /// </summary>
        public static class CoreFactors
        {
            static CoreFactors()
            {
                M6DestructionFactor = 1.9;
                M6ExcitationFactor = 1.0179;
                M19DestructionFactor = 1.33;
                M19ExcitationFactor = 1.0197;
                M50DestructionFactor = 1;
                M50ExcitationFactor = 1.0187;
            }

            /// <summary>
            /// The native destruction factor for M6 laminations.
            /// </summary>
            public static double M6DestructionFactor { get; }
            /// <summary>
            /// The native destruction factor for M19 laminations.
            /// </summary>
            public static double M19DestructionFactor { get; }
            /// <summary>
            /// The native destruction factor for M50 laminations.
            /// </summary>
            public static double M50DestructionFactor { get; }
            /// <summary>
            /// The native excitation factor for M6 laminations.
            /// </summary>
            public static double M6ExcitationFactor { get; }
            /// <summary>
            /// The native excitation factor for M19 laminations.
            /// </summary>
            public static double M19ExcitationFactor { get; }
            /// <summary>
            /// The native excitation factor for M50 laminations.
            /// </summary>
            public static double M50ExcitationFactor { get; }

            /// <summary>
            /// Returns the native destruction factor for a given grade. If the grade is not found, 9999 is returned, which should fail any design.
            /// 
            /// As of 04/04/2019 ideally this should probably be an extension method and enum attribute.
            /// </summary>
            /// <param name="grade">Grade to lookup.</param>
            /// <returns>The destruction factor of the given grade.</returns>
            public static double GetDestructionFactor(Grade grade)
            {
                switch (grade)
                {
                    case Grade.M6:
                        return M6DestructionFactor;
                    case Grade.M19:
                        return M19DestructionFactor;
                    case Grade.M50:
                        return M50DestructionFactor;
                    default:
                        return 9999;
                }
            }

            /// <summary>
            /// Returns the native excitation factor for a given grade. If the grade is not found, 9999 is returned, which should fail any design.
            /// 
            /// As of 04/04/2019 ideally this should probably be an extension method and enum attribute.
            /// </summary>
            /// <param name="grade">Grade to lookup.</param>
            /// <returns>The excitation factor of the given grade.</returns>
            public static double GetExcitationFactor(Grade grade)
            {
                switch (grade)
                {
                    case Grade.M6:
                        return M6ExcitationFactor;
                    case Grade.M19:
                        return M19ExcitationFactor;
                    case Grade.M50:
                        return M50ExcitationFactor;
                    default:
                        return 9999;
                }
            }
        }

        /// <summary>
        /// Determines the yoke size of a given lamination.
        /// 
        /// As of 04/04/2019 there is no support for custom yoke size and standard laminations are not given a yoke size.
        /// </summary>
        /// <param name="coreShape">Shape of the lamination.</param>
        /// <param name="phase">Phase of the design.</param>
        /// <param name="tongue">Tongue size of the lamination.</param>
        /// <returns>The yoke size for the lamination.</returns>
        public static double DetermineYoke(CoreShape coreShape, Phase phase, double tongue)
        {
            if (coreShape == CoreShape.EI && phase == Phase.SINGLE)
                return tongue / 2;
            else
                return tongue;
        }
    }
}
