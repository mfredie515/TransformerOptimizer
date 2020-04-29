using System;
using System.Collections.Generic;
using System.Text;
using static TransformerOptimizer.Data.Constants;
using static TransformerOptimizer.Data.LoadedData.Prices;

namespace TransformerOptimizer.Components.Base
{
    /// <summary>
    /// Contains information related to a lamination that will be used to generate a core.
    /// 
    /// The lamination can be either a standard or cut-to-length lamination, and its' other properties set based on the shape and phase provided.
    /// </summary>
    [Serializable]
    public class Lamination
    {
        /// <summary>
        /// Constructor.
        /// 
        /// If <paramref name="standardLamination"/> is true, the lamination sets its' <see cref="Surcharge"/> based on the function <see cref="Data.LoadedData.Prices.GetMaterialSurcharge(Grade)"/>.
        /// Otherwise <see cref="Surcharge"/> is set to 0 if <paramref name="standardLamination"/> is false.
        /// 
        /// If <paramref name="weight"/> is 0 then the <see cref="Weight"/> property is set using the equation <see cref="Length"/> * <see cref="Height"/> * <see cref="Thickness"/> * 0.276.
        /// </summary>
        /// <param name="standardLamination">If the lamination to be created uses a standard lamination or is cut-to-length.</param>
        /// <param name="partNumber">Part number of the lamination.</param>
        /// <param name="shape">Shape of the lamination.</param>
        /// <param name="phase">Phase of the design lamination is used in.</param>
        /// <param name="grade">Grade of steel of the lamination.</param>
        /// <param name="thickness">Thickness of steel of the lamination.</param>
        /// <param name="tongue">Width of middle leg.</param>
        /// <param name="yoke">Width of two side legs and of top and bottom yoke.</param>
        /// <param name="windowWidth">Window width.</param>
        /// <param name="windowHeight">Window height.</param>
        /// <param name="scrapFactor">Weight ratio of used laminations and bought laminations. Used only for standard laminations. Between 0 and 1.</param>
        /// <param name="desctructionFactor">Built in destruction factor of the lamination.</param>
        /// <param name="excitationFactor">Built in excitation factor of the lamination.</param>
        /// <param name="cost">Cost of the lamination, does not include surcharge.</param>
        /// <param name="weight">Weight of the lamination, if 0 then the weight will be approximated.</param>
        protected internal Lamination(bool standardLamination, string partNumber, CoreShape shape, Phase phase, Grade grade, double thickness, double tongue, double yoke, double windowWidth, double windowHeight, double scrapFactor, double desctructionFactor, double excitationFactor, double cost, double weight)
        {
            this.StandardLamination = standardLamination;
            this.LaminationPartNumber = partNumber;
            this.Shape = shape;
            this.Phase = phase;
            this.Grade = grade;
            this.Thickness = thickness;
            this.Tongue = tongue;
            this.Yoke = yoke;
            this.WindowWidth = windowWidth;
            this.WindowHeight = windowHeight;
            this.ScrapFactor = scrapFactor;
            this.DestructionFactor = desctructionFactor;
            this.ExcitationFactor = excitationFactor;
            if (standardLamination)
            {
                this.Surcharge = (double)GetMaterialSurcharge(grade);
                this.Cost = cost;
            }
            else
            {
                this.Surcharge = 0;
                this.Cost = cost;
            }
            if (weight != 0)
                this.Weight = weight;
            else
            {
                if (shape == CoreShape.FiveLegged)
                    this.Weight = ((Length * Height) - (3 * WindowWidth * WindowHeight)) * Thickness * 0.276;
                else if (shape == CoreShape.UI)
                    this.Weight = ((Length * Height) - (WindowWidth * WindowHeight)) * Thickness * 0.276;
                else
                    this.Weight = ((Length * Height) - (2 * WindowWidth * WindowHeight)) * Thickness * 0.276;
            }
        }

