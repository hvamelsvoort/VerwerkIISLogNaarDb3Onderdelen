using Npgsql;
using System;
using System.Collections.Generic;
using VerwerkIISLogNaarDb3Onderdelen.Properties;

namespace VerwerkIISLogNaarDb3Onderdelen {
  internal class SQLafhandeling {

    private HashSet<IISLogObject> lijstIISLogObjecten;

    public String wachtwoord = ""; // wordt gezet in aanroepende Verwerk

    private NpgsqlConnection verbindingSchrijven;
    private NpgsqlConnection verbindingLezen;
    private NpgsqlCommand commando;
    private NpgsqlDataReader reader;

    public HashSet<IISLogObject> LijstIISLogObjecten { get => lijstIISLogObjecten; set => lijstIISLogObjecten = value; }

    /**
     * Constructor voor het ontvangen van de iislog objecten
     */
    public SQLafhandeling(HashSet<IISLogObject> lijstIISLogObjecten) {
      this.LijstIISLogObjecten = lijstIISLogObjecten;
    }

    private void deleteTabellenDeDag() {
      legenTabel("powerbi_cumul_aantal_viewer");
      legenTabel("powerbi_cumul_gebruiker");
      legenTabel("powerbi_cumul_herkomst");
      legenTabel("powerbi_cumul_runs");
      //  legenTabel("powerbi_cumul_service_tijdsduur"); verhuisd

    }
    /**
     * Vooraf een tabel legen
     */
    private void legenTabel(string deTabel) {
      openenVerbindingSchrijven();

      commando = new NpgsqlCommand("delete from " + deTabel + " where datum=@datum", verbindingSchrijven);

      //      commando.Parameters.AddWithValue("@datum", DeFuncties.behandelDatum.ToString("yyyy-MM-dd")); MySQL
      commando.Parameters.AddWithValue("@datum", DeFuncties.behandelDatum); // Postgres


      try {
        int aantalVerwijderd = commando.ExecuteNonQuery();
        DeFuncties.HuubLog(String.Format("Geleegd in  : {0} aantal {1} ", deTabel, aantalVerwijderd), true);
        DeFuncties.HuubLog(String.Format("Geleegd in  : {0} aantal {1} ", deTabel, aantalVerwijderd), false);
      } catch (Exception e) {
        DeFuncties.HuubLog("Fout in het legenTabel : " + e.Message, true);
        DeFuncties.HuubLog("Fout in het legenTabel : " + e.Message, false);
      }

      sluitenSchrijven();
    }

    /**
    * Verwerk hier de opgeslagen iisobjecten
    */
    internal void verwerkDeLogsObjecten() {
      if (LijstIISLogObjecten.Count > 0) {
        deleteTabellenDeDag();
        doeInsertUiteindelijkEind();
      } else {
        DeFuncties.HuubLog("Geen objecten gevonden !!", true);
        DeFuncties.HuubLog("Geen objecten gevonden !!", false);
      }
    }
    /**
     * Subobjecten aanmaken voor Gebruikers, Herkomst.
     * Dit om alvast dubbele rijen te vookomen op log niveau
     * Ook om minder Database IO te krijgen
     */
    private void voorverwerken() {
      HashSet<GebruikerObject> lijstGebruikerObject = new HashSet<GebruikerObject>();
      HashSet<HerkomstObject> lijstHerkomstObject = new HashSet<HerkomstObject>();

      foreach (IISLogObject iislog in LijstIISLogObjecten) {
        GebruikerObject gebruikersObject = new GebruikerObject();
        HerkomstObject herkomstObject = new HerkomstObject();

        gebruikersObject.s_computername = iislog.s_computername;
        gebruikersObject.datum = iislog.datum;
        gebruikersObject.uur = iislog.uur;
        gebruikersObject.x_forwarded_for = iislog.x_forwarded_for;

        herkomstObject.s_computername = iislog.s_computername;
        herkomstObject.datum = iislog.datum;
        herkomstObject.uur = iislog.uur;
        herkomstObject.x_forwarded_for = iislog.x_forwarded_for;

        herkomstObject.cs_referer = iislog.cs_referer;

        if (!lijstGebruikerObject.Contains(gebruikersObject)) {
          if (!(gebruikersObject.x_forwarded_for.CompareTo("-") == 0)) {
            lijstGebruikerObject.Add(gebruikersObject);
          }
        }
        if (!lijstHerkomstObject.Contains(herkomstObject)) {
          if (!(herkomstObject.cs_referer.CompareTo("-") == 0)) {
            lijstHerkomstObject.Add(herkomstObject);
          }
        }
      }
      foreach (GebruikerObject gebruikerObject in lijstGebruikerObject) {
        verwerkGebruiker(gebruikerObject);
      }

      foreach (HerkomstObject herkomstObject in lijstHerkomstObject) {
        verwerkHerkomst(herkomstObject);
      }
    }

