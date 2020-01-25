using CredentialManagement;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace VerwerkIISLogNaarDb3Onderdelen {
  internal class DeFuncties {

    private static List<IISLogBestandObject> iisLogBestanden = new List<IISLogBestandObject>();
    public static StuurBestand stuurBestand = new StuurBestand();

    public static string LogBestandNaam { get; set; }

    public static StreamWriter logBestand;
    public static DateTime behandelDatum = DateTime.Now;

    private static SQLafhandeling mySQLafhandeling;

    // Volledige IISLog regel object
    public HashSet<IISLogObject> lijstIISLogObjecten = new HashSet<IISLogObject>();
    internal static bool automatisch;

    public DeFuncties() {
      mySQLafhandeling = new SQLafhandeling(lijstIISLogObjecten);
    }

    /**
     * Standaard gaat de log naar console en naar een bestand
     * Bij deze functie alleen naar Console bij naarLogschrijven = False
     */
    public static void HuubLog(string tekst, bool naarLogschrijven) {
      if (naarLogschrijven) {
        HuubLog(tekst);
      } else {
        Debug.WriteLine(tekst);
        Console.WriteLine(tekst);
      }
    }
    /**
     * Standaard gaat de log naar console en naar een bestand
     */
    public static void HuubLog(string tekst) {
      logBestand.WriteLine(tekst);
      logBestand.Flush();
    }

    /**
     * Regel bevat de goede lay-out (Na 1 april 2018 als het goed is).
     * Nu nog wel bepalen of deze naar de database geschreven moet worden
     * */
    internal void verwerkGoedeRegel(string regel) {
      IISLogObject iislog = new IISLogObject();

      // Soms wordt binnen een tekst 2 quotes gebruikt om de quote te escapen
      if (regel.Contains("\"\"") & !regel.Contains(" \"\" ")) {
        regel = regel.Replace("\"\"", "");
      }

      List<string> gesplitst = splitsDeRegel(regel);
      try {
        iislog = maakEenIISLogObject(gesplitst);
      } catch (Exception e) {
        HuubLog("Fout in maakEenIISLogObject : " + e.Message);
      }

      // Toevoegen van de objecten in een hashset TODO vaak dubbel maar waarom
      if (! lijstIISLogObjecten.Add(iislog)) {
        HuubLog("Dubbele rij in de iislog : " + regel, true);
        //var x = lijstIISLogObjecten.GetHashCode();
        //var y = lijstIISLogObjecten.GetObjectData(iislog);
      }
    }
    /**
     * De regel opslitsen in elementen, omgeven door quotes wordt als een geheel gezien.
     */
    private List<string> splitsDeRegel(string regel) {
      return Regex.Matches(regel, @"[\""].+?[\""]|[^ ]+")
                      .Cast<Match>()
                      .Select(m => m.Value)
                      .ToList();
    }
    /**
     * Ik altijd denken dat zonder meer de *.exe.config aangepast kan worden, maar dat is niet zo
     */
    internal void wijzigConfig() {
      // https://blogs.msdn.microsoft.com/youssefm/2010/01/21/how-to-change-net-configuration-files-at-runtime-including-for-wcf/

      Configuration config = ConfigurationManager.OpenExeConfiguration(Assembly.GetEntryAssembly().Location);
      AppSettingsSection appSettings = (AppSettingsSection)config.GetSection("appSettings");
      appSettings.Settings.Clear();
      config.Save();

      ConfigurationManager.RefreshSection("appSettings");
    }

    /// <summary>
    /// Op basis van een gesplitse tekstregel omzetten naar object iislog
    /// voorlopig alleen op oude en nieuwe omgeving aangepast. Nog niet op de NAM logs
    /// </summary>
    /// <param name="gesplitst"></param>
    /// <returns></returns>
    private IISLogObject maakEenIISLogObject(List<string> gesplitst) {
      IISLogObject iislog = new IISLogObject();

      string tijd = gesplitst[LogVeldIndex.tijd];
      string tijdDeelVoorPunt = tijd.Substring(0, tijd.IndexOf('.'));
            //TODO Davey: kijken of hier losse objecten of samen moeten komen als er een regel op hetzelfde tijdstip voorkomt
            string tijdDeelNaPunt = tijd.Substring(tijd.IndexOf('.') + 1);  

      try {
        iislog.datum = maakDatum(gesplitst[LogVeldIndex.datum]);
        iislog.uur = tijdDeelVoorPunt.Substring(0, 2);
        iislog.fractie_tijd = tijdDeelNaPunt;
        iislog.time_taken = gesplitst[LogVeldIndex.time_taken];
        iislog.cs_uri_query = gesplitst[LogVeldIndex.cs_uri_query].Replace("\"", "");
        iislog.s_computername = gesplitst[LogVeldIndex.s_computername].Replace("\"", "");
        iislog.cs_referer = gesplitst[LogVeldIndex.cs_referer].Replace("\"", "");
        iislog.s_contentpath = gesplitst[LogVeldIndex.s_contentpath].Replace("\"", "");
        iislog.sc_status = gesplitst[LogVeldIndex.sc_status];
        iislog.sc_bytes = gesplitst[LogVeldIndex.sc_bytes];
        iislog.cs_bytes = gesplitst[LogVeldIndex.cs_bytes];
        iislog.cs_host = gesplitst[LogVeldIndex.cs_host].Replace("\"", "");
        iislog.cs_method = gesplitst[LogVeldIndex.cs_method];
        iislog.s_ip = gesplitst[LogVeldIndex.s_ip];
        iislog.x_forwarded_for = gesplitst[LogVeldIndex.x_forwarded_for].Replace("\"", "");
        iislog.cs_cookie = gesplitst[LogVeldIndex.cs_cookie].Replace("\"", "");
        iislog.c_ip = gesplitst[LogVeldIndex.c_ip];
      } catch (Exception e) {
        HuubLog("Fout in maakEenIISLogObject : " + e.Message);
      }
      return iislog;
    }
    /**
    * Opvragen wachtwoord uit de Credential Manager van Windows
    */
    public static String ophalenCredentials(String gebruiker) {
      try {
        using (var cred = new Credential()) {
          cred.Target = gebruiker;
          cred.Load();
         Console.WriteLine("Credentials ophalen");
         return cred.Password;
        }
      } catch (Exception e) {
        HuubLog("Fout in ophalenCredentials : " + e.Message);
      }
      return null ;
    }

    /**
     * Hier de objecten naar de database schrijven
     */
    internal static void verwerkDeLogsObjecten() {
      mySQLafhandeling.wachtwoord = ophalenCredentials("postgres");
      if (mySQLafhandeling.wachtwoord == null || mySQLafhandeling.wachtwoord.Length < 2) {
        Console.WriteLine("Wachtwoord niet gevonden !! Programma stopt.");
        System.Environment.Exit(-3295);
      }


      mySQLafhandeling.verwerkDeLogsObjecten();
    }
    /**
    * Indien er geen argumenten meegegeven worden dan automatisch alle log bestanden van de OTAP verplaatsen.
    * 
    */
    internal static void verplaatsDeLogs() {
      string pad;

      foreach(string sb in stuurBestand.iisLogs) {
        pad = String.Format("{0}", sb); 
        verplaatsenLogs(pad);
      }
    }
    /**
     * Verplaatst de logs van de oorspronkelijke omgeving naar een centrale plek (bestemming)
     */
    private static void verplaatsenLogs(string pad) {
      string bestemming = @"D:\huub_van_amelsvoort\data\iis_advancedlog";
      string[] logBestandsNamen;

      try {
        logBestandsNamen = Directory.GetFiles(pad, "*.log", SearchOption.AllDirectories);
        foreach (string bestandNaam in logBestandsNamen) {
          if (File.Exists(bestandNaam)) {
            IISLogBestandObject iisLogBestand = new IISLogBestandObject();
            iisLogBestand.CompleteNaam = bestandNaam;
            iisLogBestand.Naam = Path.GetFileName(bestandNaam);
            iisLogBestand.BestandsLengte = new FileInfo(bestandNaam).Length;
            iisLogBestanden.Add(iisLogBestand);
          }
        }
      } catch (Exception e) {
        DeFuncties.HuubLog("Fout in bepalenBestanden : " + e, true);
        DeFuncties.HuubLog("Fout in bepalenBestanden : " + e, false);
      }

      foreach (IISLogBestandObject bestand in iisLogBestanden) {
        try {
          string deBestemming = bestemming + @"\" + bestand.Naam;
          if (! File.Exists(deBestemming)) {
            DeFuncties.HuubLog(String.Format("Verplaats : {0} naar : {1}", bestand.CompleteNaam, deBestemming), true);
            DeFuncties.HuubLog(String.Format("Verplaats : {0} naar : {1}", bestand.CompleteNaam, deBestemming), false);
            File.Move(bestand.CompleteNaam, deBestemming);
          }
        } catch (Exception e) {
          DeFuncties.HuubLog(String.Format("Verplaats fout : {0} ; naam : {1}", e.Message, bestand), true);
          DeFuncties.HuubLog(String.Format("Verplaats fout : {0} ; naam : {1}", e.Message, bestand), false);
        }
      }
    }

    /**
     * Maak van een string 2019-11-01 een DateTime
     */
    public DateTime maakDatum(String datum) {
      int dag = Int32.Parse(datum.Substring(8, 2));
      int maand = Int32.Parse(datum.Substring(5, 2));
      int jaar = Int32.Parse(datum.Substring(0, 4));
      //int eeuw = Int32.Parse(iislog.datum.Substring(1, 2));

      DateTime datumVoorInsert = new DateTime(jaar, maand, dag);

      return datumVoorInsert;
    }

  }
}