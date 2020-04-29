using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using TransformerOptimizer.Components.Base;
using TransformerOptimizer.Components.Derived;
using static TransformerOptimizer.Functions.Functions;
using static TransformerOptimizer.Data.Constants;
using static TransformerOptimizer.Data.Constants.CoreFactors;
using System.Data.SQLite;

namespace TransformerOptimizer.Data
{
    /// <summary>
    /// Static class that contains other static class regarding loaded data.
    /// </summary>
    public static class LoadedData
    {
        //Path to the sqlite database
        private const string DATABASE_CONNECTION_STRING = @"Data Source=ComponentData.db;Version=3;";
        //Path to xml document
        private const string XML_FILE_PATH = @"Settings.xml";

        /// <summary>
        /// Static class that contains a list of all standard wires. 
        /// </summary>
        public static class Wires
        {
            /// <summary>
            /// List of wires.
            /// </summary>
            private static List<Wire> _wires { get; }

            /// <summary>
            /// Static constructor.
            ///
            /// Loads wires from an optimizer database.
            /// As of 03/15/2019 it only loads from a particular database.
            /// </summary>
            static Wires()
            {
                _wires = new List<Wire>();
                

                using (SQLiteConnection con = new SQLiteConnection(@DATABASE_CONNECTION_STRING, true))
                {
                    con.Open();
                    SQLiteCommand sqlCom = new SQLiteCommand("SELECT [wire_name], [wire_material], [wire_shape], [wire_width], [wire_thickness], [wire_insulation], [wire_resistance], [wire_weight], [wire_price], [skew_factor] FROM wire_data", con);
                    SQLiteDataReader reader = sqlCom.ExecuteReader();
                    while (reader.Read())
                    {
                        _wires.Add(new Wire(reader["wire_name"].ToString(), reader["wire_material"].ToString() == "COPPER" ? WireMaterial.COPPER : WireMaterial.ALUMINUM, reader["wire_shape"].ToString() == "ROUND" ? WireShape.ROUND : WireShape.RECTANGULAR, Bifilar._1H1W, double.Parse(reader["wire_width"].ToString()), double.Parse(reader["wire_thickness"].ToString()), double.Parse(reader["wire_insulation"].ToString()), double.Parse(reader["wire_resistance"].ToString()), double.Parse(reader["wire_weight"].ToString()), double.Parse(reader["wire_price"].ToString()), int.Parse(reader["skew_factor"].ToString())));
                    }
                }
            }