    /**
     * Hier uiteindelijk de inserts doen naar de database
     */
    private void doeInsertUiteindelijkEind() {

      // Grote hoeveelheden eerst in interne tabellen verwerken

      voorverwerken();

      foreach (IISLogObject iislog in LijstIISLogObjecten) {
        foreach (string verwerk in DeFuncties.stuurBestand.teVerwerken) {
          int tellerConditie = 0;
          if (verwerk.Contains("+")) {
            string[] condities = verwerk.Split('+');
            foreach (string conditie in condities) {
              string voorIs = conditie.Split('=')[0];
              string naIs = conditie.Split('=')[1];
              if (voorIs.Contains("s_contentpath") && iislog.s_contentpath.Contains(naIs)) {
                tellerConditie++;
              }
              if (voorIs.Contains("cs_uri_query") && iislog.cs_uri_query.Contains(naIs)) {
                tellerConditie++;
              }
            }
            if (tellerConditie == condities.Length) {
              VerwerkAantalViewer(iislog);
            }
          }
        }
      }
    }

    /**
    * Openen van de database voor het lezen
    */
    internal void openenVerbindingLezen() {
      verbindingLezen = new NpgsqlConnection();
      // Om te voorkomen bij verkeerd wachtwoord lock op de database direct programma afsluiten bij Exception
      try {
        verbindingLezen.ConnectionString = bepaalConnectie();
        verbindingLezen.Open();
      } catch (Exception e) {
        DeFuncties.HuubLog("Foute NpgsqlConnection openenVerbindingLezen. Programma wordt afgebroken !!!!" + e.Message, false);
        DeFuncties.HuubLog("Foute NpgsqlConnection openenVerbindingLezen. Programma wordt afgebroken !!!!" + e.Message, true);
        System.Environment.Exit(-3295);
      }
    }

    private string bepaalConnectie() {
      String connectie = String.Format(DeFuncties.stuurBestand.ConnectString, wachtwoord);
      return connectie;
    }

    /**
* Openen van de database voor het lezen
* Zorg dat op een nieuw systeem het volgende is geinstalleerd om database te kunnen zien:
* https://marketplace.visualstudio.com/items?itemName=RojanskyS.NpgsqlPostgreSQLIntegration
* 
* Bij een foutmelding
* Kan bestand of assembly System.Threading.Tasks.Extensions
, Version=4.2.1.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51 
of een van de afhankelijkheden hiervan niet laden. 
Het systeem kan het opgegeven bestand niet vinden.
* 
* 
* Toevoegen aan VerwerkIISLogNaarDb3Onderdelen\VerwerkIISLogNaarDb3Onderdelen\VerwerkIISLogNaarDb3Onderdelen.csproj
* 
*   <Reference Include="System.Runtime.CompilerServices.Unsafe, Version=4.0.4.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL"> 
      <HintPath>..\packages\System.Runtime.CompilerServices.Unsafe.4.5.0\lib\netstandard2.0\System.Runtime.CompilerServices.Unsafe.dll</HintPath> 
    </Reference>

The type initializer for 'Npgsql.PoolManager' threw an exception
      uiteindelijk opgelost door naar een oudere versie te gaan van Npgsql 4.0.9

*/
    internal void openenVerbindingSchrijven() {
      verbindingSchrijven = new NpgsqlConnection();

      try {
        verbindingSchrijven.ConnectionString = bepaalConnectie();
        verbindingSchrijven.Open();
      } catch (Exception e) {
        DeFuncties.HuubLog("Foute NpgsqlConnection verbindingSchrijven. Programma wordt afgebroken !!!! " + e.Message, false);
        DeFuncties.HuubLog("Foute NpgsqlConnection verbindingSchrijven. Programma wordt afgebroken !!!! " + e.Message, true);
        // Om te voorkomen bij verkeerd wachtwoord lock op de database direct programma afsluiten bij Exception
        System.Environment.Exit(-3295);
      }
    }
    /**
     * sluiten van lezen database
     */
    internal void SluitenLezen() {
      try {
        reader.Close();
        verbindingLezen.Close();
      } catch (Exception e) {
        DeFuncties.HuubLog("Fout sluitenLezen " + e.Message, true);
        DeFuncties.HuubLog("Fout sluitenLezen " + e.Message, false);
      }

      reader = null;
      verbindingLezen = null;
    }
    /**
      * sluiten van de database
      */
    internal void sluitenSchrijven() {
      try {
        verbindingSchrijven.Close();
      } catch (Exception e) {
        DeFuncties.HuubLog("Fout sluitenSchrijven " + e.Message, true);
        DeFuncties.HuubLog("Fout sluitenSchrijven " + e.Message, false);
      }
      verbindingSchrijven = null;
    }
    /**
     * Hier lezen of de run al bestaat en zoniet insert anders update en tellen.
     */
    private void verwerkRuns(IISLogObject iislog) {
      if (bestaatCumulRuns(iislog)) {
        updateCumulRuns(iislog);
      } else {
        insertCumulRuns(iislog);
      }
    }
    /**
     * Hier lezen of het ip adres (x_forwarde_for) al bestaat en zoniet insert anders update en tellen.
     */
    private void verwerkGebruiker(GebruikerObject gebruikerObject) {
      if (bestaatGebruiker(gebruikerObject)) {
        updateGebruiker(gebruikerObject);
      } else {
        insertGebruiker(gebruikerObject);
      }
    }

