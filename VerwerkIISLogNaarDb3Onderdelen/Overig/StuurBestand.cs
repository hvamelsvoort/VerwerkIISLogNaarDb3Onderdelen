using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace VerwerkIISLogNaarDb3Onderdelen {
  class StuurBestand {
    private readonly char scheidingsTeken = '|';

    public List<String> skipIp = new List<String>();
    public List<String> teVerwerken = new List<String>();
    public List<String> iisLogs = new List<String>();
    public List<String> tabellen = new List<String>();
    public List<String> servers = new List<String>();

    public String LogBestand = @"D:\huub_van_amelsvoort\data\";
    public String InputFolder = @"D:\huub_van_amelsvoort\data\iis_advancedlog\";
    public string LogKop = "#Fields:  date time cs-uri-query s-contentpath sc-status s-computername cs(Referer) sc-bytes cs-bytes c-ip cs(Host) cs-method cs(User-Agent) s-ip X-Forwarded-For sc-substatus s-port cs-version c-protocol cs(Cookie) TimeTakenMS cs-uri-stem";

    public StuurBestand() {
      try {
        string[] text = File.ReadAllLines(@"StuurBestand.txt");
        foreach (string regel in text) {
          if (!regel.StartsWith("#")) {

            if (regel.Contains(String.Format("server{0}", scheidingsTeken))) verwerkServer(regel);
            if (regel.Contains(String.Format("skipip{0}", scheidingsTeken))) verwerkSkipIp(regel);
            if (regel.Contains(String.Format("verwerk{0}", scheidingsTeken))) verwerken(regel);
            if (regel.Contains(String.Format("iislog{0}", scheidingsTeken))) verwerkIislog(regel);
            if (regel.Contains(String.Format("tabel{0}", scheidingsTeken))) verwerkTabel(regel);

            if (regel.Contains(String.Format("logbestand{0}", scheidingsTeken))) LogBestand = regel.Split(scheidingsTeken)[1];
            if (regel.Contains(String.Format("inputFolder{0}", scheidingsTeken))) InputFolder = regel.Split(scheidingsTeken)[1];
            if (regel.Contains(String.Format("logKop{0}", scheidingsTeken))) LogKop = regel.Split(scheidingsTeken)[1];
          }
        }
      } catch (Exception e) {
        Console.WriteLine("Geen StuurBestand aanwezig !!?? " + e.Message);
        Debug.WriteLine("Geen StuurBestand aanwezig !!?? " + e.Message);
      }
    }

    private void verwerkServer(string regel) {
      servers.Add(regel.Split(scheidingsTeken)[1]);
    }

    private void verwerkTabel(string regel) {
      tabellen.Add(regel.Split(scheidingsTeken)[1]);
    }

    private void verwerken(string regel) {
      teVerwerken.Add(regel.Split(scheidingsTeken)[1]);
    }

    private void verwerkIislog(string regel) {
      iisLogs.Add(regel.Split(scheidingsTeken)[1]);
    }

    private void verwerkSkipIp(string regel) {
      skipIp.Add(regel.Split(scheidingsTeken)[1]);
    }
  }
}
