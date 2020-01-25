using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace VerwerkIISLogNaarDb3Onderdelen {
  internal class Verwerk {

    private string[] logBestandsNamen;

    private DeFuncties deFuncties;
    private List<IISLogBestandObject> iisLogBestanden = new List<IISLogBestandObject>();
    private bool swGoedeRegel;
    /**
     * Instantie met DeFuncties
     */
    public Verwerk(DeFuncties deFuncties) {
      this.deFuncties = deFuncties;
    }

    /**
     * Controleer of de datum wordt meegegeven
     */
    internal bool CheckArgs(string[] args) {
      DeFuncties.automatisch = false;
      string tekstdatum = "";

      if (args.Length > 1) return false;

      if (args.Length == 0) {
        // Zonder argumenten worden alle logs verplaatst
        // en wordt 2 dagen terug verwerkt
        DeFuncties.automatisch = true;
        DeFuncties.behandelDatum = DateTime.Now;
        // Altijd de logs van 2 dagen terug, omdat die pas beschikbaar zijn
        DeFuncties.behandelDatum = DeFuncties.behandelDatum.AddDays(-2);

      } else {
        tekstdatum = args[0];
        try {
          int jaar = Int32.Parse(tekstdatum.Substring(0, 4));
          int maand = Int32.Parse(tekstdatum.Substring(4, 2));
          int dag = Int32.Parse(tekstdatum.Substring(6, 2));

          DeFuncties.behandelDatum = new DateTime(jaar, maand, dag, 0, 0, 0);
        } catch (Exception e) {
          Debug.WriteLine("Datumfout : " + e.Message);
          Console.WriteLine("Datumfout : " + e.Message);
          return false;
        }
      }
      DeFuncties.LogBestandNaam = String.Format("{0}\\VerwerkIISLogNaarDb3Onderdelen_{1}{2}.log", DeFuncties.stuurBestand.LogBestand 
        ,  DeFuncties.behandelDatum.ToString("yyyyMMdd"), DeFuncties.automatisch);

      DeFuncties.HuubLog("De log wordt geschreven naar : " + DeFuncties.LogBestandNaam, false);
      try {
        DeFuncties.logBestand = new StreamWriter(DeFuncties.LogBestandNaam, false);
      }
      catch(Exception e) {
        Debug.WriteLine("Openen logbestand fout : " + e.Message);
        Console.WriteLine("Openen logbestand fout : " + e.Message);
        System.Environment.Exit(-329561);
      }

      if (DeFuncties.automatisch) DeFuncties.verplaatsDeLogs();

      return true;
    }

    /**
     * Indien geen of verkeerde argumenten zijn meegegeven.
     */
    internal void GeefGebruik() {
      DeFuncties.HuubLog("Gebruik programma : <verwerkdatum>", false);
      DeFuncties.HuubLog("of zonder argument automatische verwerking laatste dag !!!!", false);
    }
    /**
     * Hier start de applicatie vanuit Program.cs
     */
    internal void doen() {

      deFuncties.wijzigConfig();

      String startDatum = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt");

      string pad = DeFuncties.stuurBestand.InputFolder;
      try {
        logBestandsNamen = Directory.GetFiles(pad, "*.log", System.IO.SearchOption.AllDirectories);
        int aantal = Directory.GetFiles(pad, "*.log", System.IO.SearchOption.AllDirectories).Length;
        foreach (string bestandNaam in logBestandsNamen) {
          string datum = DeFuncties.behandelDatum.ToString("yyyyMMdd");
          bool bestaatBestand = File.Exists(bestandNaam); // zou eigenlijk altijd zo moeten zijn
          if (bestaatBestand && bestandNaam.Contains(".log") && bestandNaam.Contains(datum)) {
            IISLogBestandObject iisLogBestand = new IISLogBestandObject();
            iisLogBestand.CompleteNaam = bestandNaam;
            iisLogBestand.BestandsLengte = new FileInfo(bestandNaam).Length;
            iisLogBestanden.Add(iisLogBestand);
            DeFuncties.HuubLog(String.Format("zoekBestanden lengte : {0,10:######} ; naam : {1}", iisLogBestand.BestandsLengte, iisLogBestand.CompleteNaam.Replace(pad, "")));
          }
        }
      } catch (Exception e) {
        DeFuncties.HuubLog("Fout in zoekBestanden : " + e);
      }

      swGoedeRegel = false;
      string regel;

      foreach (IISLogBestandObject iisLogBestand in iisLogBestanden) {
        try {
          StreamReader bestand = new StreamReader(iisLogBestand.CompleteNaam);
          DeFuncties.HuubLog("Verwerk bestand : " + iisLogBestand.CompleteNaam, false);
          DeFuncties.HuubLog("Verwerk bestand : " + iisLogBestand.CompleteNaam);

          while ((regel = bestand.ReadLine()) != null) {

            if (regel.Contains("#Fields")) {
              int vergelijk = DeFuncties.stuurBestand.LogKop.CompareTo(regel);
              if (vergelijk == 0) {
                swGoedeRegel = true;
              } else {
                swGoedeRegel = false;
                DeFuncties.HuubLog("De kopregel voldoet niet aan de standaard !! " + regel, false);
                DeFuncties.HuubLog("De kopregel voldoet niet aan de standaard !! " + regel, true);
              }

            } else {
              if (swGoedeRegel && voorSelectieRegel(regel)) {
                deFuncties.verwerkGoedeRegel(regel);
              }
            }
          }
          bestand.Close();
        } catch (Exception e) {
          DeFuncties.HuubLog("Fout tijdens openenen bestand : " + e.Message, false);
          DeFuncties.HuubLog("Fout tijdens openenen bestand : " + e.Message);
        }
      }

      DeFuncties.HuubLog("Start verwerking MySQL: " + DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt"), true);

      DeFuncties.verwerkDeLogsObjecten();

      DeFuncties.HuubLog("Einde verwerking MySQL: " + DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt"), true);

      DeFuncties.HuubLog("Start verwerking: " + startDatum, false);
      DeFuncties.HuubLog("Einde verwerking: " + DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt"), false);

      DeFuncties.HuubLog("Start verwerking: " + startDatum, true);
      DeFuncties.HuubLog("Einde verwerking: " + DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt"), true);
    }
    /**
     * Deze functie nog voor het uitsplitsen van de regel
     * met name o.a. de Zabbix checks buiten beschouwing houden (ip adres )
     * maar ook bedrijven die onderzoek doen naar onze geoservices.denhaag.nl voor beschikbaarheid.
     */
    private bool voorSelectieRegel(string regel) {

      if (regel.StartsWith("#")) return false; // is een van de kopregels, die is al verwerkt

      foreach (string ipAdres in DeFuncties.stuurBestand.skipIp) {
        String controleIp = String.Format(" \"{0}\" ", ipAdres);
        if (regel.Contains(controleIp)) return false; // indien ip gevonden dan de regel skippen
      }

      foreach(string server in DeFuncties.stuurBestand.servers) {
        if (regel.ToUpper().Contains(server)) {
          return true;
        }
      }

      return false;
    }
  }
}