    /**
     * Hier lezen of het ip adres (x_forwarde_for) al bestaat en zoniet insert anders update en tellen.
     */
    private void verwerkHerkomst(HerkomstObject herkomstObject) {
      if (bestaatHerkomst(herkomstObject)) {
        updateHerkomst(herkomstObject);
      } else {
        insertHerkomst(herkomstObject);
      }
    }
    /**
     * Hier lezen of de rij al bestaat en zoniet insert anders update en tellen.
     */
    private void VerwerkAantalViewer(IISLogObject iislog) {
      if (bestaatAantalViewer(iislog)) {
        updateAantalViewer(iislog);
      } else {
        insertAantalViewer(iislog);
      }
    }

    /**
     * Lezen true als bestaat anders false
     */
    private bool lezen(IISLogObject iislog, string sql, Soort soort) {
      openenVerbindingLezen();

      commando = new NpgsqlCommand(sql, verbindingLezen);

      string uur = iislog.uur;

      commando.Parameters.AddWithValue("@datum", iislog.datum);
      commando.Parameters.AddWithValue("@uur", Int32.Parse(uur));
      commando.Parameters.AddWithValue("@s_computername", iislog.s_computername);

      bepalenCommandParameters(iislog, soort);

      try {
        reader = commando.ExecuteReader();

        if (reader.Read()) {
          SluitenLezen();
          return true;
        }
      } catch (Exception e) {
        DeFuncties.HuubLog("Fout lezen " + e.Message, true);
        DeFuncties.HuubLog("Fout lezen " + e.Message, false);
      }

      SluitenLezen();

      return false;
    }


    /**
     * Algemeen schrijf gedeelte
     */
    private void schrijven(IISLogObject iislog, string sql, Soort soort) {
      openenVerbindingSchrijven();

      commando = new NpgsqlCommand(sql, verbindingSchrijven);

      string uur = iislog.uur;

      commando.Parameters.AddWithValue("@datum", iislog.datum);
      commando.Parameters.AddWithValue("@uur", Int32.Parse(uur));
      commando.Parameters.AddWithValue("@s_computername", iislog.s_computername);
      commando.Parameters.AddWithValue("@time_taken", iislog.time_taken);
      commando.Parameters.AddWithValue("@aantal", 1);

      bepalenCommandParameters(iislog, soort);

      try {
        long id = commando.ExecuteNonQuery();
      } catch (Exception e) {
        DeFuncties.HuubLog(String.Format("Fout in het schrijven : {0} bij computer : {1} datum : {2} uur: {3}", e.Message, iislog.s_computername, iislog.datum, uur), true);
        DeFuncties.HuubLog(String.Format("Fout in het schrijven : {0} bij computer : {1} datum : {2} uur: {3}", e.Message, iislog.s_computername, iislog.datum, uur), false);
      }

      // long id = commando.LastInsertedId; // MySQL

      sluitenSchrijven();
    }