        /// <summary>
        /// If the lamination is a standard lamination.
        /// True = Standard, False = Cut to Length.
        /// </summary>
        public bool StandardLamination { get; }
        /// <summary>
        /// Cost of the lamination.
        /// </summary>
        public double Cost { get; }
        /// <summary>
        /// Weight of the lamination.
        /// </summary>
        public double Weight { get; }
        /// <summary>
        /// Destruction Factor of the lamination.
        /// </summary>
        public double DestructionFactor { get; }
        /// <summary>
        /// Excitation Factor of the lamination.
        /// </summary>
        public double ExcitationFactor { get; }
        /// <summary>
        /// Scrap Factor of the lamination.
        /// </summary>
        public double ScrapFactor { get; }
        /// <summary>
        /// Surcharge of the lamination material in $/Ton.
        /// </summary>
        public double Surcharge { get; }
        /// <summary>
        /// Thickness of the lamination.
        /// </summary>
        public double Thickness { get; }
        /// <summary>
        /// Tongue of the lamination.
        /// </summary>
        public double Tongue { get; }
        /// <summary>
        /// Window Width of the lamination.
        /// </summary>
        public double WindowWidth { get; }
        /// <summary>
        /// Window Height of the lamination.
        /// </summary>
        public double WindowHeight { get; }
        /// <summary>
        /// Yoke of the lamination.
        /// </summary>
        public double Yoke { get; }
        /// <summary>
        /// Lamination Shape.
        /// </summary>
        public CoreShape Shape { get; }
        /// <summary>
        /// Lamination Grade.
        /// </summary>
        public Grade Grade { get; }
        /// <summary>
        /// Lamination phase.
        /// </summary>
        public Phase Phase { get; }

        /// <summary>
        /// Returns the part number of the lamination
        /// </summary>
        public string LaminationPartNumber { get; }
        /// <summary>
        /// Returns the name of the lamination, form of 'Shape'-'Tongue'.
        /// </summary>
        public string Name { get { return Shape.ToString() + '-' + Tongue.ToString(); } }
        /// <summary>
        /// Returns the length of the lamination, taken with windows facing 'viewer' and legs vertical.
        /// </summary>
        public double Length
        {
            get
            {
                switch (Shape)
                {
                    case CoreShape.EI:
                        return Phase == Phase.SINGLE ? Tongue + 2 * WindowWidth + 2 * Yoke : 3 * Tongue + 2 * WindowWidth;
                    case CoreShape.UI:
                        return 2 * Tongue + WindowWidth;
                    case CoreShape.FiveLegged:
                        return 4 * Tongue + 3 * WindowWidth;
                    default:
                        return 0;
                }
            }
        }
        /// <summary>
        /// Returns the height of the lamination, taken with windows facing 'viewer' and legs vertical.
        /// </summary>
        public double Height
        {
            get
            {
                return 2 * Yoke + WindowHeight;
            }
        }
        /// <summary>
        /// Returns the build factor of the lamination, used to determine the correct build percentage of a design as single phase EI, three phase EI, and UI utilize different window percentages per coil.        
        /// </summary>
        public double BuildFactor
        {
            get
            {
                switch (Shape)
                {
                    /*                      
                     * Single Phase EI
                     * ---------------
                     * |   +++|+++   |
                     * |   +++|+++   |
                     * |   +++|+++   |
                     * ---------------
                     * 
                     *   Three Phase EI
                     *   ---------------
                     * ++|++  ++|++  ++|++
                     * ++|++  ++|++  ++|++
                     * ++|++  ++|++  ++|++
                     *   ---------------
                     *   
                     *  Single Phase UI
                     *   --------
                     * ++|++  ++|++
                     * ++|++  ++|++
                     * ++|++  ++|++
                     *   --------
                    */
                    case CoreShape.EI:
                        return Phase == Phase.SINGLE ? 1 : 0.5;
                    case CoreShape.UI:
                        return 0.5;
                    default:
                        return 1;
                }
            }
        }
        /// <summary>
        /// Determines if the lamination is scrapless or not.
        /// 
        /// For Single Phase, scrapless laminations have a window height and window width ratio of 3.
        /// For Three Phase, scrapless laminations have a window height and window width ratio of 2.5.
        /// </summary>
        public bool ScraplessLamination { get { return (Phase == Phase.SINGLE ? WindowHeight / WindowWidth == 3 : WindowHeight / WindowWidth == 2.5); } }
    }
}
