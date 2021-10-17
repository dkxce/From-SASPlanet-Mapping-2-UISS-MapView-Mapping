//******************************************************//
//*                                                    *//
//* From SAS.Planet Mapping to UISS UI-MapView Mapping *//
//*                 milokz@gmail.com                   *//
//*                                                    *//
//******************************************************//


using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace FromSASPlanetMapping2UISSMapViewMapping
{
    class Program
    {
        static System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InvariantCulture;

        static void Main(string[] args)
        {
            // HEADER
            Console.WriteLine("/************************************************************/");
            Console.WriteLine("/*                                                          */");
            Console.WriteLine("/*    From SAS.Planet Mapping to UISS UI-MapView Mapping    */");
            Console.WriteLine("/*                    milokz@gmail.com                      */");
            Console.WriteLine("/*                                                          */");
            Console.WriteLine("/*  UISS:                                                   */");
            Console.WriteLine("/*      Convert .dat/.tab/.kml to .ini in current directory */");
            Console.WriteLine("/*                                                          */");
            Console.WriteLine("/*  UI-View:                                                */");
            Console.WriteLine("/*      Convert .dat/.tab/.kml to .inf in current directory */");
            Console.WriteLine("/*                                                          */");
            Console.WriteLine("/*  Это приложение конвертирует файлы привязки карт  .dat,  */");
            Console.WriteLine("/*  .tab и .kml сформированные в SAS.Planet в .ini и .inf   */");
            Console.WriteLine("/*                                     для UISS и UI-View.  */");
            Console.WriteLine("/*                                                          */");
            Console.WriteLine("/*  Для конвертации просто запустите программу в папке с    */");
            Console.WriteLine("/*                                   требуемыми файлами.    */");
            Console.WriteLine("/*                                                          */");
            Console.WriteLine("/************************************************************/");
            Console.WriteLine("");

            // CF
            string cd = System.IO.Directory.GetCurrentDirectory();
            Console.WriteLine(cd);

            // List Files
            string[] exts = new string[] { "*.tab", "*.dat", "*.kml" };
            int fCount = 0;
            foreach (string ext in exts)
            {
                string[] files = System.IO.Directory.GetFiles(cd, ext);
                if (files == null) continue;
                if (files.Length == 0) continue;
                foreach (string file in files)
                {
                    double[] gett = null;
                    if (System.IO.Path.GetExtension(file).ToLower() == ".tab") gett = ReadTabFile(file);
                    if (System.IO.Path.GetExtension(file).ToLower() == ".dat") gett = ReadDatFile(file);
                    if (System.IO.Path.GetExtension(file).ToLower() == ".kml") gett = ReadKmlFile(file);
                    if (gett != null)
                    {
                        fCount++;
                        string f2 = file.Substring(0, file.LastIndexOf(".")) + ".ini";
                        if(WriteIniFile(f2, gett))
                            Console.WriteLine("  " + Path.GetFileName(file) + " -> " + Path.GetFileName(f2) + " - Ok (UISS)");
                        else
                            Console.WriteLine("  " + Path.GetFileName(file) + " -> " + Path.GetFileName(f2) + " - Error (UISS)");
                        string f3 = file.Substring(0, file.LastIndexOf(".")) + ".inf";
                        if (WriteInfFile(f3, gett))
                            Console.WriteLine("  " + Path.GetFileName(file) + " -> " + Path.GetFileName(f3) + " - Ok (UI-View)");
                        else
                            Console.WriteLine("  " + Path.GetFileName(file) + " -> " + Path.GetFileName(f3) + " - Error (UI-View)");
                    }
                    else
                        Console.WriteLine("  " + Path.GetFileName(file) + " .. Unknown Format");
                };
            };
            if (fCount == 0) Console.WriteLine("  NO FILES");
            Console.WriteLine("Done");
            System.Threading.Thread.Sleep(3500);            
        }

        static double[] ReadTabFile(string fileName)
        {
            if (String.IsNullOrEmpty(fileName)) return null;
            if (!File.Exists(fileName)) return null;
            
            FileStream fs = null;
            try
            {
                double[] res = new double[] { 0.0, 0.0, 0.0, 0.0, 1.0 };
                int xmax = 0;
                int ymax = 0;
                Regex rx = new Regex(@"^\s*\((?<lon>[\-\d\.]*),\s?(?<lat>[\-\d\.]*)\)\s?\((?<x>\d*),\s?(?<y>\d*)\)\s?Label");
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine().Trim();
                    if (String.IsNullOrEmpty(line)) continue;
                    Match mc = rx.Match(line);
                    if (!mc.Success) continue;
                    int x = int.Parse(mc.Groups["x"].Value);
                    int y = int.Parse(mc.Groups["y"].Value);
                    if (x == 0) res[0] = double.Parse(mc.Groups["lon"].Value, ci);
                    if (y == 0) res[1] = double.Parse(mc.Groups["lat"].Value, ci);
                    if ((x > 0) && (x > xmax)) { xmax = x; res[2] = double.Parse(mc.Groups["lon"].Value, ci); };
                    if ((y > 0) && (y > ymax)) { ymax = y; res[3] = double.Parse(mc.Groups["lat"].Value, ci); };
                };
                sr.Close();
                fs.Close();
                return res;
            }
            catch { }
            finally { if (fs != null) fs.Close(); };
            return null;
        }

        static double[] ReadDatFile(string fileName)
        {
            if (String.IsNullOrEmpty(fileName)) return null;
            if (!File.Exists(fileName)) return null;
            
            FileStream fs = null;
            try
            {
                double[] res = new double[] { 0.0, 0.0, 0.0, 0.0, 2.0 };
                fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                sr.ReadLine();
                string[] lt = sr.ReadLine().Trim().Split(new char[] { ',' }, 2);  // left, top
                sr.ReadLine();
                string[] rb = sr.ReadLine().Trim().Split(new char[] { ',' }, 2);  // right, bottom
                res[0] = double.Parse(lt[0], ci);
                res[1] = double.Parse(lt[1], ci);
                res[2] = double.Parse(rb[0], ci);
                res[3] = double.Parse(rb[1], ci);
                sr.Close();
                fs.Close();
                return res;
            }
            catch { }
            finally { if (fs != null) fs.Close(); };
            return null;
        }

        static double[] ReadKmlFile(string fileName)
        {
            if (String.IsNullOrEmpty(fileName)) return null;
            if (!File.Exists(fileName)) return null;

            double[] res = new double[] { 0.0, 0.0, 0.0, 0.0, 3.0 };
            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(fileName);
                res[0] = double.Parse(xd.SelectSingleNode(@"kml/GroundOverlay/LatLonBox/west").ChildNodes[0].Value, ci);
                res[1] = double.Parse(xd.SelectSingleNode(@"kml/GroundOverlay/LatLonBox/north").ChildNodes[0].Value, ci);
                res[2] = double.Parse(xd.SelectSingleNode(@"kml/GroundOverlay/LatLonBox/east").ChildNodes[0].Value, ci);
                res[3] = double.Parse(xd.SelectSingleNode(@"kml/GroundOverlay/LatLonBox/south").ChildNodes[0].Value, ci);
                return res;
            }
            catch { };
            return null;
        }

        static bool WriteIniFile(string fileName, double[] coord)
        {
            if (String.IsNullOrEmpty(fileName)) return false;
            if (coord == null) return false;
            if(coord.Length == 0) return false;

            FileStream fs = null;
            try
            {
                fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine("[UI-MapView compatible Scale External Map]");
                sw.WriteLine("[South_Left=360-lon]");
                sw.WriteLine(String.Format(ci, " {0}", 360 + coord[0]));
                sw.WriteLine("[East_Top=180-lat]");
                sw.WriteLine(String.Format(ci, " {0}", 180 - coord[1]));
                sw.WriteLine("[West_Heigth=lat1-lat2]");
                sw.WriteLine(String.Format(ci, " {0}", Math.Abs(coord[1] - coord[3])));
                sw.WriteLine("[North_Width=long1-long2]");
                sw.WriteLine(String.Format(ci, " {0}", Math.Abs(coord[2] - coord[0])));                
                sw.WriteLine("[Name]");
                sw.WriteLine(System.IO.Path.GetFileNameWithoutExtension(fileName));
                // COMMENT
                sw.WriteLine("[Source]");
                sw.WriteLine(Path.GetDirectoryName(fileName));
                if (coord[4] == 1) sw.WriteLine(Path.GetFileNameWithoutExtension(fileName) + ".tab " + DateTime.UtcNow.ToString("ddMMyyyyHHmmssUTC"));
                if (coord[4] == 2) sw.WriteLine(Path.GetFileNameWithoutExtension(fileName) + ".dat " + DateTime.UtcNow.ToString("ddMMyyyyHHmmssUTC"));
                if (coord[4] == 3) sw.WriteLine(Path.GetFileNameWithoutExtension(fileName) + ".kml " + DateTime.UtcNow.ToString("ddMMyyyyHHmmssUTC"));
                sw.WriteLine("[SKIP]");
                sw.WriteLine(";UB3APB");
                sw.WriteLine("Lon E = 360 + LON WEST/LEFT // ex: 37.55 == 397.55");
                sw.WriteLine("Lat N = 180 - LAT NORTH/TOP // ex: 55.55 == 124.55");
                sw.WriteLine("Use SAS.Planet to Export & kml to View Bounds");
                sw.Close();
                fs.Close();
                return true;
            }
            catch { }
            finally { if (fs != null) fs.Close(); };
            return false;
        }

        static bool WriteInfFile(string fileName, double[] coord)
        {
            if (String.IsNullOrEmpty(fileName)) return false;
            if (coord == null) return false;
            if (coord.Length == 0) return false;

            FileStream fs = null;
            try
            {
                fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                double deg = 0;                
                sw.Write(String.Format(ci, "{0:.}.{1:00.}.{2:00.}{3},", new object[] { Math.Truncate(deg = Math.Abs(coord[1])), Math.Truncate(deg = (deg - Math.Truncate(deg)) * 60.0), Math.Truncate(deg = (deg - Math.Truncate(deg)) * 60.0), coord[1] >= 0 ? "N" : "S" }));
                sw.Write(String.Format(ci, "{0:.}.{1:00.}.{2:00.}{3}", new object[] { Math.Truncate(deg = Math.Abs(coord[0])), Math.Truncate(deg = (deg - Math.Truncate(deg)) * 60.0), Math.Truncate(deg = (deg - Math.Truncate(deg)) * 60.0), coord[0] >= 0 ? "E" : "W" }));
                sw.WriteLine();
                sw.Write(String.Format(ci, "{0:.}.{1:00.}.{2:00.}{3},", new object[] { Math.Truncate(deg = Math.Abs(coord[3])), Math.Truncate(deg = (deg - Math.Truncate(deg)) * 60.0), Math.Truncate(deg = (deg - Math.Truncate(deg)) * 60.0), coord[3] >= 0 ? "N" : "S" }));
                sw.Write(String.Format(ci, "{0:.}.{1:00.}.{2:00.}{3}", new object[] { Math.Truncate(deg = Math.Abs(coord[2])), Math.Truncate(deg = (deg - Math.Truncate(deg)) * 60.0), Math.Truncate(deg = (deg - Math.Truncate(deg)) * 60.0), coord[2] >= 0 ? "E" : "W" }));
                sw.WriteLine();
                sw.WriteLine(System.IO.Path.GetFileNameWithoutExtension(fileName));
                // COMMENT
                sw.WriteLine("----------------------------------------------------------");
                sw.WriteLine(Path.GetDirectoryName(fileName));
                if (coord[4] == 1) sw.WriteLine(Path.GetFileNameWithoutExtension(fileName) + ".tab " + DateTime.UtcNow.ToString("ddMMyyyyHHmmssUTC"));
                if (coord[4] == 2) sw.WriteLine(Path.GetFileNameWithoutExtension(fileName) + ".dat " + DateTime.UtcNow.ToString("ddMMyyyyHHmmssUTC"));
                if (coord[4] == 3) sw.WriteLine(Path.GetFileNameWithoutExtension(fileName) + ".kml " + DateTime.UtcNow.ToString("ddMMyyyyHHmmssUTC"));
                sw.WriteLine(";UB3APB");
                sw.WriteLine("Lon E = 360 + LON WEST/LEFT // ex: 37.55 == 397.55");
                sw.WriteLine("Lat N = 180 - LAT NORTH/TOP // ex: 55.55 == 124.55");
                sw.WriteLine("Use SAS.Planet to Export & kml to View Bounds");
                sw.WriteLine("----------------------------------------------------------");
                sw.Close();
                fs.Close();
                return true;
            }
            catch { }
            finally { if (fs != null) fs.Close(); };
            return false;
        }
    }
}