    /**
     * Voor de meeste objecten zijn datum, uur en computernaam standaard. Hier de afwijkingen meegeven
     * aan de parameter van de query
     */
    private void bepalenCommandParameters(IISLogObject iislog, Soort soort) {
      switch (soort) {
        case Soort.runs:
        case Soort.aantalViewer:
          commando.Parameters.AddWithValue("@x_forwarded_for", iislog.x_forwarded_for);
          commando.Parameters.AddWithValue("@s_contentpath", iislog.s_contentpath); break;
        case Soort.gebruiker: commando.Parameters.AddWithValue("@x_forwarded_for", iislog.x_forwarded_for); break;
        case Soort.herkomst:
          commando.Parameters.AddWithValue("@cs_referer", iislog.cs_referer);
          commando.Parameters.AddWithValue("@x_forwarded_for", iislog.x_forwarded_for);
          break;
        case Soort.tijdsduur:
          commando.Parameters.AddWithValue("@cs_method", iislog.cs_method);
          commando.Parameters.AddWithValue("@sc_status", iislog.sc_status);
          commando.Parameters.AddWithValue("@s_contentpath", iislog.s_contentpath);
          break;
        default:
          DeFuncties.HuubLog("Fout schrijven soort kan niet bepaald worden !! ", true);
          DeFuncties.HuubLog("Fout schrijven soort kan niet bepaald worden !! ", false);
          break;
      }
    }
    /*
     ##########################################################################
    */

    /**
     * Check of het record al bestaat true indien waar anders false
     */
    private bool bestaatCumulRuns(IISLogObject iislog) {
      string sql = "select * from powerbi_cumul_runs where " +
                   "datum=@datum " +
                   "and uur=@uur " +
                   "and s_computername=@s_computername " +
                   "and s_contentpath=@s_contentpath";

      return lezen(iislog, sql, Soort.runs);
    }
    /**
     * Doe een insert
     */
    private void insertCumulRuns(IISLogObject iislog) {
      string sql = "insert into powerbi_cumul_runs(" +
        " s_computername" +
        ",datum" +
        ",uur" +
        ",s_contentpath" +
        ",aantal" +
        ") values (" +
        " @s_computername" +
        ",@datum" +
        ",@uur" +
        ",@s_contentpath" +
        ",1" +
        ")";

      schrijven(iislog, sql, Soort.runs);

    }
    /**
    * Hier een update en het aantal met 1 verhogen.
    */
    private void updateCumulRuns(IISLogObject iislog) {
      string sql = "update powerbi_cumul_runs set " +
        " aantal = aantal + 1" +
        " where " +
        "    s_computername = @s_computername" +
        " and datum = @datum" +
        " and uur = @uur" +
        " and s_contentpath = @s_contentpath" +
        ";"
        ;

      schrijven(iislog, sql, Soort.runs);
    }
    /*
     ##########################################################################
    */

    private bool bestaatGebruiker(GebruikerObject gebruikerObject) {
      string sql = "select * from powerbi_cumul_gebruiker where " +
                   "datum=@datum " +
                   "and uur=@uur " +
                   "and s_computername=@s_computername " +
                   "and x_forwarded_for=@x_forwarded_for";

      return lezen(gebruikerObject, sql, Soort.gebruiker);
    }
    private bool lezen(GebruikerObject gebruikerObject, string sql, Soort gebruiker) {
      IISLogObject iislog = new IISLogObject();

      iislog.s_computername = gebruikerObject.s_computername;
      iislog.datum = gebruikerObject.datum;
      iislog.uur = gebruikerObject.uur;
      iislog.x_forwarded_for = gebruikerObject.x_forwarded_for;

      return lezen(iislog, sql, gebruiker);
    }

    private void insertGebruiker(GebruikerObject gebruikerObject) {
      string sql = "insert into powerbi_cumul_gebruiker(" +
        " s_computername" +
        ",datum" +
        ",uur" +
        ",x_forwarded_for" +
        ",aantal" +
        ") values (" +
        " @s_computername" +
        ",@datum" +
        ",@uur" +
        ",@x_forwarded_for" +
        ",1" +
        ")";

      schrijven(gebruikerObject, sql, Soort.gebruiker);
    }

    private void schrijven(GebruikerObject gebruikerObject, string sql, Soort gebruiker) {
      IISLogObject iislog = new IISLogObject();

      iislog.s_computername = gebruikerObject.s_computername;
      iislog.datum = gebruikerObject.datum;
      iislog.uur = gebruikerObject.uur;
      iislog.x_forwarded_for = gebruikerObject.x_forwarded_for;

      schrijven(iislog, sql, gebruiker);
    }