            /// <summary>
            /// Returns a list of wires that pass the given parameters.
            /// 
            /// All wires will be iterated of the range of bifilars supplied.            
            /// As only 1H1W wires are considered standard, if a bifilar would create a passing wire with a bifilar that is not 1H1W, that wire will be created.
            /// 
            /// Note: Round wires can only have a bifilar of 1H1W, if bifilar range contains values other than 1H1W, those values will be skipped for round wires.
            /// 
            /// As of 04/22/2019 with the addition of the section parameter and min and max CD checking, this would make sense to become an extension method.
            /// </summary>
            /// <param name="wireMaterial">Wires of certain material to be returned, if ANY, then all materials are looked at.</param>
            /// <param name="wireShapes">Wires of certain shape to be returned, if ANY, then all shapes are looked at.</param>
            /// <param name="bifilars">Range of bifilars to iterate over.</param>
            /// <param name="minimumCSA">Minimum cross sectional area of the wire.</param>
            /// <param name="maximumCSA">Maximum cross sectional area of the wire.</param>
            /// <param name="section">Optional, section wires are determined for; used for checking max and min current density ranges for any material.</param>
            /// <returns></returns>
            public static List<Wire> GetWires(WireMaterial wireMaterial, WireShape[] wireShapes, Bifilar[] bifilars, double minimumCSA, double maximumCSA, Section section = null)
            {
                List<Wire> _wires = new List<Wire>();

                WireMaterial[] materials;
                if (wireMaterial == WireMaterial.ANY)
                    materials = new WireMaterial[] { WireMaterial.COPPER, WireMaterial.ALUMINUM };
                else
                    materials = new WireMaterial[] { wireMaterial };

                foreach (var material in materials)
                {
                    foreach (var shape in wireShapes)
                    {
                        foreach (var bifilar in bifilars)
                        {
                            if (bifilar != Bifilar._1H1W && shape != WireShape.RECTANGULAR)
                                break;
                            if (shape == WireShape.FOIL)
                            {
                                List<Foil> foils = new List<Foil>();
                                for (double d = section.CurrentDensityMinimum.CurrentDensityMaterialFactor(material); d <= section.CurrentDensityMaximum.CurrentDensityMaterialFactor(material); d += 100)
                                    foils.Add(new Foil(section.SectionCurrent / d, material));
                                _wires.AddRange(foils);
                            }
                            else
                            {
                                IEnumerable<Wire> query;
                                if (bifilar == Bifilar._1H1W)
                                    query = Wires._wires.Where(w => w.WireShape == shape && w.WireMaterial == material && w.CrossSectionalArea >= section.SectionCurrent / section.CurrentDensityMaximum.CurrentDensityMaterialFactor(material) / BifilarMultiplier(bifilar) && w.CrossSectionalArea <= section.SectionCurrent / section.CurrentDensityMinimum.CurrentDensityMaterialFactor(material) / BifilarMultiplier(bifilar));
                                else
                                    query = from wire in Wires._wires.Where(w => w.WireShape == shape && w.WireMaterial == material && w.CrossSectionalArea >= section.SectionCurrent / section.CurrentDensityMaximum.CurrentDensityMaterialFactor(material) / BifilarMultiplier(bifilar) && w.CrossSectionalArea <= section.SectionCurrent / section.CurrentDensityMinimum.CurrentDensityMaterialFactor(material) / BifilarMultiplier(bifilar))
                                            select new Wire(wire.Name, wire.WireMaterial, wire.WireShape, bifilar, wire.ConductorWidth, wire.ConductorThickness, wire.InsulationThickness, wire.ResistancePer1000Inches, wire.WeightPer1000Inches, wire.Cost, wire.SkewFactor);

                                _wires.AddRange(query);
                            }
                        }
                    }
                }

                return _wires;
            }
        }

        /// <summary>
        /// Static class that contains a list of all standard tubes. 
        /// </summary>
        public static class Tubes
        {
            /// <summary>
            /// List of tubes.
            /// </summary>
            private static List<Tube> _tubes { get; }

            /// <summary>
            /// Static constructor.
            ///
            /// Loads tubes from an optimizer database.
            /// As of 03/15/2019 it only loads from a particular database.
            /// </summary>
            static Tubes()
            {
                _tubes = new List<Tube>();

                using (SQLiteConnection con = new SQLiteConnection(@DATABASE_CONNECTION_STRING, true))
                {
                    con.Open();
                    SQLiteCommand sqlCom = new SQLiteCommand("SELECT [tube_number], [tube_width], [tube_depth], [tube_thickness], [tube_length] FROM tubes", con);
                    SQLiteDataReader reader = sqlCom.ExecuteReader();
                    while (reader.Read())
                    {
                        _tubes.Add(new Tube(reader["tube_number"].ToString(), double.Parse(reader["tube_width"].ToString()), double.Parse(reader["tube_depth"].ToString()), double.Parse(reader["tube_length"].ToString()), double.Parse(reader["tube_thickness"].ToString()), 0, 0));
                    }
                }
            }

            /// <summary>
            /// Gets the tube that fits the closest for a given core.
            /// 
            /// Checks tubes both the standard way and if the tubes would be rotated.
            /// </summary>
            /// <param name="tongue">Core/Lamination tongue size.</param>
            /// <param name="stack">Core stack.</param>
            /// <param name="tube">out parameter, the tube that is closest to fitting with the given parameters.</param>
            /// <returns>If the tube is rotated or not. True = rotated, False = standard.</returns>
            public static bool GetTube(double tongue, double stack, out Tube tube)
            {
                IEnumerable<Tube> standard = _tubes.Where(w => w.Width >= tongue && w.Depth >= stack).OrderBy(t => t.Width).ThenBy(x => x.Depth).Where(z => z.Width >= tongue + 0.03125 && z.Width <= tongue + 0.125 && z.Depth >= stack + 0.03125 && z.Depth <= stack + 0.5);
                IEnumerable<Tube> rotated = _tubes.Where(w => w.Width >= stack && w.Depth >= tongue).OrderBy(t => t.Depth).ThenBy(x => x.Width).Where(z => z.Width >= stack + 0.03125 && z.Width <= stack + 0.125 && z.Depth >= tongue + 0.03125 && z.Depth <= tongue + 0.5);

                if (standard.Count() >= 1 && rotated.Count() >= 1)
                {
                    tube = GetClosestTube(standard.First(), rotated.First());
                    return tube == rotated.First() ? true : false;
                }
                else if (standard.Count() >= 1)
                {
                    tube = standard.First();
                    return false;
                }
                else if (rotated.Count() >= 1)
                {
                    tube = rotated.First();
                    return true;
                }
                else
                {
                    tube = new Tube("Handmade Tube", tongue + 0.0625, stack + 0.0625, 0, 0.0625, 0.02, 0);
                    return false;
                }
            }