    private void updateGebruiker(GebruikerObject gebruikerObject) {
      string sql = "update powerbi_cumul_gebruiker set " +
        " aantal = aantal + 1" +
        " where " +
        "    s_computername = @s_computername" +
        " and datum = @datum" +
        " and uur = @uur" +
        " and x_forwarded_for = @x_forwarded_for" +
        ";"
        ;

      schrijven(gebruikerObject, sql, Soort.gebruiker);
    }
    /*
    ##########################################################################
     */

    private bool bestaatHerkomst(HerkomstObject herkomstObject) {
      string sql = "select * from powerbi_cumul_herkomst where " +
                   "datum=@datum " +
                   "and uur=@uur " +
                   "and s_computername=@s_computername " +
                   "and cs_referer=@cs_referer " +
                   "and x_forwarded_for=@x_forwarded_for";

      return lezen(herkomstObject, sql, Soort.herkomst);
    }

    private bool lezen(HerkomstObject herkomstObject, string sql, Soort herkomst) {
      IISLogObject iislog = new IISLogObject();

      iislog.s_computername = herkomstObject.s_computername;
      iislog.datum = herkomstObject.datum;
      iislog.uur = herkomstObject.uur;
      iislog.cs_referer = herkomstObject.cs_referer;
      iislog.x_forwarded_for = herkomstObject.x_forwarded_for;

      return lezen(iislog, sql, herkomst);
    }

    private void insertHerkomst(HerkomstObject herkomstObject) {
      string sql = "insert into powerbi_cumul_herkomst(" +
        " s_computername" +
        ",datum" +
        ",uur" +
        ",cs_referer" +
        ",x_forwarded_for" +
        ",aantal" +
        ") values (" +
        " @s_computername" +
        ",@datum" +
        ",@uur" +
        ",@cs_referer" +
        ",@x_forwarded_for" +
        ",1" +
        ")";

      schrijven(herkomstObject, sql, Soort.herkomst);
    }

    private void schrijven(HerkomstObject herkomstObject, string sql, Soort herkomst) {
      IISLogObject iislog = new IISLogObject();

      iislog.s_computername = herkomstObject.s_computername;
      iislog.datum = herkomstObject.datum;
      iislog.uur = herkomstObject.uur;
      iislog.x_forwarded_for = herkomstObject.x_forwarded_for;
      iislog.cs_referer = herkomstObject.cs_referer;

      schrijven(iislog, sql, herkomst);
    }

    private void updateHerkomst(HerkomstObject herkomstObject) {
      string sql = "update powerbi_cumul_herkomst set " +
        " aantal = aantal + 1" +
        " where " +
        "    s_computername = @s_computername" +
        " and datum = @datum" +
        " and uur = @uur" +
        " and x_forwarded_for = @x_forwarded_for" +
        " and cs_referer = @cs_referer" +
        ";"
        ;

      schrijven(herkomstObject, sql, Soort.herkomst);
    }
    /*
     ##########################################################################
    */
    private bool bestaatAantalViewer(IISLogObject iislog) {
      string sql = "select * from powerbi_cumul_aantal_viewer where " +
                   "datum=@datum " +
                   "and uur=@uur " +
                   "and s_computername=@s_computername " +
                   "and x_forwarded_for=@x_forwarded_for " +
                   "and s_contentpath=@s_contentpath";

      return lezen(iislog, sql, Soort.aantalViewer);
    }

    private void insertAantalViewer(IISLogObject iislog) {
      string sql = "insert into powerbi_cumul_aantal_viewer(" +
        " s_computername" +
        ",datum" +
        ",uur" +
        ",s_contentpath" +
        ",x_forwarded_for" +
        ",aantal" +
        ") values (" +
        " @s_computername" +
        ",@datum" +
        ",@uur" +
        ",@s_contentpath" +
        ",@x_forwarded_for" +
        ",1" +
        ")";

      schrijven(iislog, sql, Soort.aantalViewer);
    }

    private void updateAantalViewer(IISLogObject iislog) {
      string sql = "update powerbi_cumul_aantal_viewer set " +
        " aantal = aantal + 1" +
        " where " +
        "    s_computername = @s_computername" +
        " and datum = @datum" +
        " and uur = @uur" +
        " and s_contentpath = @s_contentpath" +
        " and x_forwarded_for = @x_forwarded_for" +
        ";"
        ;

      schrijven(iislog, sql, Soort.aantalViewer);
    }
  }
}