            /// <summary>
            /// Determines which tube would provide the closest fit.
            /// 
            /// No core/lamination details are needed because the two tubes would already be the closest matches to those details.
            /// </summary>
            /// <param name="standard">Tube that is not rotated.</param>
            /// <param name="rotated">Tube that is rotated.</param>
            /// <returns>Whichever tube is the closest fit.</returns>
            public static Tube GetClosestTube(Tube standard, Tube rotated)
            {
                if (standard.Width > rotated.Depth)
                    return rotated;
                else if (standard.Width < rotated.Depth)
                    return standard;
                else
                {
                    if (standard.Depth > rotated.Width)
                        return rotated;
                    else if (standard.Depth < rotated.Width)
                        return standard;
                    else
                    {
                        if (standard.Thickness > rotated.Thickness)
                            return rotated;
                        else if (standard.Thickness < rotated.Thickness)
                            return standard;
                        else
                        {
                            if (standard.Cost > rotated.Cost)
                                return rotated;
                            else if (standard.Cost < rotated.Cost)
                                return standard;
                            else
                                return standard;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Static class that contains a list of all standard laminations. 
        /// </summary>
        public static class StandardLaminations
        {
            /// <summary>
            /// List of laminations.
            /// </summary>
            private static List<Lamination> __laminations { get; }

            /// <summary>
            /// Static constructor.
            ///
            /// Loads laminations from an optimizer database.
            /// As of 03/15/2019 it only loads from a particular database.
            /// </summary>
            static StandardLaminations()
            {
                __laminations = new List<Lamination>();

                using (SQLiteConnection sqlCon = new SQLiteConnection(@DATABASE_CONNECTION_STRING, true))
                {
                    sqlCon.Open();
                    SQLiteCommand sqlCom = new SQLiteCommand("SELECT [name], [tongue], [phase], [width], [height], [base_price], [weight], [shape], [thickness], [grade], [scrap_factor] FROM Std_Lams", sqlCon);
                    SQLiteDataReader reader = sqlCom.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                Lamination lamination = new Lamination(true, reader["name"].ToString(), GetCoreShape(reader["shape"].ToString()), GetPhase(reader["phase"].ToString()), GetGrade(reader["grade"].ToString()),
                                double.Parse(reader["thickness"].ToString()), double.Parse(reader["tongue"].ToString()), DetermineYoke(GetCoreShape(reader["shape"].ToString()), GetPhase(reader["phase"].ToString()), double.Parse(reader["tongue"].ToString())),
                                double.Parse(reader["width"].ToString()), double.Parse(reader["height"].ToString()), double.Parse(reader["scrap_factor"].ToString()),
                                GetDestructionFactor(GetGrade(reader["grade"].ToString())), GetExcitationFactor(GetGrade(reader["grade"].ToString())),
                                double.Parse(reader["base_price"].ToString()), double.Parse(reader["weight"].ToString()));
                                __laminations.Add(lamination);
                            }
                            catch (Exception) { }
                        }
                    }
                }
            }

            /// <summary>
            /// Returns contained list of laminations.
            /// </summary>
            /// <returns></returns>
            public static List<Lamination> GetLaminations() { return __laminations; }
            /// <summary>
            /// Returns a list of laminations that match given parameters, with a tongue that is bounded by [MinTongue, MaxTongue].            
            /// </summary>
            /// <param name="coreShape">Shape of the lamination.</param>
            /// <param name="phase">Phase of the lamination.</param>
            /// <param name="grade">Grade of lamination.</param>
            /// <param name="thickness">Thickness of lamination.</param>
            /// <param name="minTongue">Minimum tongue size of lamination.</param>
            /// <param name="maxTongue">Maximum tongue size of lamination.</param>
            /// <returns>A list of all laminations that meet given parameters.</returns>
            public static List<Lamination> GetLaminations(CoreShape coreShape, Phase phase, Grade grade, double thickness, double minTongue, double maxTongue)
            {
                IEnumerable<Lamination> _laminations = __laminations.Where(l => l.Shape == coreShape && l.Phase == phase && l.Grade == grade && l.Thickness == thickness && l.Tongue >= minTongue && l.Tongue <= maxTongue);
                return _laminations.ToList();
            }
        }

        /// <summary>
        /// Static class that contains price information for various materials. 
        /// 
        /// Prices are stored as decimal but often converted to double.
        /// </summary>
        public static class Prices
        {
            /// <summary>
            /// Static constructor.
            ///
            /// Loads prices from an optimizer database.
            /// As of 03/15/2019 it only loads from a particular database.
            /// </summary>
            static Prices()
            {
                using (FileStream fileStream = new FileStream(XML_FILE_PATH, FileMode.Open, FileAccess.Read))
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(fileStream);
                    XmlNodeList xmlNodeList = xmlDocument.GetElementsByTagName("Data");
                    foreach (XmlNode node in xmlNodeList)
                    {
                        foreach (XmlElement element in node.ChildNodes)
                        {
                            if (element.Name == "Copper")
                                CopperPrice = decimal.Parse(element.GetAttribute("Price"));
                            else if (element.Name == "Aluminum")
                                AluminumPrice = decimal.Parse(element.GetAttribute("Price"));
                            else if (element.Name == "CTL_Lamination")
                            {
                                foreach (XmlNode child in element.ChildNodes)
                                {
                                    if (child.NodeType != XmlNodeType.Comment)
                                    {
                                        if (child.Name == "M6")
                                            M6DollarsPerPound = decimal.Parse(((XmlElement)child).GetAttribute("Price"));
                                        else if (child.Name == "M12")
                                            M12DollarsPerPound = decimal.Parse(((XmlElement)child).GetAttribute("Price"));
                                        else if (child.Name == "M085_ButtLap")
                                            M085_ButtLapDollarsPerPound = decimal.Parse(((XmlElement)child).GetAttribute("Price"));
                                        else if (child.Name == "M085_StepLap")
                                            M085_StepLapDollarsPerPound = decimal.Parse(((XmlElement)child).GetAttribute("Price"));
                                        else if (child.Name == "M19")
                                            M19DollarsPerPound = decimal.Parse(((XmlElement)child).GetAttribute("Price"));
                                        else if (child.Name == "M50")
                                            M50DollarsPerPound = decimal.Parse(((XmlElement)child).GetAttribute("Price"));
                                    }
                                }
                            }
                            else if (element.Name == "STD_Lamination")
                            {
                                foreach (XmlNode child in element.ChildNodes)
                                {
                                    if (child.NodeType != XmlNodeType.Comment)
                                    {
                                        if (child.Name == "M6")
                                            M6Surchage = decimal.Parse(((XmlElement)child).GetAttribute("Price"));
                                        else if (child.Name == "M19")
                                            M19Surchage = decimal.Parse(((XmlElement)child).GetAttribute("Price"));
                                        else if (child.Name == "M50")
                                            M50Surchage = decimal.Parse(((XmlElement)child).GetAttribute("Price"));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Price of M6 surcharge in $/Lb.
            /// </summary>
            public static decimal M6Surchage { get; }
            /// <summary>
            /// Price of M19 surcharge in $/Lb.
            /// </summary>
            public static decimal M19Surchage { get; }
            /// <summary>
            /// Price of M50 surcharge in $/Lb.
            /// </summary>
            public static decimal M50Surchage { get; }
            /// <summary>
            /// Price of M6 in $/Lb.
            /// </summary>
            public static decimal M6DollarsPerPound { get; }
            /// <summary>
            /// Price of M12 in $/Lb.
            /// </summary>
            public static decimal M12DollarsPerPound { get; }
            /// <summary>
            /// Price of M19 in $/Lb.
            /// </summary>
            public static decimal M19DollarsPerPound { get; }
            /// <summary>
            /// Price of M50 in $/Lb.
            /// </summary>
            public static decimal M50DollarsPerPound { get; }
            /// <summary>
            /// Price of M085 Butt Lap in $/Lb.
            /// </summary>
            public static decimal M085_ButtLapDollarsPerPound { get; }
            /// <summary>
            /// Price of M085 Step Lap in $/Lb.
            /// </summary>
            public static decimal M085_StepLapDollarsPerPound { get; }
            /// <summary>
            /// Price of copper wire in $/Lb.
            /// </summary>
            public static decimal CopperPrice { get; }
            /// <summary>
            /// Price of aluminum wire in $/Lb.
            /// </summary>
            public static decimal AluminumPrice { get; }

            /// <summary>
            /// Get the cost of a given grade of steel in $/Lb.
            /// 
            /// Returns $9999 if grade is not found, as of 03/15/2019 no checks are in place to remove designs that have such a price.
            /// </summary>
            /// <param name="grade">Grade of steel to get price of.</param>
            /// <returns>Price of the price for given steel in $/Lb.</returns>
            public static decimal GetMaterialDollarsPerPound(Grade grade)
            {
                switch (grade)
                {
                    case Grade.M6:
                        return M6DollarsPerPound;
                    case Grade.M12:
                        return M12DollarsPerPound;
                    case Grade.M19:
                        return M19DollarsPerPound;
                    case Grade.M50:
                        return M50DollarsPerPound;
                    case Grade.M085_ButtLap:
                        return M085_ButtLapDollarsPerPound;
                    case Grade.M085_StepLap:
                        return M085_StepLapDollarsPerPound;
                    default:
                        return 9999;
                }
            }

            /// <summary>
            /// Get the surcharge amount on a given grade of steel in $/Ton.
            /// 
            /// Returns $9999 if grade is not found, as of 03/15/2019 no checks are in place to remove designs that have such a price.
            /// </summary>
            /// <param name="grade">Grade of steel to get surcharge price of.</param>
            /// <returns>Price of the surcharge for given steel.</returns>
            public static decimal GetMaterialSurcharge(Grade grade)
            {
                switch (grade)
                {
                    case Grade.M6:
                        return M6Surchage;
                    case Grade.M19:
                        return M19Surchage;
                    case Grade.M50:
                        return M50Surchage;
                    default:
                        return 9999;
                }
            }

            /// <summary>
            /// Get the material price of a given wire material.
            /// </summary>
            /// <param name="wireMaterial">Wire material to get price of.</param>
            /// <returns>Price of wire material.</returns>
            public static decimal GetWireMaterialPrice(WireMaterial wireMaterial)
            {
                if (wireMaterial == WireMaterial.COPPER)
                    return CopperPrice;
                else
                    return AluminumPrice;
            }
        }

        /// <summary>
        /// Static class that contains core losses for various grade and thickness combinations at different flux densities and frequencies.        
        /// </summary>
        public static class CoreLosses
        {
            /// <summary>
            /// Holder of Losses objects.
            /// </summary>
            private static List<Losses> _Losses { get; }

            /// <summary>
            /// Static constructor.
            /// 
            /// Initializes list and fills it with Losses objects that each contain a value from an optimizer database.
            /// As of 03/15/2019 it only loads from a particular database.
            /// </summary>
            static CoreLosses()
            {
                _Losses = new List<Losses>();
                using (SQLiteConnection sqlCon = new SQLiteConnection(@DATABASE_CONNECTION_STRING, true))
                {
                    sqlCon.Open();
                    SQLiteCommand sqlCom = new SQLiteCommand("SELECT [flux_density], [thickness], [losses], [frequency], [grade] FROM core_losses", sqlCon);
                    SQLiteDataReader reader = sqlCom.ExecuteReader();
                    while (reader.Read())
                    {
                        _Losses.Add(new Losses(double.Parse(reader["flux_density"].ToString()), double.Parse(reader["thickness"].ToString()), double.Parse(reader["losses"].ToString()), int.Parse(reader["frequency"].ToString()), reader["grade"].ToString()));
                    }
                }
            }

            /// <summary>
            /// Determines what the core losses would be in W/Lb with given parameters.
            /// 
            /// If the exact values for flux density are not found, the function will use linear interpolation to find the core losses.
            /// </summary>
            /// <param name="FluxDensity">Flux Density of the core.</param>
            /// <param name="Thickness">Thickness of the lamination.</param>
            /// <param name="Frequency">Operating frequency of the core.</param>
            /// <param name="Grade">Grade of the lamination.</param>
            /// <returns>The core losses to expect with the given parameters in W/Lb.</returns>
            public static double GetCoreLosses(double FluxDensity, double Thickness, int Frequency, string Grade)
            {
                IEnumerable<Losses> losses = _Losses.Where(loss => loss.Frequency == Frequency && loss.Thickness == Thickness && loss.Grade == Grade).OrderBy(loss => loss.FluxDensity);
                if (losses.Count() > 0 && losses.Where(loss => loss.FluxDensity == FluxDensity).Count() > 0)
                    return losses.Where(loss => loss.FluxDensity == FluxDensity).First().CoreLosses;

                int maxIndex = 0, minIndex = 0;
                if (FluxDensity > (double)19 / 2)
                {
                    for (int i = losses.Count() - 1; i >= 0; i--)
                    {
                        if (losses.ElementAt(i).FluxDensity < FluxDensity)
                        {
                            minIndex = i;
                            maxIndex = i + 1;
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i <= losses.Count() - 1; i++)
                    {
                        if (losses.ElementAt(i).FluxDensity > FluxDensity)
                        {
                            maxIndex = i;
                            minIndex = i - 1;
                            break;
                        }
                    }
                }

                if (minIndex == maxIndex)
                    return 100;
                if (minIndex == -1 || maxIndex == losses.Count())
                    return 100;
                return LinearInterpolation(FluxDensity, losses.ElementAt(maxIndex).CoreLosses, losses.ElementAt(minIndex).CoreLosses, losses.ElementAt(maxIndex).FluxDensity, losses.ElementAt(minIndex).FluxDensity);
            }

            /// <summary>
            /// Container object for each record in database, as of 03/15/2019 only database loading is supported so therefore the records can only come from a database.
            /// </summary>
            private class Losses
            {
                /// <summary>
                /// Constructor.
                /// </summary>
                /// <param name="FluxDensity">Flux Density the losses occur at.</param>
                /// <param name="Thickness">Thickness of the grade of steel.</param>
                /// <param name="CoreLosses">Core Losses at other given parameters.</param>
                /// <param name="Frequency">Frequency the losses occur at.</param>
                /// <param name="Grade">Grade of steel.</param>
                internal Losses(double FluxDensity, double Thickness, double CoreLosses, int Frequency, string Grade)
                {
                    this.FluxDensity = FluxDensity;
                    this.Thickness = Thickness;
                    this.CoreLosses = CoreLosses;
                    this.Frequency = Frequency;
                    this.Grade = Grade;
                }

                /// <summary>
                /// Flux Density losses occur at.
                /// </summary>
                protected internal double FluxDensity { get; }
                /// <summary>
                /// Thickness of the grade of steel.
                /// </summary>
                protected internal double Thickness { get; }
                /// <summary>
                /// Core Losses at other given parameters.
                /// </summary>
                protected internal double CoreLosses { get; }
                /// <summary>
                /// Frequency the losses occur at.
                /// </summary>
                protected internal int Frequency { get; }
                /// <summary>
                /// Grade of steel.
                /// </summary>
                protected internal string Grade { get; }
            }
        }

        /// <summary>
        /// Static class that contains the laminations and their thicknesses.
        /// </summary>
        public static class Laminations
        {
            private static readonly Dictionary<Grade, Tuple<bool, bool, double[]>> _keyValuePairs;

            /// <summary>
            /// Static constructor.
            /// 
            /// Initializes dictionary loads it with lamination data from an optimizer database.
            /// As of 04/17/2019 it only loads from a particular database.
            /// </summary>
            static Laminations()
            {
                _keyValuePairs = new Dictionary<Grade, Tuple<bool, bool, double[]>>();
                using (SQLiteConnection sqlCon = new SQLiteConnection(@DATABASE_CONNECTION_STRING, true))
                {
                    sqlCon.Open();
                    SQLiteCommand sqlCom = new SQLiteCommand("SELECT [grade], [thicknesses], [standard], [cuttolength] FROM lamination_data", sqlCon);
                    SQLiteDataReader reader = sqlCom.ExecuteReader();
                    while (reader.Read())
                    {
                        _keyValuePairs.Add(GetGrade(reader["grade"].ToString()), new Tuple<bool, bool, double[]>(bool.Parse(reader["standard"].ToString()), bool.Parse(reader["cuttolength"].ToString()), Array.ConvertAll(reader["thicknesses"].ToString().Split(','), double.Parse)));
                    }
                }
            }

            /// <summary>
            /// Returns the grades that have data available.
            /// </summary>
            /// <returns>Array of available grades.</returns>
            public static Grade[] GetGrades()
            {
                return _keyValuePairs.Keys.ToArray();
            }
            /// <summary>
            /// Gets the tuple of data associated with the given grade.
            /// 
            /// Returned tuple has three items.
            /// Item1 is a boolean representing if the grade can be used as a standard lamination.
            /// Item2 is a boolean representing if the grade can be used as a cut-to-length lamination.
            /// Item3 is an array oif double representing the thickness the lamination comes in.
            /// </summary>
            /// <param name="grade">Grade to get data of.</param>
            /// <returns>Data of the grade.</returns>
            public static Tuple<bool, bool, double[]> GetDataForGrade(Grade grade)
            {
                return _keyValuePairs[grade];
            }
            /// <summary>
            /// Returns if the grade can be used as a standard lamination.
            /// </summary>
            /// <param name="grade">Grade to get data of.</param>
            /// <returns>Can the grade be used as a standard lamination.</returns>
            public static bool ValidStandard(Grade grade)
            {
                return _keyValuePairs[grade].Item1;
            }
            /// <summary>
            /// Returns if the grade can be used as a cut-to-length lamination.
            /// </summary>
            /// <param name="grade">Grade to get data of.</param>
            /// <returns>Can the grade be used as a cut-to-length lamination.</returns>
            public static bool ValidCutToLength(Grade grade)
            {
                return _keyValuePairs[grade].Item2;
            }
            /// <summary>
            /// Returns an array of double representing the thickness the lamination comes in.
            /// </summary>
            /// <param name="grade">Grade to get data of.</param>
            /// <returns>An array of double representing the thickness the lamination comes in.</returns>
            public static double[] Thicknesses(Grade grade)
            {
                return _keyValuePairs[grade].Item3;
            }
        }
    }
}
